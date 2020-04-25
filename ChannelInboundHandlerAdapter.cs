using System;

namespace DotNetTcpFramework
{
    public class ChannelInboundHandlerAdapter : ChannelInboundHandler
    {
        public virtual void FireChannelActive(ChannelHandlerContext context)
        {
            context.FireChannelActive();
        }

        public virtual void FireChannelInactive(ChannelHandlerContext context)
        {
            context.FireChannelInactive();
        }

        public virtual void FireChannelRead(ChannelHandlerContext context, object msg)
        {
            context.FireChannelRead(msg);
        }

        public virtual void FireChannelRegistered(ChannelHandlerContext context)
        {
            context.FireChannelRegistered();
        }

        public virtual void FireChannelUnregistered(ChannelHandlerContext context)
        {
            context.FireChannelUnregistered();
        }

        public virtual void FireExceptionCaught(ChannelHandlerContext context, Exception e)
        {
            context.FireExceptionCaught(e);
        }
    }
}
