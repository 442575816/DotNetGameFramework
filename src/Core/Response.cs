using System;
using System.Collections.Generic;

namespace DotNetGameFramework
{
    /// <summary>
    /// 响应Response
    /// </summary>
    public interface Response
    {
        /// <summary>
        /// 写响应
        /// </summary>
        /// <param name="obj"></param>
        void Write(object obj);

        /// <summary>
        /// 添加头
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void AddHeader(string name, string value);

        /// <summary>
        /// 添加Cookie
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void AddCookie(string name, string value);

        /// <summary>
        /// 获取响应内容
        /// </summary>
        object Content { get; }

        /// <summary>
        /// Cookies
        /// </summary>
        Dictionary<string, string> Cookies { get; }

        /// <summary>
        /// Headers
        /// </summary>
        Dictionary<string, string> Headers { get; }

        /// <summary>
        /// Http状态码
        /// </summary>
        /// <param name="status"></param>
        int HttpStatus { get; set; }

        /// <summary>
        /// 通讯协议
        /// </summary>
        ServerProtocol Protocol { get; }
    }
}
