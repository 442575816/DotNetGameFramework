using System;

namespace DotNetGameFramework
{
    /// <summary>
    /// Socket参数
    /// </summary>
    public class SocketOption
    {
        /// <summary>
        /// IO线程数量
        /// </summary>
        public int IOQueueCount { get; set; } = Math.Min(Environment.ProcessorCount, 16);

        /// <summary>
        /// 分配Buff之前先等待数据
        /// </summary>
        public bool WaitForDataBeforeAllocatingBuffer { get; set; } = true;

        /// <summary>
        /// 等待连接的队列长度
        /// </summary>
        public int Backlog { get; set; } = 512;

        /// <summary>
        /// 不启用Nagle算法，立即发送数据
        /// </summary>
        public bool NoDelay { get; set; } = true;

        /// <summary>
        /// 重用端口
        /// </summary>
        public bool ReusePort { get; set; } = true;

        /// <summary>
        /// 默认ByteBuf分配大小
        /// </summary>
        public int DefaultByteBufSize { get; set; } = 256;

        /// <summary>
        /// 默认ByteBuffSize
        /// </summary>
        public int MinByteBufPoolSize { get; set; } = 10;

        /// <summary>
        /// 默认ByteBuffSize
        /// </summary>
        public int MaxByteBufPoolSize { get; set; } = 50;
    }
}
