using System;

namespace DotNetGameFramework
{
    public static class WrapperUtil
    {
        /// <summary>
        /// ByteBuf池子
        /// </summary>
        public static ByteBufPool<ByteBuf> ByteBufPool { get; set; }
    }
}
