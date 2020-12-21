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
    }
}
