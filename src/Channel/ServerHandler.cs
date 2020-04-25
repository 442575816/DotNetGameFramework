using System;

namespace DotNetGameFramework
{
    public interface ServerHandler
    {
        /// <summary>
        /// 初始化Channel
        /// </summary>
        /// <param name="channel"></param>
        void InitChannel(SocketServerChannel channel);

        /// <summary>
        /// 触发异常
        /// </summary>
        /// <param name="e"></param>
        void FireExceptionCaught(Exception e);
    }
}
