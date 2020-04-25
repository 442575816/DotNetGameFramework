using System;

namespace DotNetTcpFramework
{
    public interface ChannelFuture
    {
        /// <summary>
        /// 添加监听器
        /// </summary>
        /// <typeparam name="F"></typeparam>
        /// <param name="listener"></param>
        /// <returns></returns>
        ChannelFuture AddListener<F>(FutureListener<F> listener) where F : ChannelFuture;


        /// <summary>
        /// 移除监听器
        /// </summary>
        /// <typeparam name="F"></typeparam>
        /// <param name="listener"></param>
        /// <returns></returns>
        ChannelFuture RemoveListener<F>(FutureListener<F> listener) where F : ChannelFuture;

        /// <summary>
        /// 同步执行
        /// </summary>
        /// <returns></returns>
        ChannelFuture Sync();

        /// <summary>
        /// 等待
        /// </summary>
        /// <returns></returns>
        ChannelFuture Await();
    }
}
