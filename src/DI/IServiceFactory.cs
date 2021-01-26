using System;

namespace DotNetGameFramework
{
    public interface IServiceFactory
    {
        /// <summary>
        /// 获取Service
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        object GetService(Type serviceType);

        /// <summary>
        /// 获取Service
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetService<T>();

        /// <summary>
        /// 内部获取Service
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        internal object GetServiceInternal(Type serviceType);
    }
}
