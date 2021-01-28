using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Threading;

namespace DotNetGameFramework
{
    /// <summary>
    /// Session管理器
    /// </summary>
    public class SessionManager
    {
        /// <summary>
        /// 单例对象
        /// </summary>
        private static readonly SessionManager _instance = new SessionManager();

        /// <summary>
        /// 默认SessionId字节长度
        /// </summary>
        private const int IdByteCount = 16;

        /// <summary>
        /// Session存储器
        /// </summary>
        private readonly ConcurrentDictionary<string, Session> sessions = new ConcurrentDictionary<string, Session>();

        /// <summary>
        /// 内部定时器
        /// </summary>
        private Timer timer;

       
        /// <summary>
        /// Session创建事件
        /// </summary>
        public event SessionHandler SessionEvent;

        /// <summary>
        /// Session属性变化事件
        /// </summary>
        public event SessionAttributeHandler SessionAttributeEvent;

        /// <summary>
        /// Servlet配置
        /// </summary>
        public ServletConfig ServletConfig { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        private SessionManager()
        {
        }

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <returns></returns>
        public static SessionManager Instance
        {
            get
            {
                return _instance;
            }
        }

        /// <summary>
        /// 开启Session检测
        /// </summary>
        public void StartSessionCheckThread()
        {
            timer = new Timer(obj =>
            {
                foreach(var item in sessions)
                {
                    var session = item.Value;

                    try
                    {
                        if (session.IsInvalidate())
                        {
                            // 销毁session
                            session.Dispose();
                            continue;
                        }

                        var isAlive = session.CheckAlive();
                        if (!session.IsExpire && !isAlive)
                        {
                            // 失效session
                            session.Invalidate();
                        }
                        else if (isAlive)
                        {
                            // 心跳客户端
                            session.GetPush()?.Heartbeat();
                        }
                    }
                    catch (Exception e)
                    {
                        // TODO 记录日志
                        session.Invalidate();
                    }
                    
                }

            }, null, ServletConfig.GetSessionTickTime(), ServletConfig.GetSessionTickTime());
        }

        /// <summary>
        /// 停止Session检测线程
        /// </summary>
        public void StopSessionCheckThread()
        {
            timer?.Dispose();
        }

        /// <summary>
        /// 获取一个新的Session
        /// </summary>
        /// <returns></returns>
        public Session GetNewSession()
        {
            return GetSession(null, true);
        }

        /// <summary>
        /// 获取Session
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="allowCreate"></param>
        /// <returns></returns>
        public Session GetSession(string sessionId, bool allowCreate = false)
        {
            if (null == sessionId && !allowCreate)
            {
                return null;
            } else if (null == sessionId)
            {
                sessionId = CreateNewSessionId();
            }

            if (sessions.TryGetValue(sessionId, out var session))
            {
                return session;
            }

            if (allowCreate)
            {
                // 创建Session
                session = new StandardSession(sessionId, ServletConfig, SessionEvent, SessionAttributeEvent);
                if (sessions.TryAdd(sessionId, session))
                {
                    return session;
                }
                return sessions[sessionId];

            }
            return null;
        }

        /// <summary>
        /// 访问Session
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public void Access(string sessionId)
        {
            if (null == sessionId)
            {
                return;
            }
            if (sessions.TryGetValue(sessionId, out var session))
            {
                session.Access();
            }
        }

        /// <summary>
        /// 生成新的SessionId
        /// </summary>
        /// <returns></returns>
        public string CreateNewSessionId()
        {
            byte[] bytes = new byte[IdByteCount];
            RandomNumberGenerator.Fill(bytes);

            var guid = new Guid(bytes);
            return guid.ToString("N");
        }

        /// <summary>
        /// 移除session
        /// </summary>
        /// <param name="sessionId"></param>
        public void RemoveSession(string sessionId)
        {
            sessions.TryRemove(sessionId, out _);
        }
    }
}
