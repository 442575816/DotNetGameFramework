using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.IO.Pipelines;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetGameFramework
{
    /// <summary>
    /// 异步化改造的SocketAsyncEventArgs
    /// </summary>
    internal sealed class SocketAwaitableEventArgs : SocketAsyncEventArgs, ICriticalNotifyCompletion
    {
        // 完成Action
        private static readonly Action _callbackCompleted = () => { };

        // io scheduler
        private readonly PipeScheduler _ioScheduler;

        // 回调action
        private Action _callback;

        /// <summary>
        /// 构造函数
        /// </summary>
        public SocketAwaitableEventArgs(PipeScheduler ioScheduler) : base(unsafeSuppressExecutionContextFlow: true)
        {
            _ioScheduler = ioScheduler;
        }

        /// <summary>
        /// 鸭子模型，适应异步编程
        /// </summary>
        /// <returns></returns>
        public SocketAwaitableEventArgs GetAwaiter() => this;

        /// <summary>
        /// 是否完成
        /// </summary>
        public bool IsCompleted => ReferenceEquals(_callback, _callbackCompleted);

        /// <summary>
        /// 获取结果，返回的是传输的字节数
        /// </summary>
        /// <returns></returns>
        public int GetResult()
        {
            Debug.Assert(IsCompleted);

            _callback = null;

            if (SocketError != SocketError.Success)
            {
                // Socket异常了
                throw new SocketException((int)SocketError);
            }

            return BytesTransferred;
        }

        public void OnCompleted(Action continuation)
        {
            if (ReferenceEquals(_callback, _callbackCompleted) ||
                ReferenceEquals(Interlocked.CompareExchange(ref _callback, continuation, null), _callbackCompleted))
            {
                Task.Run(continuation);
            }
            
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            OnCompleted(continuation);
        }

        public void Complete()
        {
            OnCompleted(this);
        }

        protected override void OnCompleted(SocketAsyncEventArgs e)
        {
            var continuation = Interlocked.Exchange(ref _callback, _callbackCompleted);

            if (continuation != null)
            {
                _ioScheduler.Schedule(state => ((Action)state)(), continuation);
            }
        }
    }
}
