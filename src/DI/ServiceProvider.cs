using System;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetGameFramework
{
    public class ServiceProvider : IServiceProvider
    {
        /// <summary>
        /// Services容器
        /// </summary>
        public ServiceCollection Services { get; }

        /// <summary>
        /// ServiceFactory
        /// </summary>
        public IServiceFactory ServiceFactory { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="services"></param>
        public ServiceProvider(ServiceCollection services)
        {
            Services = services;
            ServiceFactory = new DefaultServiceFactory(services);
        }



        /// <summary>
        /// 获取Services
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object GetService(Type serviceType)
        {
            return ServiceFactory.GetService(serviceType);
        }
    }
}
