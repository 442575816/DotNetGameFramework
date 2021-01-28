using System;
using System.Text;

namespace DotNetGameFramework
{
    public class HttpPush : Push
    {
        /// <summary>
        /// 内部Channel
        /// </summary>
        private Channel channel;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="channel"></param>
        public HttpPush(Channel channel)
        {
            this.channel = channel;
        }

        /// <inheritdoc/>
        public ServerProtocol Protocol => ServerProtocol.HTTP;

        /// <inheritdoc/>
        public void Discard()
        {
            Dispose();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            channel?.Close();
        }

        /// <inheritdoc/>
        public void Heartbeat()
        {
            return;
        }

        /// <inheritdoc/>
        public bool IsPushable()
        {
            return false;
        }

        /// <inheritdoc/>
        public void Push(string command, byte[] bytes)
        {
            throw new NotSupportedException("http not support push function");
        }

        /// <inheritdoc/>
        public void Push(ByteBuf buffer)
        {
            throw new NotSupportedException("http not support push function");
        }
    }
}
