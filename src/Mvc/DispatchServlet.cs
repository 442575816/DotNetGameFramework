using System;
using System.Collections.Generic;
using System.Reflection;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace DotNetGameFramework
{
    /// <summary>
    /// Servlet
    /// </summary>
    public class DispatchServlet : Servlet
    {
        private Logger log = new LoggerConfiguration().WriteTo.File("log.txt", rollingInterval: RollingInterval.Day, buffered: true, restrictedToMinimumLevel: LogEventLevel.Information).CreateLogger();

        /// <summary>
        /// Config服务配置
        /// </summary>
        protected ServletConfig Config { get; private set; }

        /// <summary>
        /// Config服务配置
        /// </summary>
        protected ServletContext Context { get; private set; }

        /// <summary>
        /// handler字典
        /// </summary>
        protected Dictionary<string, Func<Request, Response, string>> handlerDict;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="config"></param>
        /// <param name="context"></param>
        public void Init(ServletConfig config, ServletContext context)
        {
            Config = config;
            Context = context;

            log.Information("init servlet start");
            

        }

        public void Service(Request request, Response response)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }
}
