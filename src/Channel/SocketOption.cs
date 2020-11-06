using System;

namespace DotNetGameFramework
{
    /// <summary>
    /// Socket参数
    /// </summary>
    public class SocketOption
    {
        /// <summary>
        /// 不启用Nagle算法，立即发送数据
        /// </summary>
        public bool NoDelay { get; set; } = true;

        /// <summary>
        /// 重用端口
        /// </summary>
        public bool ReusePort { get; set; } = true;

        /// <summary>
        /// 等待连接队列的长度
        /// </summary>
        public int Backlog { get; set; } = 1000;

        /// <summary>
        /// 默认ByteBufPool
        /// </summary>
        public ByteBufPool<ByteBuf> ByteBufPool { get; set; } = ByteBufHelper.DefaultByteBufPool;
    }
}
