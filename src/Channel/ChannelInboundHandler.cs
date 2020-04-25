using System;
namespace DotNetGameFramework
{
    /// <summary>
    /// Channel输入处理器
    /// </summary>
    public interface ChannelInboundHandler : ChannelHandler
    {
        /// <summary>
        /// Channel注册时候触发
        /// </summary>
        /// <returns></returns>
        void FireChannelRegistered(ChannelHandlerContext context);

        /// <summary>
        /// Channel取消注册
        /// </summary>
        /// <returns></returns>
        void FireChannelUnregistered(ChannelHandlerContext context);

        /// <summary>
        /// Channel连接时候触发
        /// </summary>
        /// <returns></returns>
        void FireChannelActive(ChannelHandlerContext context);

        /// <summary>
        /// Channel关闭时候触发
        /// </summary>
        /// <returns></returns>
        void FireChannelInactive(ChannelHandlerContext context);

        /// <summary>
        /// Channel发生异常时候触发
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        void FireExceptionCaught(ChannelHandlerContext context, Exception e);

        /// <summary>
        /// Channel读取到消息时候触发
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        void FireChannelRead(ChannelHandlerContext context, object msg);
    }
}
