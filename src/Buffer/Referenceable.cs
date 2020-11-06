using System;

namespace DotNetGameFramework
{
    /// <summary>
    /// 可计数的
    /// </summary>
    public abstract class Referenceable
    {
        /// <summary>
        /// 回收器
        /// </summary>
        public Action<Referenceable> Deallocate { get; set; }

        /// <summary>
        /// 引用计数器
        /// </summary>
        public int ReferenceCount { get; set; }

        /// <summary>
        /// 引用+1
        /// </summary>
        public void Retain()
        {
            ReferenceCount++;
        }

        /// <summary>
        /// 引用-1, 释放ByteBuf
        /// </summary>
        public void Release()
        {
            ReferenceCount--;
            if (ReferenceCount == 0)
            {
                Recycle();
                Deallocate?.Invoke(this);
            }
        }

        public abstract void Recycle();
    }
}
