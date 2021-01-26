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
        protected Dictionary<string, Func<Request, Response, object>> handlerDict;

        /// <summary>
        /// view字典
        /// </summary>
        protected Dictionary<string, object> viewDict;

        /// <summary>
        /// 是否启用压缩
        /// </summary>
        protected bool Compress { get; set; } = false;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="config"></param>
        /// <param name="context"></param>
        public void Init(ServletConfig config, ServletContext context)
        {
            Config = config;
            Context = context;
            handlerDict = new Dictionary<string, Func<Request, Response, object>>();
            viewDict = new Dictionary<string, object>();

            log.Information("init servlet start");
            InitCompress();
            log.Information("init servlet end");

        }

        /// <summary>
        /// 注册Handler
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="handler"></param>
        public void AddHandler(string command, Func<Request, Response, object> handler)
        {
            if (handlerDict.TryGetValue(command, out _))
            {
                throw new Exception($"command {command} already register handler");
            }
            
            handlerDict[command] = handler;
        }

        /// <summary>
        /// 注册视图
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="view"></param>
        public void AddView(View view)
        {
            var viewName = view.ViewName;
            if (viewDict.TryGetValue(viewName, out _))
            {
                throw new Exception($"view {viewName} already registed");
            }

            viewDict[viewName] = view;
        }

        /// <summary>
        /// 调用服务
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        public void Service(Request request, Response response)
        {
            string command = request.Command;

            if (handlerDict.TryGetValue(command, out var handler))
            {
                object result = handler(request, response);
                var _result = result as IViewName;

                if (viewDict.TryGetValue(_result.ViewName, out object view))
                {
                    var _view = view as View;
                    _view.Render(result, request, response);
                }
            }
        }

        /// <summary>
        /// Dispose Servlet
        /// </summary>
        public void Dispose()
        {
            handlerDict.Clear();
        }



        /// <summary>
        /// 初始化压缩配置
        /// </summary>
        protected void InitCompress()
        {
            string value = (string)Config.GetInitParam(Servlet.ACTION_COMPRESS);
            if (!string.IsNullOrEmpty(value))
            {
                Compress = bool.Parse(value);
            }
        }

    }
}
