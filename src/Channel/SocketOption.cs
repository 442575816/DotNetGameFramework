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
    }
}
