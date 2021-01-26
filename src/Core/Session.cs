using System;
using System.Collections.Generic;

namespace DotNetGameFramework
{
    /// <summary>
    /// session 创建代理
    /// </summary>
    /// <param name="sessionEvent"></param>
    public delegate void SessionHandler(SessionEventType eventType, SessionEvent sessionEvent);

    /// <summary>
    /// session attribute变化handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="attributeEvent"></param>
    public delegate void SessionAttributeHandler(SessionAttributeEventType eventType, SessionAttributeEvent attributeEvent);

    /// <summary>
    /// Session
    /// </summary>
    public interface Session : IDisposable
    {
        /// <summary>
        /// SessionId，具体标识
        /// </summary>
        string Id { get; }

        /// <summary>
        /// 获取指定Attribute
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object GetAttribute(string key);

        /// <summary>
        /// 设置Attribute
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetAttribute(string key, object value);

        /// <summary>
        /// 移除指定Attribute
        /// </summary>
        /// <param name="key"></param>
        void RemoveAttribute(string key);

        /// <summary>
        /// 访问Session
        /// </summary>
        void Access();

        /// <summary>
        /// 是否合法
        /// </summary>
        bool IsValid { get; set; }

        /// <summary>
        /// 是否过期
        /// </summary>
        bool IsExpire { get; set; }

        /// <summary>
        /// 是否活跃
        /// </summary>
        /// <returns></returns>
        bool IsActive();

        /// <summary>
        /// 是否已失效
        /// </summary>
        /// <returns></returns>
        bool IsInvalidate();

        /// <summary>
        /// 重新激活
        /// </summary>
        void ReActive();

        /// <summary>
        /// 是否是空Session，未存储任何Attribute
        /// </summary>
        /// <returns></returns>
        bool IsEmpty();

        /// <summary>
        /// 检查Session是否存活
        /// </summary>
        /// <returns></returns>
        bool CheckAlive();

        /// <summary>
        /// 设置推送通道
        /// </summary>
        void SetPush(Push push);

        /// <summary>
        /// 获取推送通道
        /// </summary>
        /// <returns></returns>
        Push GetPush();

        /// <summary>
        /// 标志为丢弃
        /// </summary>
        void MarkDiscard();

        /// <summary>
        /// 失效当前Session
        /// </summary>
        void Invalidate();

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
    }
}
