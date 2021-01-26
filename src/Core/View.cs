using System;

namespace DotNetGameFramework
{
    /// <summary>
    /// 视图IView
    /// </summary>
    public interface IViewName
    {
        /// <summary>
        /// 对应视图名称
        /// </summary>
        string ViewName { get; }
    }

    /// <summary>
    /// 结果返回视图
    /// </summary>
    public interface View : IViewName
    {
        /// <summary>
        /// 渲染视图
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        void Render(object result, Request request, Response response);

        /// <summary>
        /// 设置压缩属性
        /// </summary>
        bool Compress { get; set; }
    }
}
