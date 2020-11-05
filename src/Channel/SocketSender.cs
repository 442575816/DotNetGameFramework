using System;
using System.IO.Pipelines;
using System.Net.Sockets;

namespace DotNetGameFramework
{
    /// <summary>
    /// Socket消息发送者
    /// </summary>
    internal sealed class SocketSender : SocketSenderReceiverBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="scheduler"></param>
        public SocketSender(Socket socket, PipeScheduler scheduler) : base(socket, scheduler)
        {
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        public SocketAwaitableEventArgs SendAsync(ByteBuf buff)
        {
            _awaitableEventArgs.SetBuffer(buff.Data, buff.ReaderIndex, buff.ReadableBytes);
            _awaitableEventArgs.UserToken = buff;
      
            if (!_socket.SendAsync(_awaitableEventArgs))
            {
                _awaitableEventArgs.Complete();
            }

            return _awaitableEventArgs;
        }


    }
}
