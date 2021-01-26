using System;
using System.Collections.Concurrent;
using System.Linq;

namespace DotNetGameFramework
{
    /// <summary>
    /// 默认Service工厂
    /// </summary>
    public class DefaultServiceFactory : IServiceFactory
    {
        /// <summary>
        /// Services集合
        /// </summary>
        private ServiceCollection services;

        /// <summary>
        /// services map
        /// </summary>
        private ConcurrentDictionary<Type, object> servicesMap = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="services"></param>
        public DefaultServiceFactory(ServiceCollection services)
        {
            this.services = services;
        }

        /// <summary>
        /// 获取Service
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object GetService(Type serviceType)
        {
            if (servicesMap.TryGetValue(serviceType, out object obj))
            {
                return obj;
            }

            // 创建Service
            var descriptor = services.Where(p => p.ServiceType == serviceType).First();
            if (null == descriptor)
            {
                return null;
            }

            obj = descriptor.CreateInstance(this);
            if (null != obj)
            {
                servicesMap[serviceType] = obj;
            }
            

            return obj;
        }

        /// <summary>
        /// 获取Service
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        /// <summary>
        /// 内部获取Service
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        object IServiceFactory.GetServiceInternal(Type serviceType)
        {
            if (servicesMap.TryGetValue(serviceType, out object obj))
            {
                return obj;
            }
            return null;
        }
    }
}
