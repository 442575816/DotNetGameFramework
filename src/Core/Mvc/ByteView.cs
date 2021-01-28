using System;
using System.Buffers;
using System.Text;

namespace DotNetGameFramework
{
    public class ByteView : View
    {
        /// <inheritdoc/>
        public string ViewName => "byte";

        /// <inheritdoc/>
        public bool Compress { get; set; } = false;

        /// <inheritdoc/>
        public void Render(object result, Request request, Response response)
        {
            var _result = result as ByteResult;
            if (_result == null)
            {
                throw new ArgumentException($"unexcepted argument type except {typeof(ByteResult)}, now {result.GetType()}");
            }
            
            if (request.Protocol == ServerProtocol.TCP)
            {
                //string response = $"hello world 111";
                byte[] bodyBytes = _result.Result;
                byte[] commandBytes = Encoding.UTF8.GetBytes(request.Command);
                byte[] array = new byte[32];
                Array.Copy(commandBytes, array, commandBytes.Length);


                ByteBuf buff = WrapperUtil.ByteBufPool.Allocate();
                buff.Retain();
                buff.WriteInt(36 + bodyBytes.Length);
                buff.WriteBytes(array);
                buff.WriteInt(0);
                buff.WriteBytes(bodyBytes);

                response.Write(buff);

            }
            else if (request.Protocol == ServerProtocol.HTTP)
            {
                //response.AddHeader("Content-Type", "text/json");
                response.Write(_result.Result);
            }
            
        }
    }
}
