using System;
using System.Collections.Generic;

namespace DotNetGameFramework
{
    /// <summary>
    /// Session
    /// </summary>
    public interface Session : IDisposable
    {
        /// <summary>
        /// SessionId
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
        void SetAttribute(string key, string value);

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


    }
}
