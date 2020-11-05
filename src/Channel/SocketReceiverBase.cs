using System;
using System.IO.Pipelines;
using System.Net.Sockets;

namespace DotNetGameFramework
{
    internal abstract class SocketSenderReceiverBase : IDisposable
    {
        // socket
        protected readonly Socket _socket;
        // socketEventArgs
        protected readonly SocketAwaitableEventArgs _awaitableEventArgs;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="scheduler"></param>
        protected SocketSenderReceiverBase(Socket socket, PipeScheduler scheduler)
        {
            _socket = socket;
            _awaitableEventArgs = new SocketAwaitableEventArgs(scheduler);
        }

        public void Dispose() => _awaitableEventArgs.Dispose();
    }
}
