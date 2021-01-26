using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DotNetGameFramework
{
    /// <summary>
    /// TcpRequest Tcp请求
    /// </summary>
    public class TcpRequest : Request
    {
        /// <summary>
        /// 内部Channel
        /// </summary>
        private SocketServerChannel channel;

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
        public ServerProtocol Protocol => ServerProtocol.TCP;

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
        protected Dictionary<string, string[]> paramterMap;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="context"></param>
        /// <param name="message"></param>
        public TcpRequest(SocketServerChannel channel, ServletContext context, RequestMessage message)
        {
            this.channel = channel;
            Context = context;
            CreateTime = DateTime.Now;

            Command = message.Command;
            RequestId = message.RequestId;
            Content = message.Content;

            SessionId = message.SessionId;
            SessionManager.Instance.Access(SessionId);
        }

        /// <inheritdoc/>
        public Dictionary<string, string[]> ParamterMap
        {
            get
            {
                ParseParam();
                return paramterMap;
            }
        }

        /// <inheritdoc/>
        public string Ip => (channel.Socket.RemoteEndPoint as IPEndPoint)?.Address.ToString();

        /// <summary>
        /// 解析参数
        /// </summary>
        protected void ParseParam()
        {
            if (parseFlag)
            {
                return;
            }

            if (null == Content)
            {
                return;
            }

            try
            {
                ParseParam(Encoding.UTF8.GetString(Content));
                parseFlag = true;
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        /// <summary>
        /// 解析参数
        /// </summary>
        /// <param name="content"></param>
        protected void ParseParam(string content)
        {
            this.paramterMap = new Dictionary<string, string[]>();

            string[] strs = content.Split('&');
            foreach (string value in strs)
            {
                var index = value.IndexOf('=');
                if (index == -1)
                {
                    string k = value;
                    paramterMap[k] = null;
                }
                else
                {
                    string k = value.Substring(0, index);
                    string v = value.Substring(index + 1);

                    if (paramterMap.TryGetValue(k, out string[] values))
                    {
                        if (null == values || values.Length == 0)
                        {
                            paramterMap[k] = new string[] { v };
                        }
                        else
                        {
                            string[] newArray = new string[values.Length + 1];
                            Array.Copy(values, newArray, values.Length);
                            newArray[values.Length] = v;

                            paramterMap[k] = newArray;
                        }
                    }
                    else
                    {
                        paramterMap[k] = new string[] { v };
                    }
                }
            }
        }

        /// <inheritdoc/>
        public string GetHeader(string key)
        {
            throw new NotSupportedException(nameof(GetHeader));
        }

        /// <inheritdoc/>
        public Session GetNewSession()
        {
            throw new NotSupportedException(nameof(GetNewSession));
        }

        /// <inheritdoc/>
        public string[] GetParamterValues(string key)
        {
            if (paramterMap.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }

        /// <inheritdoc/>
        public Session GetSession(bool allowCreate)
        {
            return SessionManager.Instance.GetSession(SessionId, allowCreate);
        }

        /// <inheritdoc/>
        public void SetSessionId(string sessionId)
        {
            SessionId = sessionId;
            channel.AddAttribute<string>(Constants.SESSION_ID, sessionId);
        }
    }
}
