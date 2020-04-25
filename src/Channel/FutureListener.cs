using System;

namespace DotNetTcpFramework
{
    public interface FutureListener<F> where F : ChannelFuture
    {
        /// <summary>
        /// 完成时候触发
        /// </summary>
        /// <param name="future"></param>
        void operationComplete(F future);
    }
}
