using System;
using System.Collections;
using System.Collections.Generic;

namespace DotNetGameFramework
{
    public class ServiceCollection : IList<ServiceDescriptor>
    {
        /// <summary>
        /// 内部ServiceDescriptor容器
        /// </summary>
        private readonly List<ServiceDescriptor> _descriptors = new List<ServiceDescriptor>();

        /// <summary>
        /// 服务提供器
        /// </summary>
        private IServiceProvider _serviceProvider = null;

        /// <inheritdoc/>
        public int Count => _descriptors.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        // ServiceFactory
        public IServiceFactory ServiceFactory { get; internal set; }

        /// <inheritdoc/>
        public ServiceDescriptor this[int index]
        {
            get
            {
                return _descriptors[index];
            }
            set
            {
                _descriptors[index] = value;
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _descriptors.Clear();
        }

        /// <inheritdoc/>
        public bool Contains(ServiceDescriptor item)
        {
            return _descriptors.Contains(item);
        }

        /// <inheritdoc/>
        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            _descriptors.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public bool Remove(ServiceDescriptor item)
        {
            return _descriptors.Remove(item);
        }

        /// <inheritdoc/>
        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return _descriptors.GetEnumerator();
        }

        /// <inheritdoc/>
        public int IndexOf(ServiceDescriptor item)
        {
            return _descriptors.IndexOf(item);
        }

        /// <inheritdoc/>
        public void Insert(int index, ServiceDescriptor item)
        {
            _descriptors.Insert(index, item);
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            _descriptors.RemoveAt(index);
        }

        /// <inheritdoc/>
        void ICollection<ServiceDescriptor>.Add(ServiceDescriptor item)
        {
            _descriptors.Add(item);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// 添加Service实例
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <returns></returns>
        public ServiceCollection AddSingleton(Type serviceType, Type implementationType)
        {
            var descriptor = ServiceDescriptor.Singleton(serviceType, implementationType);
            _descriptors.Add(descriptor);

            return this;
        }

        /// <summary>
        /// 添加Service实例
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <returns></returns>
        public ServiceCollection AddSingleton<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            var descriptor = ServiceDescriptor.Singleton(typeof(TService), typeof(TImplementation));
            _descriptors.Add(descriptor);

            return this;
        }

        /// <summary>
        /// 添加Service实例
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public ServiceCollection AddSingleton(Type serviceType)
        {
            var descriptor = ServiceDescriptor.Singleton(serviceType, serviceType);
            _descriptors.Add(descriptor);

            return this;
        }

        /// <summary>
        /// 添加Service实例
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public ServiceCollection AddSingleton<TService>()
            where TService: class
        {
            var descriptor = ServiceDescriptor.Singleton(typeof(TService), typeof(TService));
            _descriptors.Add(descriptor);

            return this;
        }

        /// <summary>
        /// 添加Service实例
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <returns></returns>
        public ServiceCollection AddSingleton(Type serviceType, object instance)
        {
            var descriptor = ServiceDescriptor.Singleton(serviceType, instance);
            _descriptors.Add(descriptor);

            return this;
        }

        /// <summary>
        /// 添加Service实例
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <returns></returns>
        public ServiceCollection AddSingleton<TService>(TService instance)
            where TService : class
        {
            var descriptor = ServiceDescriptor.Singleton(typeof(TService), instance);
            _descriptors.Add(descriptor);

            return this;
        }

        /// <summary>
        /// 构建类获取器
        /// </summary>
        /// <returns></returns>
        public IServiceProvider BuildServiceProvider()
        {
            _serviceProvider = new ServiceProvider(this);
            return _serviceProvider;
        }

        /// <summary>
        /// 获取类获取器
        /// </summary>
        /// <returns></returns>
        public IServiceProvider GetServiceProvider()
        {
            return _serviceProvider;
        }


    }
}
