using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace DotNetGameFramework
{
    /// <summary>
    /// HttpRequest http请求
    /// </summary>
    public class HttpRequest : Request
    {
        /// <summary>
        /// HttpContext
        /// </summary>
        private HttpContext httpContext;

        /// <summary>
        /// 内部HttpRequest
        /// </summary>
        private Microsoft.AspNetCore.Http.HttpRequest httpRequest;

        /// <summary>
        /// 内部HttpResponse
        /// </summary>
        private Response response;

        /// <summary>
        /// 请求Command
        /// </summary>
        public string Command { get; private set; }

        /// <summary>
        /// 请求RequestId
        /// </summary>
        public int RequestId { get; private set; }

        /// <summary>
        /// 请求Content
        /// </summary>
        public byte[] Content { get; private set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; private set; }

        /// <summary>
        /// ServerProtocol
        /// </summary>
        public ServerProtocol Protocol => ServerProtocol.HTTP;

        /// <summary>
        /// SessionId
        /// </summary>
        protected string SessionId { get; private set; }

        /// <summary>
        /// ServletContext
        /// </summary>
        protected ServletContext Context { get; private set; }

        /// <summary>
        /// 解析标志位
        /// </summary>
        protected bool parseFlag;

        /// <summary>
        /// 参数Map
        /// </summary>
        protected Dictionary<string, string[]> parameterMap;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        public HttpRequest(HttpContext httpContext, HttpResponse httpResponse, ServletContext context)
        {
            this.httpContext = httpContext;
            this.httpRequest = httpContext.Request;
            this.response = httpResponse;

            Context = context;
            CreateTime = DateTime.Now;

            Command = HttpUtil.GetCommand(httpRequest);
            RequestId = 0;

            if (Constants.COMMAND_GATEWAY == Command)
            {
                var values = GetParamterValues(Constants.COMMAND);
                Command = (null != values) ? values[0] : null;
            }

            SessionId = HttpUtil.GetCookieValue(httpRequest, Constants.SESSION_ID);
            SessionManager.Instance.Access(SessionId);
        }

        /// <inheritdoc/>
        public Dictionary<string, string[]> ParameterMap
        {
            get
            {
                ParseParam();
                return parameterMap;
            }
        }

        /// <inheritdoc/>
        public string Ip => (httpContext.Connection.RemoteIpAddress)?.ToString();

        /// <summary>
        /// 解析参数
        /// </summary>
        protected void ParseParam()
        {
            if (parseFlag)
            {
                return;
            }

            // FormData
            if (httpRequest.HasFormContentType && httpRequest.Form.Count > 0)
            {
                this.parameterMap = new Dictionary<string, string[]>();
                foreach(var item in httpRequest.Form)
                {
                    HttpUtil.FillParameter(parameterMap, item);
                }

                
            }

            // URL参数
            if (httpRequest.Query.Count > 0)
            {
                if (this.parameterMap == null)
                {
                    this.parameterMap = new Dictionary<string, string[]>();
                }

                foreach (var item in httpRequest.Query)
                {
                    HttpUtil.FillParameter(parameterMap, item);
                }
            }

            parseFlag = true;
        }

        /// <inheritdoc/>
        public string GetHeader(string key)
        {
            return HttpUtil.GetHeaderValue(httpRequest, key);
        }

        /// <inheritdoc/>
        public Session GetNewSession()
        {
            var session = SessionManager.Instance.GetNewSession();
            
            SessionId = session.Id;

            response.AddCookie(Constants.SESSION_ID, SessionId);

            return session;
        }

        /// <inheritdoc/>
        public string[] GetParamterValues(string key)
        {
            ParseParam();
            if (null != parameterMap && parameterMap.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }

        /// <inheritdoc/>
        public Session GetSession(bool allowCreate)
        {
            var session = SessionManager.Instance.GetSession(SessionId, allowCreate);
            if (allowCreate && session.Id != SessionId)
            {
                SessionId = session.Id;

                response.AddCookie(Constants.SESSION_ID, SessionId);   
            }

            return session;
        }

        /// <inheritdoc/>
        public void SetSessionId(string sessionId)
        {
            SessionId = sessionId;
        }
    }
}
