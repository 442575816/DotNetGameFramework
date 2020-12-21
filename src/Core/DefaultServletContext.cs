using System;
using System.Collections.Concurrent;

namespace DotNetGameFramework
{
    /// <summary>
    /// 服务上下文
    /// </summary>
    public class DefaultServletContext : ServletContext
    {
        /// <summary>
        /// 内部词典
        /// </summary>
        private Lazy<ConcurrentDictionary<string, object>> lazyDict = new Lazy<ConcurrentDictionary<string, object>>(true);

       
        public object GetAttribute(string key, object defaultValue = null)
        {
            if (lazyDict.IsValueCreated)
            {
                if (lazyDict.Value.TryGetValue(key, out object result))
                {
                    return result;
                }
            }
           
            return defaultValue;
        }

        public void RemoveAttribute(string key)
        {
            if (lazyDict.IsValueCreated)
            {
                lazyDict.Value.TryRemove(key, out _);
            }
        }

        public void SetAttribute(string key, object value)
        {
            lazyDict.Value.TryAdd(key, value);
        }

        public void Dispose()
        {
            lazyDict.Value.Clear();
        }
    }
}
