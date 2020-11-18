using System;
using System.IO.Pipelines;

namespace DotNetGameFramework
{
    public class DuplexPipe : IDuplexPipe
    {
        /// <summary>
        /// 输入流
        /// </summary>
        public PipeReader Input { get; }

        /// <summary>
        /// 输出流
        /// </summary>
        public PipeWriter Output { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="writer"></param>
        public DuplexPipe(PipeReader reader, PipeWriter writer)
        {
            Input = reader;
            Output = writer;
        }

        /// <summary>
        /// 构建一个Pipeline的链接pair
        /// </summary>
        /// <param name="inputOptions"></param>
        /// <param name="outputOptions"></param>
        /// <returns></returns>
        public static (IDuplexPipe, IDuplexPipe) CreateConnectionPair(PipeOptions inputOptions, PipeOptions outputOptions)
        {
            var input = new Pipe(inputOptions);
            var output = new Pipe(outputOptions);

            var transportToApplication = new DuplexPipe(output.Reader, input.Writer);
            var applicationToTransport = new DuplexPipe(input.Reader, output.Writer);

            return ValueTuple.Create(applicationToTransport, transportToApplication);
        }

    }
}
