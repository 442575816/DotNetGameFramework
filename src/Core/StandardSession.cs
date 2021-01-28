using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace DotNetGameFramework
{
    public class StandardSession : Session
    {
        /// <summary>
        /// 默认为玩家保存的历史消息数目
        /// </summary>
        private const int MAX_MSG_LEN = 100;

        /// <summary>
        /// 存放session具体内容词典
        /// </summary>
        private ConcurrentDictionary<string, object> dict;

        /// <inheritdoc/>
        public string Id { get; set; }

        /// <inheritdoc/>
        public bool IsValid { get; set; }

        /// <inheritdoc/>
        public bool IsExpire { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public long CreateTime { get; private set; }

        /// <summary>
        /// 上次访问时间
        /// </summary>
        public long LastAccessTime { get; private set; }

        private SessionHandler sessionHandler;
        private SessionAttributeHandler sessionAttributeHandler;
        private ServletConfig servletConfig;

        // session超时时间
        private long sessionTimeoutMillis;
        // 空session超时时间
        private long sessionEmptyTimeoutMillis;
        // session过期时间
        private long sessionInvalidateMillis;

        // push通道
        private Push push;

        // 丢弃标识
        private bool discard = false;

        // 历史消息存放List
        private List<(string Command, object Content)> msgList = null;

        // 锁
        private object lockObj = new object();

        /// <summary>
        /// 构造函数
        /// </summary>
        public StandardSession()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sessionHandler"></param>
        /// <param name="attributeHandler"></param>
        public StandardSession(string id, ServletConfig servletConfig, SessionHandler sessionHandler, SessionAttributeHandler attributeHandler)
        {
            Id = id;
            this.servletConfig = servletConfig;
            this.sessionHandler = sessionHandler;
            this.sessionAttributeHandler = attributeHandler;

            CreateTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            LastAccessTime = CreateTime;
            this.sessionTimeoutMillis = servletConfig.GetSessionTimeoutMillis();
            this.sessionEmptyTimeoutMillis = servletConfig.GetSessionEmptyTimeoutMillis();
            this.sessionInvalidateMillis = servletConfig.GetSessionInvalidateMillis();

            sessionHandler(SessionEventType.Create, new SessionEvent { Session = this});
        }

        /// <inheritdoc/>
        public void Access()
        {
            LastAccessTime = TimeUtil.Timestamp; 
        }

        /// <inheritdoc/>
        public void Invalidate()
        {
            if (discard)
            {
                Discard();
                return;
            }

            try
            {
                sessionHandler(SessionEventType.Destroy, new SessionEvent { Session = this });
            }
            catch (Exception e)
            {
                // TODO 记录日志
            }

            try
            {
                push?.Dispose();
            }
            catch (Exception)
            {
                // Ignore
            }

            // 清理历史消息
            msgList?.Clear();
            msgList = null;

            if (IsEmpty() || !IsValid)
            {
                // 清空Session内容
                Dispose();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            dict?.Clear();
            dict = null;

            msgList?.Clear();
            msgList = null;

            SessionManager.Instance.RemoveSession(Id);
        }

        /// <inheritdoc/>
        public object GetAttribute(string key)
        {
            if (null == dict)
            {
                return null;
            }

            if (dict.TryGetValue(key, out var v))
            {
                return v;
            }
            return null;
        }

        /// <inheritdoc/>
        public bool CheckAlive()
        {
            if (IsExpire)
            {
                return false;
            }

            if (discard)
            {
                return false;
            }

            if (IsEmpty() && TimeUtil.GetTimeSpan(LastAccessTime) < sessionEmptyTimeoutMillis)
            {
                return true;
            }
            else if (TimeUtil.GetTimeSpan(LastAccessTime) < sessionTimeoutMillis)
            {
                return true;
            }
            else
            {
                IsExpire = true;
            }

            return false;
        }

        public bool IsActive()
        {
            return null != push && push.IsPushable();
        }

        /// <inheritdoc/>
        public bool IsEmpty()
        {
            return dict == null || dict.IsEmpty;
        }

        /// <inheritdoc/>
        public bool IsInvalidate()
        {
            if (!IsExpire)
            {
                return false;
            }

            return TimeUtil.GetTimeSpan(LastAccessTime) > sessionInvalidateMillis;
        }

        /// <inheritdoc/>
        public void ReActive()
        {
            if (!TimeUtil.IsSameDay(CreateTime, DateTime.Now))
            {
                this.sessionInvalidateMillis = servletConfig.GetSessionNextDayInvalidateMillis();
            }
            LastAccessTime = TimeUtil.Timestamp;
            IsExpire = false;
            this.discard = false;
        }

        /// <inheritdoc/>
        public void RemoveAttribute(string key)
        {
            if (null == dict)
            {
                return;
            }
            if (dict.TryRemove(key, out var value))
            {
                sessionAttributeHandler(SessionAttributeEventType.Remove, new SessionAttributeEvent { Session = this, Key = key, Value = value });
            }
        }

        /// <inheritdoc/>
        public void SetAttribute(string key, object value)
        {
            CheckSessionDict();

            var eventType = SessionAttributeEventType.Add;
            var eventValue = value;

            dict.AddOrUpdate(key, value, (k, oldValue) => {
                eventType = SessionAttributeEventType.Replace;
                eventValue = oldValue;
                return value;
            });

            try
            {
                // 通知Attribute
                sessionAttributeHandler(eventType, new SessionAttributeEvent { Session = this, Key = key, Value = eventValue });
            } catch (Exception e)
            {
                // TODO 记录日志
            }
            
        }

        /// <inheritdoc/>
        public void MarkDiscard()
        {
            discard = true;
        }

        /// <inheritdoc/>
        public void SetPush(Push push)
        {
            this.push?.Dispose();
            this.push = push;
        }

        /// <inheritdoc/>
        public Push GetPush()
        {
            return this.push;
        }

        /// <inheritdoc/>
        public void Push(string command, byte[] bytes)
        {
            Push push = this.push;
            if (null != push && push.IsPushable())
            {
                PushHistoryMsg();

                push.Push(command, bytes);
            }
            else
            {
                SaveHistoryMsg((Command:command, Content:bytes));
            }    
        }

        /// <inheritdoc/>
        public void Push(ByteBuf buffer)
        {
            Push push = this.push;
            if (null != push && push.IsPushable())
            {
                PushHistoryMsg();

                push.Push(buffer);
            }
            else
            {
                SaveHistoryMsg((Command: null, Content: buffer));
            }
        }

        /// <summary>
        /// 丢弃该Session
        /// </summary>
        private void Discard()
        {
            try
            {
                sessionHandler(SessionEventType.Destroy, new SessionEvent { Session = this });
            }
            catch (Exception e)
            {
                // TODO 记录日志
            }

            try
            {
                push?.Discard();
            }
            catch (Exception)
            {
                // Ignore
            }

            // 清空Session内容
            Dispose();
        }

        /// <summary>
        /// 检查Dict
        /// </summary>
        private void CheckSessionDict()
        {
            if (dict == null)
            {
                Interlocked.CompareExchange(ref dict, new ConcurrentDictionary<string, object>(), null);
            }
        }

        /// <summary>
        /// 存储历史推送消息
        /// </summary>
        /// <param name="message"></param>
        private void SaveHistoryMsg((string Command, object Content) message)
        {
            lock(lockObj)
            {
                if (null == msgList)
                {
                    msgList = new List<(string, object)>(MAX_MSG_LEN);
                }

                if (msgList.Count < MAX_MSG_LEN)
                {
                    msgList.Add(message);
                }

            }
        }

        /// <summary>
        /// 处理历史消息
        /// </summary>
        private void PushHistoryMsg()
        {
            if (null != msgList && msgList.Count > 0)
            {
                lock(lockObj)
                {
                    foreach(var item in msgList)
                    {
                        if (item.Item1 != null)
                        {
                            push?.Push(item.Command, item.Content as byte[]);
                        }
                        else
                        {
                            push?.Push(item.Content as ByteBuf);
                        }
                    }
                    msgList.Clear();
                }
            }
        }
    }
}
