using System;
using System.Threading;

namespace DotNetGameFramework
{
    sealed class DefaultAttribute
    {
        /// <summary>
        /// 属性Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 属性值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 上一个节点
        /// </summary>
        public DefaultAttribute Prev { get; set; }

        /// <summary>
        /// 下一个节点
        /// </summary>
        public DefaultAttribute Next { get; set; }
    }

    public class DefaultAttributeMap : AttributeMap
    {
        // 内部属性存放地
        private DefaultAttribute[] attributes;

        private const int BUCKET_SIZE = 4;
        private const int MASK = BUCKET_SIZE - 1;

        /// <inheritdoc/>
        public void AddAttribute<T>(string key, T value)
        {
            if (null == attributes)
            {
                Interlocked.CompareExchange(ref attributes, new DefaultAttribute[BUCKET_SIZE], null);
            }

            int i = key.GetHashCode() & MASK;
            DefaultAttribute head = attributes[i];
            if (null == head)
            {
                head = new DefaultAttribute();

                var attribute = new DefaultAttribute { Key = key, Value = value };
                head.Next = attribute;
                attribute.Prev = head;

                if (Interlocked.CompareExchange(ref attributes[i], head, null) == null)
                {
                    return;
                }
            }

            head = attributes[i];
            lock(head)
            {
                var curr = head;
                for (; ; )
                {
                    var next = curr.Next;
                    if (next == null)
                    {
                        var attribute = new DefaultAttribute { Key = key, Value = value };
                        curr.Next = attribute;
                        attribute.Prev = curr;
                        break;
                    }

                    if (next.Key == key)
                    {
                        next.Value = value;
                        break;
                    }
                    curr = next;
                }
            }

        }

        /// <inheritdoc/>
        public T GetAttribute<T>(string key)
        {
            if (null == attributes)
            {
                return default(T);
            }

            int i = key.GetHashCode() & MASK;
            DefaultAttribute head = attributes[i];

            if (null == head)
            {
                return default(T);
            }

            //lock (head)
            //{
                var curr = head;
                for (; ; )
                {
                    var next = curr.Next;
                    if (next == null)
                    {
                        return default(T);
                    }

                    if (next.Key == key)
                    {
                        return (T)next.Value;
                    }
                    curr = next;
                }
            //}
        }

        /// <inheritdoc/>
        public void RemoveAttribute(string key)
        {
            if (null == attributes)
            {
                return;
            }

            int i = key.GetHashCode() & MASK;
            DefaultAttribute head = attributes[i];

            if (null == head)
            {
                return;
            }

            lock (head)
            {
                var curr = head;
                for (; ; )
                {
                    var next = curr.Next;
                    if (next == null)
                    {
                        return;
                    }

                    if (next.Key == key)
                    {
                        curr.Next = next.Next;
                        if (next.Next != null)
                        {
                            next.Next.Prev = curr;
                        }
                        next.Prev = null;
                        next.Next = null;
                        break;
                    }
                    curr = next;
                }
            }
        }
    }
}
