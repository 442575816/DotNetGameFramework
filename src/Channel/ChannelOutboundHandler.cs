using System;
namespace DotNetGameFramework
{
    /// <summary>
    /// Channel输出处理器
    /// </summary>
    public interface ChannelOutboundHandler : ChannelHandler
    {
        /// <summary>
        /// 写数据
        /// </summary>
        /// <returns></returns>
        ChannelFuture Write(object msg);
    }
}
