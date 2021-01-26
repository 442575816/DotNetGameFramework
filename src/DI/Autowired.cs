using System;

namespace DotNetGameFramework
{
    /// <summary>
    /// 自动注入属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class Autowired : Attribute
    {
        /// <summary>
        /// 注入类的名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Autowired()
        {
            Name = "";
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name"></param>
        public Autowired(string name = "")
        {
            Name = name;
        }
    }
}
