using System;

namespace DotNetGameFramework
{
    public static class ByteBufHelper
    {
        /// <summary>
        /// 默认池子
        /// </summary>
        public static ByteBufPool<ByteBuf> DefaultByteBufPool = new ByteBufPool<ByteBuf>(() =>
        {
            return new ByteBuf(256);
        }, 10, 100);
    }
}
