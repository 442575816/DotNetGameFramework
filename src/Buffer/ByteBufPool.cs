﻿using System;
using System.Collections.Concurrent;
using System.Threading;

namespace DotNetGameFramework
{
    public class ByteBufPool<T> where T : Referenceable
    {
        /// <summary>
        /// 对象工厂类
        /// </summary>
        /// <returns></returns>
        public Func<T> func;

        /// <summary>
        /// 存储对象数据结构
        /// </summary>
        public ConcurrentQueue<T> queue;

        private int minSize;
        private int maxSize;
        private volatile int count;

        public ByteBufPool(Func<T> func, int minSize, int maxSize)
        {
            this.func = func;
            this.minSize = minSize;
            this.maxSize = maxSize;
            this.queue = new ConcurrentQueue<T>();

            for (int i = 0; i < minSize; i++)
            {
                this.queue.Enqueue(func());
            }
        }

        /// <summary>
        /// 分配一个对象
        /// </summary>
        /// <returns></returns>
        public T Allocate()
        {
            if (this.queue.TryDequeue(out T t))
            {
                Interlocked.Decrement(ref count);
                return t;
            }

            t = func();
            t.Deallocate = Free;
            return t;
        }

        /// <summary>
        /// 释放对象
        /// </summary>
        /// <param name="t"></param>
        public void Free(Referenceable t)
        {
            if (null == t)
            {
                return;
            }

            this.queue.Enqueue((T)t);
            Interlocked.Increment(ref count);

            while (count > maxSize)
            {
                // 释放一些对象
                if (this.queue.TryDequeue(out _))
                {
                    Interlocked.Decrement(ref count);
                }
            }
        }
    }
}
