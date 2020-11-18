using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace DotNetGameFramework
{
    public class SlabMemoryPool : MemoryPool<byte>
    {
        /// <summary>
        /// 默认block大小. 选择4096是因为操作系统默认一个页的大小是4k.
        /// </summary>
        private const int _blockSize = 4096;

        /// <summary>
        /// 默认一个slab对应的block数量，一个slab默认128k
        /// </summary>
        private const int _blockCount = 32;

        /// <summary>
        /// 最大BuffSize定义
        /// </summary>
        public override int MaxBufferSize { get; } = _blockSize;

        /// <summary>
        /// 默认block size定义
        /// </summary>
        public static int BlockSize => _blockSize;

        /// <summary>
        /// 默认slab buff大小定义，32个block，128k
        /// </summary>
        private static readonly int _slabLength = _blockSize * _blockCount;

        /// <summary>
        /// 存储可用的block
        /// </summary>
        private readonly ConcurrentQueue<MemoryPoolBlock> _blocks = new ConcurrentQueue<MemoryPoolBlock>();

        /// <summary>
        /// 存储申请过的slab
        /// </summary>
        private readonly ConcurrentStack<MemoryPoolSlab> _slabs = new ConcurrentStack<MemoryPoolSlab>();

        /// <summary>
        /// 对象是否已经被释放过，防止重复释放
        /// </summary>
        private bool _isDisposed; // To detect redundant calls

        /// <summary>
        /// 计数器，统计总共分配的block数量
        /// </summary>
        private int _totalAllocatedBlocks;

        /// <summary>
        /// dispose锁定对象
        /// </summary>
        private readonly object _disposeSync = new object();

        /// <summary>
        /// 默认申请大小，标志使用内存池默认设定
        /// </summary>
        private const int AnySize = -1;

        /// <summary>
        /// 申请一块内存
        /// </summary>
        /// <param name="minBufferSize"></param>
        /// <returns></returns>
        public override IMemoryOwner<byte> Rent(int size = AnySize)
        {
            if (size > _blockSize)
            {
                throw new ArgumentOutOfRangeException("size", $"Cannot allocate more than {size} bytes in a single buffer");
            }

            var block = Lease();
            return block;
        }

        /// <summary>
        /// 租借一块block
        /// </summary>
        /// <returns></returns>
        private MemoryPoolBlock Lease()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("MemoryPool");
            }

            if (_blocks.TryDequeue(out MemoryPoolBlock block))
            {
                // 有可用的
                block.Lease();
                return block;
            }

            // 没有可用的block，分配一块
            block = AllocateSlab();
            block.Lease();

            return block;
        }

        /// <summary>
        /// 申请一块slab
        /// </summary>
        private MemoryPoolBlock AllocateSlab()
        {
            var slab = MemoryPoolSlab.Create(_slabLength);
            _slabs.Push(slab);

            // Get the address for alignment
            IntPtr basePtr = Marshal.UnsafeAddrOfPinnedArrayElement(slab.PinnedArray, 0);
            // Page align the blocks
            var offset = (int)((((ulong)basePtr + (uint)_blockSize - 1) & ~((uint)_blockSize - 1)) - (ulong)basePtr);
            // Ensure page aligned
            Debug.Assert(((ulong)basePtr + (uint)offset) % _blockSize == 0);

            var blockCount = (_slabLength - offset) / _blockSize;
            Interlocked.Add(ref _totalAllocatedBlocks, blockCount);

            MemoryPoolBlock block = null;

            for (int i = 0; i < blockCount; i++)
            {
                block = new MemoryPoolBlock(this, slab, offset, _blockSize);

                if (i != blockCount - 1) // last block
                {
                    Return(block);
                }

                offset += _blockSize;
            }

            return block;
        }

        /// <summary>
        /// 回收block
        /// </summary>
        /// <param name="block">The block to return. It must have been acquired by calling Lease on the same memory pool instance.</param>
        internal void Return(MemoryPoolBlock block)
        {
            if (!_isDisposed)
            {
                _blocks.Enqueue(block);
            }
            else
            {
                GC.SuppressFinalize(block);
            }
        }

        /// <summary>
        /// 该方法只会在block由垃圾回收器回收时调用
        /// </summary>
        /// <param name="slab"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        internal void RefreshBlock(MemoryPoolSlab slab, int offset, int length)
        {
            lock (_disposeSync)
            {
                if (!_isDisposed && slab != null && slab.IsActive)
                {
                    // Need to make a new object because this one is being finalized
                    // Note, this must be called within the _disposeSync lock because the block
                    // could be disposed at the same time as the finalizer.
                    Return(new MemoryPoolBlock(this, slab, offset, length));
                }
            }
        }

        /// <summary>
        /// 销毁内存池
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            lock (_disposeSync)
            {
                _isDisposed = true;

                if (disposing)
                {
                    while (_slabs.TryPop(out MemoryPoolSlab slab))
                    {
                        // dispose managed state (managed objects).
                        slab.Dispose();
                    }
                }

                // Discard blocks in pool
                while (_blocks.TryDequeue(out MemoryPoolBlock block))
                {
                    GC.SuppressFinalize(block);
                }
            }
        }
    }
}
