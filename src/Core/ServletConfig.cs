using System;
using System.Collections.Generic;

namespace DotNetGameFramework
{
    /// <summary>
    /// 服务配置
    /// </summary>
    public interface ServletConfig
    {
        /// <summary>
        /// 获取指定参数
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        object GetInitParam(string name);

        /// <summary>
        /// 获取Session检测间隔
        /// </summary>
        /// <returns></returns>
        int GetSessionTickTime();

        /// <summary>
        /// 获取Session超时时间
        /// </summary>
        /// <returns></returns>
        int GetSessionTimeoutMillis();

        /// <summary>
        /// 获取空Session超时时间
        /// </summary>
        /// <returns></returns>
        int GetSessionEmptyTimeoutMillis();

        /// <summary>
        /// 获取Session失效时间
        /// </summary>
        /// <returns></returns>
        int GetSessionInvalidateMillis();

        /// <summary>
        /// 获取Session隔天失效时间
        /// </summary>
        /// <returns></returns>
        int GetSessionNextDayInvalidateMillis();
    }
}
