using System;
using System.IO.Pipelines;
using System.Net.Sockets;

namespace DotNetGameFramework
{
    /// <summary>
    /// Socket消息接受者
    /// </summary>
    internal sealed class SocketReceiver : SocketSenderReceiverBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="scheduler"></param>
        public SocketReceiver(Socket socket, PipeScheduler scheduler) : base(socket, scheduler)
        {
        }

        /// <summary>
        /// 等待接收数据
        /// </summary>
        /// <returns></returns>
        public SocketAwaitableEventArgs WaitForDataAsync()
        {
            _awaitableEventArgs.SetBuffer(Memory<byte>.Empty);
            if (!_socket.ReceiveAsync(_awaitableEventArgs))
            {
                _awaitableEventArgs.Complete();
            }

            return _awaitableEventArgs;
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        public SocketAwaitableEventArgs ReceiveAsync(ByteBuf buff)
        {
            _awaitableEventArgs.SetBuffer(buff.Data, buff.WriterIndex, buff.WritableBytes);
            _awaitableEventArgs.UserToken = buff;

            if (!_socket.ReceiveAsync(_awaitableEventArgs))
            {
                _awaitableEventArgs.Complete();
            }

            return _awaitableEventArgs;
        }

        /// <summary>
        /// 接受数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public SocketAwaitableEventArgs ReceiveAsync(Memory<byte> buffer)
        {
            _awaitableEventArgs.SetBuffer(buffer);
            if (!_socket.ReceiveAsync(_awaitableEventArgs))
            {
                _awaitableEventArgs.Complete();
            }

            return _awaitableEventArgs;
        }

    }
}
