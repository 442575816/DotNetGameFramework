using System;

namespace DotNetGameFramework
{
    /// <summary>
    /// 服务上下文
    /// </summary>
    public interface ServletContext : IDisposable
    {
        /// <summary>
        /// 获取指定属性
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        object GetAttribute(string key, object defaultValue = null);

        /// <summary>
        /// 设置属性
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetAttribute(string key, object value);

        /// <summary>
        /// 移除属性
        /// </summary>
        /// <param name="key"></param>
        void RemoveAttribute(string key);
    }
}
