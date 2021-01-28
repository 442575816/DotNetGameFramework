using System;
using System.Collections.Generic;

namespace DotNetGameFramework
{
    public class TcpResponse : Response
    {
        /// <summary>
        /// 内部Channel
        /// </summary>
        private Channel channel;

        /// <summary>
        /// 关闭标识
        /// </summary>
        private bool closeFlag;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="channel"></param>
        public TcpResponse(Channel channel)
        {
            this.channel = channel;
        }

        /// <inheritdoc/>
        public object Content => throw new NotImplementedException();

        /// <inheritdoc/>
        public int HttpStatus { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <inheritdoc/>
        public ServerProtocol Protocol => ServerProtocol.TCP;

        /// <inheritdoc/>
        public Dictionary<string, string> Cookies => throw new NotImplementedException();

        /// <inheritdoc/>
        public Dictionary<string, string> Headers => throw new NotImplementedException();

        /// <inheritdoc/>
        public void AddCookie(string name, string value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void AddHeader(string name, string value)
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
