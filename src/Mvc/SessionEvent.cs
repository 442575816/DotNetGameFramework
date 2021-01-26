namespace DotNetGameFramework
{
    /// <summary>
    /// SessionEvent类型
    /// </summary>
    public enum SessionEventType
    {
        Create,
        Destroy
    }

    /// <summary>
    /// Session事件
    /// </summary>
    public class SessionEvent
    {
        /// <summary>
        /// Session
        /// </summary>
        public Session Session { get; set; }
    }
}