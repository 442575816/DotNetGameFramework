using System;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetGameFramework
{
    public class SocketServer
    {
        /// <summary>
        /// IP地址
        /// </summary>
        protected readonly string host;

        /// <summary>
        /// 端口号
        /// </summary>
        protected readonly string port;

        /// <summary>
        /// socket
        /// </summary>
        protected Socket serverSocket;

        /// <summary>
        /// channel列表
        /// </summary>
        protected readonly ConcurrentDictionary<string, SocketServerChannel> channels = new ConcurrentDictionary<string, SocketServerChannel>();

        /// <summary>
        /// ServerHandler
        /// </summary>
        public ServerHandler ServerHandler { get; set; }

        /// <summary>
        /// 是否开始接受请求
        /// </summary>
        protected bool IsAccepting { get; private set; } = false;

        /// <summary>
        /// 启动标识
        /// </summary>
        protected bool IsStarted { get; private set; } = false;

        /// <summary>
        /// 异步请求接受SocketAsyncEventArgs
        /// </summary>
        protected SocketAsyncEventArgs acceptorEventArgs = null;

        /// <summary>
        /// Socket参数
        /// </summary>
        public SocketOption SocketOptions { get; }

        /// <summary>
        /// 线程池相关
        /// </summary>
        private readonly int _numSchedulers;
        private readonly PipeScheduler[] _schedulers;
        private int _schedulerIndex;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public SocketServer(string host, string port, SocketOption socketOption)
        {
            this.host = host;
            this.port = port;
            SocketOptions = socketOption;

            var ioQueueCount = SocketOptions.IOQueueCount;
            if (ioQueueCount > 0)
            {
                _numSchedulers = ioQueueCount;
                _schedulers = new IOQueue[_numSchedulers];

                for (var i = 0; i < _numSchedulers; i++)
                {
                    _schedulers[i] = new IOQueue();
                }
            }
            else
            {
                var directScheduler = new PipeScheduler[] { PipeScheduler.ThreadPool };
                _numSchedulers = directScheduler.Length;
                _schedulers = directScheduler;
            }
        }

        /// <summary>
        /// 启动Server Socket
        /// </summary>
        public void Bind()
        {
            if (IsStarted)
            {
                return;
            }

            // 启动Socket
            IPEndPoint.TryParse($"{host}:{port}", out IPEndPoint endpoint);
            serverSocket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, SocketOptions.ReusePort);
            serverSocket.NoDelay = SocketOptions.NoDelay;
            serverSocket.Bind(endpoint);
            serverSocket.Listen(SocketOptions.Backlog);

            IsStarted = true;
        }

        public void AcceptAsync()
        {
            //Console.WriteLine("Main thread: {0}", Thread.CurrentThread.ManagedThreadId);
            AcceptInternalAsync();
        }

        private async void AcceptInternalAsync()
        {
            while (true)
            {
                try
                {
                    //Console.WriteLine("Accept thread1: {0}", Thread.CurrentThread.ManagedThreadId);
                    var acceptSocket = await serverSocket.AcceptAsync();
                    //Console.WriteLine("Accept thread2: {0}", Thread.CurrentThread.ManagedThreadId);
                    // Only apply no delay to Tcp based endpoints
                    if (acceptSocket.LocalEndPoint is IPEndPoint)
                    {
                        acceptSocket.NoDelay = SocketOptions.NoDelay;
                    }

                    var connection = new SocketServerChannel(acceptSocket, this, SocketOptions.MemoryPool,
                                                             _schedulers[_schedulerIndex],
                                                             SocketOptions.WaitForDataBeforeAllocatingBuffer);

                    //Console.WriteLine("Accept thread3: {0}", Thread.CurrentThread.ManagedThreadId);
                    ChannelConnected(connection);
                    connection.Start();
                    //Console.WriteLine("Accept thread4: {0}", Thread.CurrentThread.ManagedThreadId);

                    _schedulerIndex = (_schedulerIndex + 1) % _numSchedulers;
                }
                catch (ObjectDisposedException e)
                {
                    // A call was made to UnbindAsync/DisposeAsync just return null which signals we're done
                    ErrorCaught(e);
                    return;
                }
                catch (SocketException e) when (e.SocketErrorCode == SocketError.OperationAborted)
                {
                    // A call was made to UnbindAsync/DisposeAsync just return null which signals we're done
                    ErrorCaught(e);
                    return;
                }
                catch (SocketException e)
                {
                    // The connection got reset while it was in the backlog, so we try again.
                    ErrorCaught(e);
                    return;
                }
            }
        }

        /// <summary>
        /// 取消绑定
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ValueTask UnbindAsync(CancellationToken cancellationToken = default)
        {
            serverSocket?.Dispose();

            // 关闭所有连接
            ShutdownAll();

            // 启动标志设置为false
            IsStarted = false;

            return default;
        }

        /// <summary>
        /// 销毁SocketServer
        /// </summary>
        /// <returns></returns>
        public ValueTask DisposeAsync()
        {
            serverSocket?.Dispose();

            // 关闭所有连接
            ShutdownAll();

            // 启动标志设置为false
            IsStarted = false;

            return default;
        }

        /// <summary>
        /// 断开所有连接
        /// </summary>
        public void ShutdownAll()
        {
            if (!IsStarted)
            {
                return;
            }

            foreach (var channel in channels.Values)
            {
                channel.Shutdown();
            }
              
        }

        /// <summary>
        /// Channel建立
        /// </summary>
        /// <param name="socket"></param>
        protected void ChannelConnected(SocketServerChannel channel)
        {
           
            ServerHandler?.InitChannel(channel);
            RegisteredChannel(channel);

            channel.ChannelPipeline.FireChannelRegistered();
        }

        /// <summary>
        /// 发送了异常
        /// </summary>
        /// <param name="ErrorCaught">Socket error code</param>
        internal void ErrorCaught(Exception error)
        {
            // Skip disconnect errors
            if (error as SocketException != null)
            {
                SocketError socketError = (error as SocketException).SocketErrorCode;
                if ((socketError == SocketError.ConnectionAborted) ||
                    (socketError == SocketError.ConnectionRefused) ||
                    (socketError == SocketError.ConnectionReset) ||
                    (socketError == SocketError.OperationAborted) ||
                    (socketError == SocketError.Shutdown))
                {
                    return;
                }
            }
           
            // DoNothing
            ServerHandler?.FireExceptionCaught(error);
        }

        /// <summary>
        /// 添加Channel
        /// </summary>
        /// <param name="channel"></param>
        internal void RegisteredChannel(SocketServerChannel channel)
        {
            channels.TryAdd(channel.Id, channel);
        }

        /// <summary>
        /// 移除Channel
        /// </summary>
        /// <param name="channel"></param>
        internal void UnregisteredChannel(SocketServerChannel channel)
        {
            channels.TryRemove(channel.Id, out _);
        }
    }
}