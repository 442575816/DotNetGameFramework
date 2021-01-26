using System;
using System.Collections.Generic;
using System.Xml;

namespace DotNetGameFramework
{
    public class XmlServletConfig : ServletConfig
    {
        /// <summary>
        /// 内部用于存储配置文件值的词典
        /// </summary>
        private Dictionary<string, object> dict = new Dictionary<string, object>();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="path"></param>
        public XmlServletConfig(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException($"path is empty");
            }
            Parse(path);
        }

        /// <summary>
        /// 解析配置文件
        /// </summary>
        /// <param name="path"></param>
        private void Parse(string path)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(path);

            var servlet = xml.SelectSingleNode("servlet");
            if (null == servlet)
            {
                throw new Exception("config can't found servlet node");
            }

            var initParams = servlet.SelectNodes("/init-param/props");
            foreach (XmlNode param in initParams)
            {
                if ("property" == param.Name)
                {
                    string name = param.Attributes.GetNamedItem("name").Value;
                    string type = param.Attributes.GetNamedItem("type").Value;
                    string content = param.InnerText;

                    dict.Add(name, ParseValue(type, content));
                }
            }
        }

        /// <summary>
        /// 解析Value
        /// </summary>
        /// <param name="type"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private object ParseValue(string type, string content)
        {
            return type switch
            {
                "int" => int.TryParse(content, out int result1) ? result1 : null,
                "string" => content,
                "boolean" => bool.TryParse(content, out bool result2) ? result2 : null,
                _ => null,
            };
        }

        public object GetInitParam(string name)
        {
            if (dict.TryGetValue(name, out object result))
            {
                return result;
            }
            return null;
        }

        public int GetSessionTickTime()
        {
            throw new NotImplementedException();
        }

        public int GetSessionTimeoutMillis()
        {
            throw new NotImplementedException();
        }

        public int GetSessionEmptyTimeoutMillis()
        {
            throw new NotImplementedException();
        }

        public int GetSessionInvalidateMillis()
        {
            throw new NotImplementedException();
        }

        public int GetSessionNextDayInvalidateMillis()
        {
            throw new NotImplementedException();
        }
    }
}
