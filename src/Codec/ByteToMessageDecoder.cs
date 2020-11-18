using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DotNetGameFramework
{
    public abstract class ByteToMessageDecoder : ChannelInboundHandlerAdapter
    {
        private ByteBuf cumulation;
        private bool first;

        public override void FireChannelRead(ChannelHandlerContext context, object msg)
        {
            if (msg is ReadOnlySequence<byte>)
            {
                List<object> outputList = new List<object>();
                var data = (ReadOnlySequence<byte>)msg;
                var reader = new SequenceReader<byte>(data);

                try
                {
                    CallDecode(context, ref reader, outputList);
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    var input = context.ChannelPipeline.Input;
                    SequencePosition end = data.GetPosition(reader.Consumed);
                    input.AdvanceTo(end);
                    if (outputList.Count > 0)
                    {
                        for (int i = 0; i < outputList.Count; i++)
                        {
                            context.FireChannelRead(outputList[i]);
                        }
                    }
                }
            }
            else
            {
                context.FireChannelRead(msg);
            }
        }

        /// <summary>
        /// 编码方法
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cumulation"></param>
        /// <param name="outputList"></param>
        protected abstract void CallDecode(ChannelHandlerContext context, ref SequenceReader<byte> data, List<object> outputList);

        /// <summary>
        /// 连接ByteBuf
        /// </summary>
        /// <param name="cumulation"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private ByteBuf Cumulate(ByteBuf cumulation, ByteBuf data)
        {
            try
            {
                cumulation.WriteBytes(data);
                return cumulation;
            }
            finally
            {
                data.Release();
            }
        }
    }
}
