using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetGameFramework
{
    public class MessageDecoder : ByteToMessageDecoder
    {
        protected override void CallDecode(ChannelHandlerContext context, ByteBuf buf, List<object> outputList)
        {
            if (buf.ReadableBytes < 4)
            {
                return;
            }

            int dataLen = buf.GetInt();
            if (buf.ReadableBytes < dataLen + 4)
            {
                return;
            }

            buf.SkipBytes(4);

            byte[] array = new byte[32];
            buf.ReadBytes(array);

            RequestMessage message = new RequestMessage();
            message.Command = Encoding.UTF8.GetString(array).Trim('\0');
            message.RequestId = buf.ReadInt();
            
            array = new byte[dataLen - 36];
            buf.ReadBytes(array);
            message.Content = Encoding.UTF8.GetString(array);

            outputList.Add(message);
        }
    }
}
