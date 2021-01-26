using System;

namespace DotNetGameFramework
{
    public interface Result<out T> : IViewName
    {
        /// <summary>
        /// 最终结果
        /// </summary>
        T Result { get; }
    }
}
