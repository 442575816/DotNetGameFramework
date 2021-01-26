using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetGameFramework
{
    class ServiceReference
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public Type Type { get; set; }
    }

    public class ServiceDescriptor
    {
        /// <summary>
        /// Service生命周期
        /// </summary>
        public ServiceLifeTime ServiceLifeTime { get; } = ServiceLifeTime.Singleton;

        /// <summary>
        /// Service类型
        /// </summary>
        public Type ServiceType { get; }

        /// <summary>
        /// Service实现类型
        /// </summary>
        public Type? ImplementationType { get; }

        /// <summary>
        /// Service实例
        /// </summary>
        public object? ImplementationInstance { get; }

        /// <summary>
        /// Service工厂类
        /// </summary>
        public Func<IServiceProvider, object>? ImplementationFactory { get; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        ConstructorInfo? DefaultConstructor { get; set; }

        /// <summary>
        /// DI列表
        /// </summary>
        List<ServiceReference> _references = new List<ServiceReference>();

        /// <summary>
        /// 构造函数
        /// </summary>
        public ServiceDescriptor(Type serviceType, Type implementationType) : this(serviceType, implementationType, factory: null, instance: null)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="instance"></param>
        public ServiceDescriptor(Type serviceType, object instance) : this(serviceType, instance.GetType(), instance: instance)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="factory"></param>
        public ServiceDescriptor(Type serviceType, Func<IServiceProvider, object> factory) : this(serviceType, serviceType, factory)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <param name="factory"></param>
        /// <param name="instance"></param>
        public ServiceDescriptor(Type serviceType, Type implementationType, Func<IServiceProvider, object> factory = null, object instance = null)
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
            ImplementationFactory = factory;
            ImplementationInstance = instance;

            Resolve();
        }

        /// <summary>
        /// 创建单实例ServiceDescriptor
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <returns></returns>
        public static ServiceDescriptor Singleton(Type serviceType, Type implementationType)
        {
            return new ServiceDescriptor(serviceType, implementationType);
        }

        /// <summary>
        /// 创建单实例ServiceDescriptor
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static ServiceDescriptor Singleton(Type serviceType, object instance)
        {
            return new ServiceDescriptor(serviceType, instance);
        }

        /// <summary>
        /// 创建单实例ServiceDescriptor
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static ServiceDescriptor Singleton<TService>(TService instance)
        {
            return new ServiceDescriptor(typeof(TService), instance);
        }

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <returns></returns>
        public object GetInstance(ServiceCollection services)
        {
            if (ImplementationInstance != null)
            {
                return ImplementationInstance;
            }

            return services.ServiceFactory.GetService(ServiceType);
        }

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="serviceFactory"></param>
        /// <returns></returns>
        public object CreateInstance(IServiceFactory serviceFactory)
        {
            object result = null;
            var _parameters = DefaultConstructor.GetParameters();
            if (_parameters.Length == 0)
            {
                result = DefaultConstructor.Invoke(null);
            } else
            {
                object[] parameters = new object[_parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i] = serviceFactory.GetServiceInternal(_parameters[i].ParameterType);
                }
                result = DefaultConstructor.Invoke(parameters);
            }

            return result;
        }

        /// <summary>
        /// 进行解析
        /// </summary>
        private void Resolve()
        {
            if (ImplementationInstance != null)
            {
                return;
            }

            // 解析构造函数
            var constructors = ImplementationType.GetConstructors();
            ConstructorInfo defaultConstructor = null;
            var paramsLen = -1;

            foreach (ConstructorInfo ci in constructors)
            {
                var _parameters = ci.GetParameters();
                if (paramsLen == -1 || paramsLen > _parameters.Length)
                {
                    defaultConstructor = ci;
                    paramsLen = _parameters.Length;
                }
            }
            DefaultConstructor = defaultConstructor;

            // 需要注入的参数
            var parameters = DefaultConstructor.GetParameters();
            foreach (var item in parameters)
            {
                _references.Add(new ServiceReference()
                {
                    Name = item.ParameterType.Name,
                    Type = item.ParameterType
                });
            }

            // 查找Attribute
            var autowiredProps = ImplementationType.GetProperties().Where(p => p.GetCustomAttribute<Autowired>(true) != null);
            foreach (var item in autowiredProps)
            {
                _references.Add(new ServiceReference()
                {
                    Name = item.PropertyType.Name,
                    Type = item.PropertyType
                });
            }

            // 查找Fields
            var fieldProps = ImplementationType.GetFields().Where(p => p.GetCustomAttribute<Autowired>(true) != null);
            foreach (var item in fieldProps)
            {
                _references.Add(new ServiceReference()
                {
                    Name = item.FieldType.Name,
                    Type = item.FieldType
                });
            }
        }

    }
}
