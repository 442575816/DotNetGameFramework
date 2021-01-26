using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace DotNetGameFramework
{
    public class MessageDecoder : ByteToMessageDecoder
    {
        protected override void CallDecode(ChannelHandlerContext context, ref SequenceReader<byte> buff, List<object> outputList)
        {
            if (buff.Length < 4)
            {
                return;
            }

            buff.TryReadBigEndian(out int dataLen);
            if (buff.Length < dataLen + 4)
            {
                return;
            }

            byte[] array = new byte[32];
            Span<byte> span = array.AsSpan<byte>();
            buff.TryCopyTo(span);
            buff.Advance(32);

            RequestMessage message = new RequestMessage();
            message.Command = Encoding.UTF8.GetString(array).Trim('\0');
            buff.TryReadBigEndian(out int requestId);
            message.RequestId = requestId;

            array = new byte[dataLen - 36];
            span = array.AsSpan<byte>();
            buff.TryCopyTo(span);
            buff.Advance(array.Length);
            message.Content = array;

            outputList.Add(message);
        }
    }
}
