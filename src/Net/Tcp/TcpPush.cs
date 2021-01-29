using System;
using System.Text;

namespace DotNetGameFramework
{
    public class TcpPush : Push
    {
        /// <summary>
        /// 内部Channel
        /// </summary>
        private Channel channel;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="channel"></param>
        public TcpPush(Channel channel)
        {
            this.channel = channel;
        }

        /// <inheritdoc/>
        public ServerProtocol Protocol => ServerProtocol.TCP;

        /// <inheritdoc/>
        public void Discard()
        {
            return;
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
            return channel == null ? false : channel.IsConnected;
        }

        /// <inheritdoc/>
        public void Push(string command, byte[] bytes)
        {
            byte[] commandBytes = Encoding.UTF8.GetBytes(command);
            byte[] expandCommandBytes = new byte[32];
            Array.Copy(commandBytes, expandCommandBytes, commandBytes.Length);

            ByteBuf buff = WrapperUtil.ByteBufPool.Allocate();
            buff.WriteInt(36 + bytes.Length);
            buff.WriteBytes(expandCommandBytes);
            buff.WriteInt(0);
            buff.WriteBytes(bytes);

            Push(buff);
        }

        /// <inheritdoc/>
        public void Push(ByteBuf buffer)
        {
            channel?.Write(buffer);
        }
    }
}
