using System;

namespace DotNetGameFramework
{
    public struct RequestMessage
    {
        /// <summary>
        /// 请求Id
        /// </summary>
        public int RequestId { get; set; }

        /// <summary>
        /// 请求Command
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// 请求内容
        /// </summary>
        public byte[] Content { get; set; }

        /// <summary>
        /// SessionId
        /// </summary>
        public string SessionId { get; set; }

    }
}
