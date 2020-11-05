using System;

namespace DotNetGameFramework
{
    public class DefaultChannelHandlerContext : ChannelHandlerContext
    {
        /// <summary>
        /// 当前Name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// ChannelHandler
        /// </summary>
        public ChannelHandler ChannelHandler { get; set; }

        /// <summary>
        /// 下一个Handler
        /// </summary>
        internal DefaultChannelHandlerContext Next { get; set; }

        /// <summary>
        /// 前一个Handler
        /// </summary>
        internal DefaultChannelHandlerContext Prev { get; set; }

        /// <summary>
        /// 获取处理管道
        /// </summary>
        public ChannelPipeline ChannelPipeline { get; private set; }

        /// <summary>
        /// 获取当前Channel
        /// </summary>
        public SocketServerChannel Channel => ChannelPipeline.Channel;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="name"></param>
        /// <param name="handler"></param>
        public DefaultChannelHandlerContext(ChannelPipeline pipeline, String name, ChannelHandler handler)
        {
            ChannelPipeline = pipeline;
            Name = name;
            ChannelHandler = handler;
        }

        public void FireChannelRegistered()
        {
            ChannelHandlerContext context = GetNextChannelHandlerContext(typeof(ChannelInboundHandler));
            if (null != context)
            {
                (context.ChannelHandler as ChannelInboundHandler)?.FireChannelRegistered(context);
            }
        }

        public void FireChannelUnregistered()
        {
            ChannelHandlerContext context = GetNextChannelHandlerContext(typeof(ChannelInboundHandler));
            if (null != context)
            {
                (context.ChannelHandler as ChannelInboundHandler)?.FireChannelUnregistered(context);
            }
        }

        public void FireChannelActive()
        {
            ChannelHandlerContext context = GetNextChannelHandlerContext(typeof(ChannelInboundHandler));
            if (null != context)
            {
                (context.ChannelHandler as ChannelInboundHandler)?.FireChannelActive(context);
            }
        }

        public void FireChannelInactive()
        {
            ChannelHandlerContext context = GetNextChannelHandlerContext(typeof(ChannelInboundHandler));
            if (null != context)
            {
                (context.ChannelHandler as ChannelInboundHandler)?.FireChannelInactive(context);
            }
        }

        public void FireExceptionCaught(Exception e)
        {
            ChannelHandlerContext context = GetNextChannelHandlerContext(typeof(ChannelInboundHandler));
            if (null != context)
            {
                (context.ChannelHandler as ChannelInboundHandler)?.FireExceptionCaught(context, e);
            }
        }

        public void FireChannelRead(object msg)
        {
            ChannelHandlerContext context = GetNextChannelHandlerContext(typeof(ChannelInboundHandler));
            if (null != context)
            {
                (context.ChannelHandler as ChannelInboundHandler)?.FireChannelRead(context, msg);
            }
        }

        public ChannelFuture Write(object msg)
        {
            ChannelHandlerContext context = GetPrevChannelHandlerContext(typeof(ChannelOutboundHandler));
            if (null != context)
            {
                return (context.ChannelHandler as ChannelOutboundHandler)?.Write(msg);
            }
            // 最终调用Channel.SendAsync
            Channel.SendAsync(msg as ByteBuf);
            return null;
        }

        /// <summary>
        /// 获取下一个Handler
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private ChannelHandlerContext GetNextChannelHandlerContext(Type type)
        {
            DefaultChannelHandlerContext context = Next;
            while (null != context && null != context.ChannelHandler && !type.IsAssignableFrom(context.ChannelHandler.GetType()))
            {
                context = context.Next;
            }

            return context;
        }

        /// <summary>
        /// 获取下一个Handler
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private ChannelHandlerContext GetPrevChannelHandlerContext(Type type)
        {
            DefaultChannelHandlerContext context = Prev;
            while (null != context && !type.IsAssignableFrom(context.GetType()))
            {
                context = context.Prev;
            }

            return context;
        }
    }
}
