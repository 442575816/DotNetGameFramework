using System;

namespace DotNetGameFramework
{
    /// <summary>
    /// Channel管道
    /// </summary>
    public interface ChannelPipeline
    {
        /// <summary>
        /// Channel
        /// </summary>
        SocketServerChannel Channel { get; }

        /// <summary>
        /// 添加Handler
        /// </summary>
        /// <param name="name"></param>
        /// <param name="handler"></param>
        void AddFirst(string name, ChannelHandler handler);

        /// <summary>
        /// 添加Handler
        /// </summary>
        /// <param name="name"></param>
        /// <param name="handler"></param>
        void AddLast(string name, ChannelHandler handler);

        /// <summary>
        /// 移除Handler
        /// </summary>
        /// <param name="name"></param>
        void Remove(string name);

        /// <summary>
        /// 移除Handler
        /// </summary>
        /// <param name="handler"></param>
        void Remove(ChannelHandler handler);

        /// <summary>
        /// 替换Handler
        /// </summary>
        /// <param name="oldHandler"></param>
        /// <param name="newName"></param>
        /// <param name="newHandler"></param>
        void Replace(ChannelHandler oldHandler, string newName, ChannelHandler newHandler);

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
