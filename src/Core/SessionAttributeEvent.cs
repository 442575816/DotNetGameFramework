namespace DotNetGameFramework
{
    /// <summary>
    /// SessionAttributeEventType类型
    /// </summary>
    public enum SessionAttributeEventType
    {
        Add,
        Replace,
        Remove
    }

    public class SessionAttributeEvent : SessionEvent
    {
        /// <summary>
        /// 属性Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 属性值
        /// </summary>
        public object Value { get; set; }
    }
}