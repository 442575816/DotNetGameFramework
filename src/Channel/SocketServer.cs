using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

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
        /// 默认Options
        /// </summary>
        public SocketOption Options { get; set; } = new SocketOption();

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
        /// 构造函数
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public SocketServer(string host, string port)
        {
            this.host = host;
            this.port = port;

        }

        /// <summary>
        /// 启动Server Socket
        /// </summary>
        public void Start()
        {
            if (IsStarted)
            {
                return;
            }

            // 构建化SocketAsyncEventArgs
            acceptorEventArgs = new SocketAsyncEventArgs();
            acceptorEventArgs.Completed += OnAcceptCompleted;

            // 启动Socket
            IPEndPoint.TryParse($"{host}:{port}", out IPEndPoint endpoint);
            serverSocket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, Options.ReusePort);
            serverSocket.NoDelay = Options.NoDelay;
            serverSocket.Bind(endpoint);
            serverSocket.Listen(1000);

            IsStarted = true;
            IsAccepting = true;
            StartAccept(acceptorEventArgs);
        }

        /// <summary>
        /// 停止Server Socket
        /// </summary>
        public void Stop()
        {
            if (!IsStarted)
            {
                return;
            }

            // 关闭请求接受
            IsAccepting = false;

            // 移除事件监听
            acceptorEventArgs.Completed -= OnAcceptCompleted;

            // 关闭Socket
            serverSocket.Close();

            // 关闭所有连接
            DisconnectAll();

            // 启动标志设置为false
            IsStarted = false;
        }

        /// <summary>
        /// 断开所有连接
        /// </summary>
        public void DisconnectAll()
        {
            if (!IsStarted)
            {
                return;
            }

            foreach (var channel in channels.Values)
            {
                channel.Disconnect();
            }
              
        }

        /// <summary>
        /// 开始接受请求
        /// </summary>
        /// <param name="eventArgs"></param>
        private void StartAccept(SocketAsyncEventArgs eventArgs)
        {
            if (!IsAccepting)
            {
                return;
            }

            // 释放上次绑定的Socket，等待下一个Socket连接
            eventArgs.AcceptSocket = null;

            if (!serverSocket.AcceptAsync(eventArgs))
            {
                OnAcceptCompleted(null, eventArgs);
            }

        }

        /// <summary>
        /// 连接建立完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // 新的连接建立成功
                ChannelConnected(e.AcceptSocket);
            }
            else
            {
                ErrorCaught(e.SocketError);
            }

            StartAccept(e);
        }

        /// <summary>
        /// Channel建立
        /// </summary>
        /// <param name="socket"></param>
        protected void ChannelConnected(Socket socket)
        {
            var channel = new SocketServerChannel(this);
            ServerHandler?.InitChannel(channel);

            channel.Connect(socket);
            RegisteredChannel(channel);

            channel.ChannelPipeline.FireChannelRegistered();
        }

        /// <summary>
        /// 发送了异常
        /// </summary>
        /// <param name="ErrorCaught">Socket error code</param>
        internal void ErrorCaught(SocketError error)
        {
            // Skip disconnect errors
            if ((error == SocketError.ConnectionAborted) ||
                (error == SocketError.ConnectionRefused) ||
                (error == SocketError.ConnectionReset) ||
                (error == SocketError.OperationAborted) ||
                (error == SocketError.Shutdown))
                return;

            // DoNothing
            ServerHandler?.FireExceptionCaught(new Exception(error.ToString()));
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