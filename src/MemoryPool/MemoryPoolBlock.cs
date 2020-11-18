using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace DotNetGameFramework
{
    /// <summary>
    /// 内存中供使用的一个内存块
    /// </summary>
    internal class MemoryPoolBlock : IMemoryOwner<byte>
    {
        private readonly int _offset;
        private readonly int _length;

        /// <summary>
        /// 构造函数
        /// </summary>
        internal MemoryPoolBlock(SlabMemoryPool pool, MemoryPoolSlab slab, int offset, int length)
        {
            _offset = offset;
            _length = length;

            Pool = pool;
            Slab = slab;

            Memory = MemoryMarshal.CreateFromPinnedArray(slab.PinnedArray, _offset, _length);
        }

        /// <summary>
        /// 该block持有的pool对象引用
        /// </summary>
        public SlabMemoryPool Pool { get; }

        /// <summary>
        /// 该block持有的slab对象引用
        /// </summary>
        public MemoryPoolSlab Slab { get; }

        /// <summary>
        /// 内存块
        /// </summary>
        public Memory<byte> Memory { get; }

        /// <summary>
        /// 析构函数，当被回收时候执行
        /// </summary>
        ~MemoryPoolBlock()
        {
            Pool.RefreshBlock(Slab, _offset, _length);
        }

        /// <summary>
        /// 主动回收
        /// </summary>
        public void Dispose()
        {
            Pool.Return(this);
        }

        /// <summary>
        /// 租用
        /// </summary>
        public void Lease()
        {
        }
    }
}
