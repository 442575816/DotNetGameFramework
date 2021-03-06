﻿using System;
using System.Text;
using System.Threading.Tasks;

namespace DotNetGameFramework
{
    public class MessageHandler : ChannelInboundHandlerAdapter
    {
        public override void FireChannelRead(ChannelHandlerContext context, object msg)
        {
            //Task.Run(() =>
            //{
                RequestMessage message = msg as RequestMessage;
                string response = $"hello world 111";
                byte[] bodyBytes = Encoding.UTF8.GetBytes(response);
                byte[] commandBytes = Encoding.UTF8.GetBytes(message.Command);
                byte[] array = new byte[32];
                Array.Copy(commandBytes, array, commandBytes.Length);

                ByteBuf buff = ByteBufHelper.DefaultByteBufPool.Allocate();
                buff.Retain();
                buff.WriteInt(36 + bodyBytes.Length);
                buff.WriteBytes(array);
                buff.WriteInt(0);
                buff.WriteBytes(bodyBytes);

                context.Write(buff);
                return;
        //});
            
        }
    }
}
