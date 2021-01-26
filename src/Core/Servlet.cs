using System;

namespace DotNetGameFramework
{
    /// <summary>
    /// Servlet服务中心
    /// </summary>
    public interface Servlet : IDisposable
    {
        /// <summary>
        /// 服务初始化
        /// </summary>
        void Init(ServletConfig config, ServletContext context);

        /// <summary>
        /// 处理请求
        /// </summary>
        void Service(Request request, Response response);

        /// <summary>
        /// 是否启用压缩
        /// </summary>
        const string ACTION_COMPRESS = "compress";

    }
}
