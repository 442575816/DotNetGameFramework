using System;

namespace DotNetGameFramework
{
    public interface AttributeMap
    {
        /// <summary>
        /// 添加属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void AddAttribute<T>(string key, T value);

        /// <summary>
        /// 获取Attribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T GetAttribute<T>(string key);

        /// <summary>
        /// 移除指定Attribute
        /// </summary>
        /// <param name="key"></param>
        void RemoveAttribute(string key);
    }
}
