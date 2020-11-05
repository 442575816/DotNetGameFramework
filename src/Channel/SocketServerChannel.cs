using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace DotNetGameFramework
{
    public class SocketServerChannel
    {
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
        private SocketAsyncEventArgs receiveEventArgs;

        /// <summary>
        /// 发送请求
        /// </summary>
        //private SocketAsyncEventArgs sendEventArgs;

        /// <summary>
        /// 接受buffpool
        /// </summary>
        private ByteBufPool<ByteBuf> buffPool;

        /// <summary>
        /// 发送队列
        /// </summary>
        private ConcurrentQueue<ByteBuf> sendQueue = new ConcurrentQueue<ByteBuf>();

        /// <summary>
        /// 是否正在接受数据中
        /// </summary>
        private bool isReceiving = false;

        /// <summary>
        /// 是否正在发送数据
        /// </summary>
        private bool isSending = false;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="socketServer"></param>
        public SocketServerChannel(SocketServer socketServer)
        {
            Id = Guid.NewGuid().ToString();
            SocketServer = socketServer;
            ChannelPipeline = new DefaultChannelPipeline(this);
            buffPool = new ByteBufPool<ByteBuf>(() =>
            {
                return new ByteBuf(256);
            }, 10, 20);
        }

        /// <summary>
        /// 新的连接
        /// </summary>
        /// <param name="socket"></param>
        public void Connect(Socket socket)
        {
            Socket = socket;

            receiveEventArgs = new SocketAsyncEventArgs();
            receiveEventArgs.Completed += OnAsyncCompleted;

            //sendEventArgs = new SocketAsyncEventArgs();
            //sendEventArgs.Completed += OnAsyncCompleted;
            IsConnected = true;

            ChannelPipeline.FireChannelActive();

            TryReceive();
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
            TrySend();
        }


        /// <summary>
        /// 尝试接受数据
        /// </summary>
        private void TryReceive()
        {
            if (isReceiving)
            {
                return;
            }

            if (!IsConnected)
            {
                return;
            }

            isReceiving = true;
            ByteBuf buff = buffPool.Allocate();
            buff.Deallocate = buffPool.Free;
            receiveEventArgs.SetBuffer(buff.Data, buff.WriterIndex, buff.WritableBytes);
            receiveEventArgs.UserToken = buff;
            if (!Socket.ReceiveAsync(receiveEventArgs))
            {
                if (OnReceive(receiveEventArgs))
                {
                    TryReceive();
                }
            }
        }

        /// <summary>
        /// 准备发送数据
        /// </summary>
        private void TrySend()
        {
            //if (isSending)
            //{
            //    return;
            //}

            if (!IsConnected)
            {
                return;
            }

            if (sendQueue.Count == 0)
            {
                return;
            }


            if (sendQueue.TryDequeue(out ByteBuf buff))
            {
                //isSending = true;
                SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
                sendEventArgs.Completed += OnAsyncCompleted;
                sendEventArgs.SetBuffer(buff.Data, buff.ReaderIndex, buff.ReadableBytes);
                sendEventArgs.UserToken = buff;
                if (!Socket.SendAsync(sendEventArgs))
                {
                    OnSend(sendEventArgs);
                }
            }
        }


        private void OnAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    if (OnReceive(e))
                    {
                        TryReceive();
                    }
                    else
                    {
                        Console.Write("");
                    }
                    break;
                case SocketAsyncOperation.Send:
                    if (OnSend(e))
                    {
                        TrySend();
                    }
                    break;
            }
        }

        /// <summary>
        /// 处理数据接受
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool OnReceive(SocketAsyncEventArgs e)
        {
            if (!IsConnected)
            {
                return false;
            }

            int size = e.BytesTransferred;
            if (size > 0)
            {
                // Notify Receive Data

            }

            isReceiving = false;

            if (e.SocketError == SocketError.Success)
            {
                if (size > 0)
                {
                    ByteBuf buff = (e.UserToken as ByteBuf);
                    buff.WriterIndex += size;
                    buff.Retain();
                    ChannelPipeline.FireChannelRead(e.UserToken);
                    return true;
                }
                else
                {
                    Disconnect();
                }
            }
            else
            {
                ChannelPipeline.FireExceptionCaught(new Exception(e.SocketError.ToString()));
            }

            return false;
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool OnSend(SocketAsyncEventArgs e)
        {
            if (!IsConnected)
            {
                return false;
            }

            if (e.SocketError == SocketError.IOPending || e.SocketError == SocketError.Success)
            {
                byte[] data = e.Buffer;
                if (e.BytesTransferred < e.Count)
                {
                    // 未传输完毕
                    e.SetBuffer(data, e.Offset + e.BytesTransferred, e.Count - e.BytesTransferred);
                    if (!Socket.SendAsync(e))
                    {
                        return OnSend(e);
                    }
                    return false;
                }
                else
                {
                    isSending = false;
                    return true;
                }
            }
            else
            {
                isSending = false;
                ChannelPipeline.FireExceptionCaught(new Exception(e.SocketError.ToString()));
                Disconnect();
                return false;
            }


        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public bool Disconnect()
        {
            if (!IsConnected)
            {
                return false;
            }

            // Reset EventArgs
            receiveEventArgs.Completed -= OnAsyncCompleted;
            //sendEventArgs.Completed -= OnAsyncCompleted;

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

            // 设置标志位
            IsConnected = false;

            // reset 
            isReceiving = false;

            ChannelPipeline.FireChannelInactive();

            SocketServer.UnregisteredChannel(this);

            ChannelPipeline.FireChannelUnregistered();

            return true;
        }
    }
}