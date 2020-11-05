using System;
using System.Collections.Generic;

namespace DotNetGameFramework.Http
{
    /// <summary>
    /// HttpMethod
    /// </summary>
    public enum HttpMethod
    {
        HEAD,
        GET,
        POST,
        PUT,
        DELETE,
        OptionS,
        TRACE
    }

    public class HttpRequest : ChannelInboundHandlerAdapter
    {
        /// <summary>
        /// HTTP方法
        /// </summary>
        public HttpMethod Method { get; private set; }

        /// <summary>
        /// 请求URI
        /// </summary>
        public string Uri { get; private set; }

        /// <summary>
        /// 协议版本
        /// </summary>
        public string ProtocolVersion { get; private set; }

        /// <summary>
        /// 是否发生错误
        /// </summary>
        public bool Error { get; private set; }

        // header字典
        private Dictionary<string, string> headDict;

        // cookie字典
        private Dictionary<string, string> cookieDict;

        // 内部buff
        private ByteBuf buff;

        private int headIndex;

        public override void FireChannelRead(ChannelHandlerContext context, object msg)
        {
            ByteBuf data = msg as ByteBuf;
            try
            {
                buff.WriteBytes(data);
                for (int i = headIndex; i < headIndex;)
                {
                }

            }
            finally
            {
                data.Release();
            }
            
        }
    }
}
