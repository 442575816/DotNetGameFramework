using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace DotNetGameFramework
{
    /// <summary>
    /// HttpServer
    /// </summary>
    public class HttpServer
    {

        /// <summary>
        /// IP地址
        /// </summary>
        protected readonly string host;

        /// <summary>
        /// 端口号
        /// </summary>
        protected readonly int port;

        /// <summary>
        /// Socket参数
        /// </summary>
        public SocketOption Options { get; private set; }

        /// <summary>
        /// Host
        /// </summary>
        public IWebHost Host { get; private set; }

        private HttpDefaultHandler httpHandler;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="option"></param>
        public HttpServer(string ip, int port, SocketOption option, HttpDefaultHandler httpHandler)
        {
            this.host = ip;
            this.port = port;
            Options = option;
            this.httpHandler = httpHandler;
        }

        /// <summary>
        /// 启动HttpServer
        /// </summary>
        /// <returns></returns>
        public Task Start()
        {

            var urls = new List<string>();
            urls.Add(string.Format("http://{0}:{1}", host, port));
            if (Options.UseHttps)
            {
                urls.Add(string.Format("https://{0}:{1}", host, port));
            }
            
            Host = new WebHostBuilder().UseUrls(urls.ToArray())
                                       .UseKestrel()
                                       .Configure(app => {
                                           app.Run(httpHandler.HandleHttpRequestAsync);
                                       })
                                       .Build();
            var task = Host.StartAsync();
            return task;
            
        }
    }
}
