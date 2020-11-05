using System;
using System.Threading.Tasks;

namespace DotNetGameFramework
{
    public interface ChannelHandlerContext
    {
        /// <summary>
        /// 获取处理管道
        /// </summary>
        ChannelPipeline ChannelPipeline { get; }

        /// <summary>
        /// 获取当前Channel
        /// </summary>
        SocketServerChannel Channel { get; }

        /// <summary>
        /// ChannelHandler
        /// </summary>
        ChannelHandler ChannelHandler { get; }

        /// <summary>
        /// Channel注册时候触发
        /// </summary>
        /// <returns></returns>
        void FireChannelRegistered();

        /// <summary>
        /// Channel取消注册
        /// </summary>
        /// <returns></returns>
        void FireChannelUnregistered();

        /// <summary>
        /// Channel连接时候触发
        /// </summary>
        /// <returns></returns>
        void FireChannelActive();

        /// <summary>
        /// Channel关闭时候触发
        /// </summary>
        /// <returns></returns>
        void FireChannelInactive();

        /// <summary>
        /// Channel发生异常时候触发
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        void FireExceptionCaught(Exception e);

        /// <summary>
        /// Channel读取到消息时候触发
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        void FireChannelRead(object msg);

        /// <summary>
        /// 写数据
        /// </summary>
        /// <returns></returns>
        ChannelFuture Write(object msg);
    }
}
