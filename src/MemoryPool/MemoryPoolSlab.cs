using System;

namespace DotNetGameFramework
{
    /// <summary>
    /// 默认一块内存，申请一块完整内存，在通过block返回给应用使用。避免内存碎片
    /// 当block被回收时，内存空间并不会释放，而是通过构建新的block供再次使用
    /// </summary>
    internal class MemoryPoolSlab
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pinnedData"></param>
        public MemoryPoolSlab(byte[] pinnedData)
        {
            PinnedArray = pinnedData;
        }

        /// <summary>
        /// 默认情况下应该一直是True，仅仅当内存池被销毁时候变成False
        /// 默认改对象分配之后不会回收
        /// </summary>
        public bool IsActive => PinnedArray != null;

        /// <summary>
        /// 缓存的内存块
        /// </summary>
        public byte[] PinnedArray { get; private set; }

        /// <summary>
        /// 静态方法，创建指定大小的缓存内存块
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static MemoryPoolSlab Create(int length)
        {
            // allocate requested memory length from the pinned memory heap
            var pinnedArray = GC.AllocateUninitializedArray<byte>(length, pinned: true);

            // allocate and return slab tracking object
            return new MemoryPoolSlab(pinnedArray);
        }

        /// <summary>
        /// 释放方法
        /// </summary>
        public void Dispose()
        {
            PinnedArray = null;
        }
    }
}
