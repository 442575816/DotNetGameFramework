using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace DotNetGameFramework
{
    public class HttpResponse : Response
    {
        /// <summary>
        /// 内部Channel
        /// </summary>
        private HttpContext httpContext;

        /// <summary>
        /// 内部cookies
        /// </summary>
        private Dictionary<string, string> cookies;


        /// <summary>
        /// 内部headers
        /// </summary>
        private Dictionary<string, string> headers;

        /// <summary>
        /// 内部buffer
        /// </summary>
        private ByteBuf buff;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="channel"></param>
        public HttpResponse(HttpContext httpContext)
        {
            this.httpContext = httpContext;
        }

        /// <inheritdoc/>
        public object Content => buff;

        /// <inheritdoc/>
        public int HttpStatus { get; set; } = 200;

        /// <inheritdoc/>
        public ServerProtocol Protocol => ServerProtocol.HTTP;

        /// <inheritdoc/>
        public Dictionary<string, string> Cookies => cookies;

        /// <inheritdoc/>
        public Dictionary<string, string> Headers => headers;

        /// <inheritdoc/>
        public void AddCookie(string name, string value)
        {
            if (null == cookies)
            {
                cookies = new Dictionary<string, string>();
            }

            cookies[name] = value;
        }

        /// <inheritdoc/>
        public void AddHeader(string name, string value)
        {
            if (null == headers)
            {
                headers = new Dictionary<string, string>();
            }

            headers[name] = value;
        }

        /// <inheritdoc/>
        public void Write(object obj)
        {
            if (obj is byte[] bytes)
            {
                GetOutput().WriteBytes(bytes);
            }
            else if (obj is ByteBuf buff)
            {
                try
                {
                    GetOutput().WriteBytes(buff.Data, buff.ReaderIndex, buff.ReadableBytes);
                } finally
                {
                    buff.Release();
                }
                
            }
        }

        /// <summary>
        /// 获取Output
        /// </summary>
        /// <returns></returns>
        private ByteBuf GetOutput()
        {
            if (null == buff)
            {
                buff = WrapperUtil.ByteBufPool.Allocate();
            }

            return buff;
        }
    }
}
