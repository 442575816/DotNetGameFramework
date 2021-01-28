using System;

namespace DotNetGameFramework
{
    public interface Channel
    {
        /// <summary>
        /// ChannelId
        /// </summary>
        string Id { get; }

        /// <summary>
        /// 是否连接中
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 写消息
        /// </summary>
        /// <param name="msg"></param>
        ChannelFuture Write(object msg);

        /// <summary>
        /// 等待数据发送完毕后关闭链接
        /// </summary>
        void Close();

        /// <summary>
        /// 关闭链接
        /// </summary>
        /// <returns></returns>
        bool Shutdown();
    }
}
