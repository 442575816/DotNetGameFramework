using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DotNetGameFramework
{
    public class HttpDefaultHandler
    {
        /// <summary>
        /// Servlet
        /// </summary>
        private Servlet Servlet { get; set; }

        /// <summary>
        /// ServletContext
        /// </summary>
        private ServletContext ServletContext { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="servlet"></param>
        /// <param name="context"></param>
        public HttpDefaultHandler(Servlet servlet, ServletContext context)
        {
            Servlet = servlet;
            ServletContext = context;
        }

        /// <summary>
        /// 异步处理Http请求
        /// </summary>
        /// <param name="http"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task HandleHttpRequestAsync(HttpContext http)
        {
            var response = new HttpResponse(http);
            var request = new HttpRequest(http, response, ServletContext);

            try
            {
                // 处理响应
                Servlet.Service(request, response);
                

                // 写数据
                await HttpUtil.HandleHttpResponse(http, request, response);
            }
            catch (Exception e)
            {
                // TODO 记录日志
                http.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }

        }
    }
}
