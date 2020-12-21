using System;
using System.Collections.Generic;

namespace DotNetGameFramework
{
    /// <summary>
    /// Push 推送通道
    /// </summary>
    public interface Push : IDisposable
    {
        /// <summary>
        /// 推送数据
        /// </summary>
        /// <param name="command"></param>
        /// <param name="bytes"></param>
        void Push(string command, byte[] bytes);

        /// <summary>
        /// 推送数据
        /// </summary>
        /// <param name="buffer"></param>
        void Push(ByteBuf buffer);

        /// <summary>
        /// 是否可以推送
        /// </summary>
        /// <returns></returns>
        bool IsPushable();

        /// <summary>
        /// 丢弃该推送通道
        /// </summary>
        void Discard();

        /// <summary>
        /// 心跳，维持推送通道存活
        /// </summary>
        void Heartbeat();

        /// <summary>
        /// 通讯协议
        /// </summary>
        ServerProtocol Protocol { get; }
    }
}
