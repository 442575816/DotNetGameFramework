using System;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DotNetGameFramework
{
    public class SocketServerChannel
    {
        // 判断平台属性
        private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// ChannelId
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// SocketServer
        /// </summary>
        public SocketServer SocketServer { get; }

        /// <summary>
        /// Socket
        /// </summary>
        public Socket Socket { get; private set; }

        /// <summary>
        /// 是否连接中
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// ChannelPipeline
        /// </summary>
        public ChannelPipeline ChannelPipeline { get; private set; }

        /// <summary>
        /// 接受请求
        /// </summary>
        private readonly SocketReceiver _socketReceiver;

        /// <summary>
        /// 发送请求
        /// </summary>
        private readonly SocketSender _socketSender;

        /// <summary>
        /// 接受buffpool
        /// </summary>
        private readonly ByteBufPool<ByteBuf> _bufPool;

        /// <summary>
        /// 分配buff之前先等待数据
        /// </summary>
        private readonly bool _waitForData;

        /// <summary>
        /// 发送队列
        /// </summary>
        private ConcurrentQueue<ByteBuf> sendQueue = new ConcurrentQueue<ByteBuf>();

        /// <summary>
        /// 处理的任务
        /// </summary>
        private Task _processingTask;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="socketServer"></param>
        public SocketServerChannel(Socket socket, SocketServer socketServer, ByteBufPool<ByteBuf> bufPool,
                                        PipeScheduler transportScheduler,
                                        bool useInlineSchedulers = false,
                                        bool waitForData = true)
        {
            Id = Guid.NewGuid().ToString();
            SocketServer = socketServer;
            ChannelPipeline = new DefaultChannelPipeline(this);
            _bufPool = bufPool;
            _waitForData = waitForData;

            // 初始化Socket相关
            Socket = socket;

            var awaiterScheduler = IsWindows ? transportScheduler : PipeScheduler.Inline;
            var applicationScheduler = PipeScheduler.ThreadPool;
            if (useInlineSchedulers)
            {
                transportScheduler = PipeScheduler.Inline;
                awaiterScheduler = PipeScheduler.Inline;
                applicationScheduler = PipeScheduler.Inline;
            }

            _socketReceiver = new SocketReceiver(socket, awaiterScheduler);
            _socketSender = new SocketSender(socket, awaiterScheduler);

            IsConnected = true;
        }

        /// <summary>
        /// 开始工作
        /// </summary>
        public void Start()
        {
            _processingTask = StartAsync();
        }

        private async Task StartAsync()
        {
            try
            {
                var receiveTask = DoReceive();
                var sendTask = DoSend();

                await receiveTask;
                await sendTask;

                _socketReceiver.Dispose();
                _socketSender.Dispose();
            } catch (Exception e)
            {
                ChannelPipeline.FireExceptionCaught(new Exception($"Unexpceted exception in {Id}", e));
            }
        }

        /// <summary>
        /// 开始接收任务
        /// </summary>
        /// <returns></returns>
        private async Task DoReceive()
        {
            Exception error = null;

            try
            {
                await ProcessReceives();
            }
            catch (SocketException ex) when (IsConnectionResetError(ex.SocketErrorCode))
            {
                // This could be ignored if _shutdownReason is already set.
                error = new Exception(ex.Message, ex);

            }
            catch (Exception ex)
                when ((ex is SocketException socketEx && IsConnectionAbortError(socketEx.SocketErrorCode)) ||
                       ex is ObjectDisposedException)
            {
                // This exception should always be ignored because _shutdownReason should be set.
                error = ex;

            }
            catch (Exception ex)
            {
                // This is unexpected.
                error = ex;
            }
            finally
            {
                // If Shutdown() has already bee called, assume that was the reason ProcessReceives() exited.
                ChannelPipeline.FireExceptionCaught(error);
                Shutdown();
            }

        }

        /// <summary>
        /// 处理信息接收
        /// </summary>
        /// <returns></returns>
        private async Task ProcessReceives()
        {
            while (true)
            {
                if (!IsConnected)
                {
                    break;
                }

                if (_waitForData)
                {
                    await _socketReceiver.WaitForDataAsync();
                }

                var buf = AllocByteBuf();
                var bytesReceived = await _socketReceiver.ReceiveAsync(buf);

                if (bytesReceived > 0)
                {
                    ByteBuf buff = (_socketReceiver.UserToken as ByteBuf);
                    buff.WriterIndex += bytesReceived;
                    buff.Retain();

                    // TODO 应用线程池处理消息接收
                    ChannelPipeline.FireChannelRead(buff);
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 处理消息发送
        /// </summary>
        /// <returns></returns>
        private async Task DoSend()
        {
            Exception error = null;

            try
            {
                await ProcessSends();
            }
            catch (SocketException ex) when (IsConnectionResetError(ex.SocketErrorCode))
            {
                error = new Exception(ex.Message, ex);
            }
            catch (Exception ex)
                when ((ex is SocketException socketEx && IsConnectionAbortError(socketEx.SocketErrorCode)) ||
                       ex is ObjectDisposedException)
            {
                // This should always be ignored since Shutdown() must have already been called by Abort().
                error = ex;
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                // If Shutdown() has already bee called, assume that was the reason ProcessReceives() exited.
                ChannelPipeline.FireExceptionCaught(error);
                Shutdown();
            }
        }

        /// <summary>
        /// 处理发送
        /// </summary>
        /// <returns></returns>
        private async Task ProcessSends()
        {
            while (true)
            {
                if (!IsConnected)
                {
                    break;
                }

                if (sendQueue.Count == 0)
                {
                    await Task.Yield();
                }

                if (sendQueue.TryDequeue(out ByteBuf buff))
                {
                    await _socketSender.SendAsync(buff);
                    buff.Release();
                }
                else
                {
                    await Task.Yield();
                }
            }
        }

        /// <summary>
        /// 分配ByteBuf
        /// </summary>
        /// <returns></returns>
        private ByteBuf AllocByteBuf()
        {
            var buf = _bufPool.Allocate();
            buf.Deallocate = _bufPool.Free;

            return buf;
        }

        /// <summary>
        /// 异步发送数据
        /// </summary>
        /// <param name="buf"></param>
        public void SendAsync(ByteBuf buf)
        {
            if (!IsConnected)
            {
                return;
            }

            if (buf.ReadableBytes == 0)
            {
                return;
            }

            sendQueue.Enqueue(buf);
        }


        /// <summary>
        /// 断开连接
        /// </summary>
        public bool Shutdown()
        {
            if (!IsConnected)
            {
                return false;
            }

            IsConnected = false;

            try
            {
                try
                {
                    Socket.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException) { }

                Socket.Close();
                Socket.Dispose();
            }
            catch (ObjectDisposedException) { }


            ChannelPipeline.FireChannelInactive();
            SocketServer.UnregisteredChannel(this);
            ChannelPipeline.FireChannelUnregistered();

            return true;
        }

        /// <summary>
        /// 是否是连接被重置的错误
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        private static bool IsConnectionResetError(SocketError errorCode)
        {
            return errorCode == SocketError.ConnectionReset ||
                   errorCode == SocketError.Shutdown ||
                   (errorCode == SocketError.ConnectionAborted && IsWindows);
        }

        /// <summary>
        /// 是否是连接被终止的错误
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        private static bool IsConnectionAbortError(SocketError errorCode)
        {
            // Calling Dispose after ReceiveAsync can cause an "InvalidArgument" error on *nix.
            return errorCode == SocketError.OperationAborted ||
                   errorCode == SocketError.Interrupted ||
                   (errorCode == SocketError.InvalidArgument && !IsWindows);
        }
    }
}