using System;

namespace DotNetGameFramework
{
    public enum ServiceLifeTime
    {
        /// <summary>
        /// 单例
        /// </summary>
        Singleton,

        /// <summary>
        /// 多例
        /// </summary>
        ProtoType,
    }
}
