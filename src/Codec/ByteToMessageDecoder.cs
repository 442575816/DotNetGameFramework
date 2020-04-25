using System;
using System.Collections.Generic;

namespace DotNetTcpFramework
{
    public abstract class ByteToMessageDecoder : ChannelInboundHandlerAdapter
    {
        private ByteBuf cumulation;
        private bool first;

        public override void FireChannelRead(ChannelHandlerContext context, object msg)
        {
            if (msg is ByteBuf)
            {
                List<object> outputList = new List<object>();
                try
                {
                    ByteBuf data = msg as ByteBuf;
                    first = cumulation == null;

                    if (first)
                    {
                        cumulation = data;
                    }
                    else
                    {
                        cumulation = Cumulate(cumulation, data);
                    }
                    CallDecode(context, cumulation, outputList);
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    if (cumulation != null && cumulation.ReadableBytes <= 0)
                    {
                        cumulation.Release();
                        cumulation = null;
                    }

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
        protected abstract void CallDecode(ChannelHandlerContext context, ByteBuf buf, List<object> outputList);

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
