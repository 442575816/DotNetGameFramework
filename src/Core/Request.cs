using System;
using System.Collections.Generic;

namespace DotNetGameFramework
{
    /// <summary>
    /// 请求Request
    /// </summary>
    public interface Request
    {
        /// <summary>
        /// 参数Map
        /// </summary>
        Dictionary<string, string[]> ParamterMap { get; }

        /// <summary>
        /// 请求Command
        /// </summary>
        string Command { get; }

        /// <summary>
        /// 请求Id
        /// </summary>
        int RequestId { get; }

        /// <summary>
        /// 获取请求Content
        /// </summary>
        byte[] Content { get; }

        /// <summary>
        /// 请求Ip
        /// </summary>
        string Ip { get; }

        /// <summary>
        /// 请求创建时间
        /// </summary>
        DateTime CreateTime { get; }

        /// <summary>
        /// 获取指定Header
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string GetHeader(string key);

        /// <summary>
        /// 获取指定参数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string[] GetParamterValues(string key);

        /// <summary>
        /// 设置SessionId
        /// </summary>
        /// <param name="sessionId"></param>
        void SetSessionId(string sessionId);

        /// <summary>
        /// 通讯协议
        /// </summary>
        ServerProtocol Protocol { get; }

        /// <summary>
        /// 获取Session
        /// </summary>
        /// <param name="allowCreate"></param>
        /// <returns></returns>
        Session GetSession(bool allowCreate = false);

        /// <summary>
        /// 获取一个新的Session
        /// </summary>
        /// <returns></returns>
        Session GetNewSession();
    }
}
