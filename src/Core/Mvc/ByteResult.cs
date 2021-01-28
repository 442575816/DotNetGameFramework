using System;

namespace DotNetGameFramework
{
    public class ByteResult : Result<byte[]>
    {
        /// <summary>
        /// result内容
        /// </summary>
        private byte[] result;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="result"></param>
        public ByteResult(byte[] result)
        {
            this.result = result;
        }

        /// <inheritdoc/>
        public byte[] Result => result;

        /// <inheritdoc/>
        public string ViewName => "byte";
    }
}
