using System;
using System.Collections.Generic;

namespace DotNetGameFramework
{
    public class TcpResponse : Response
    {
        /// <summary>
        /// 内部Channel
        /// </summary>
        private SocketServerChannel channel;

        /// <summary>
        /// 关闭标识
        /// </summary>
        private bool closeFlag;

        /// <inheritdoc/>
        public byte[] Content => throw new NotImplementedException();

        /// <inheritdoc/>
        public int HttpStatus { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <inheritdoc/>
        public ServerProtocol Protocol => ServerProtocol.TCP;

        /// <inheritdoc/>
        public void AddHeader(string name, string value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Dictionary<string, string> GetHeaders()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Write(object obj)
        {
            channel.Write(obj);
            if (closeFlag)
            {
                channel.Close();
            }
        }
    }
}
