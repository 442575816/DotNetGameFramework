using System;

namespace DotNetGameFramework
{
    public class DefaultChannelPipeline : ChannelPipeline
    {
        /// <summary>
        /// Channel
        /// </summary>
        public SocketServerChannel Channel { get; private set; }


        // Head
        private DefaultChannelHandlerContext Head { get; set; }

        // Tail
        private DefaultChannelHandlerContext Tail { get; set; }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="channel"></param>
        public DefaultChannelPipeline(SocketServerChannel channel)
        {
            DefaultChannelHandlerContext emptyHeader = new DefaultChannelHandlerContext(this, "Internal_Head_Handler", null);
            DefaultChannelHandlerContext emptyTailer = new DefaultChannelHandlerContext(this, "Internal_Tail_Handler", null);
            Head = emptyHeader;
            Tail = emptyTailer;
            Head.Next = Tail;
            Tail.Prev = Head;
            
            Channel = channel;
        }

        public void AddFirst(string name, ChannelHandler handler)
        {
            DefaultChannelHandlerContext context = new DefaultChannelHandlerContext(this, name, handler);
            context.Next = Head.Next;
            context.Prev = Head;
            Head.Next.Prev = context;
            Head.Next = context;
        }

        public void AddLast(string name, ChannelHandler handler)
        {
            DefaultChannelHandlerContext context = new DefaultChannelHandlerContext(this, name, handler);

            context.Prev = Tail.Prev;
            context.Next = Tail;
            Tail.Prev.Next = context;
            Tail.Prev = context;
        }

        public void Remove(string name)
        {
            DefaultChannelHandlerContext context = Head;
            if (context != null && context.Name == name)
            {
                DefaultChannelHandlerContext prev = context.Prev;
                DefaultChannelHandlerContext next = context.Next;
                if (null != prev)
                {
                    prev.Next = next;
                }
                if (null != next)
                {
                    next.Prev = prev;
                }
            }
        }

        public void Remove(ChannelHandler handler)
        {
            DefaultChannelHandlerContext context = Head;
            if (context != null && context == handler)
            {
                DefaultChannelHandlerContext prev = context.Prev;
                DefaultChannelHandlerContext next = context.Next;
                if (null != prev)
                {
                    prev.Next = next;
                }
                if (null != next)
                {
                    next.Prev = prev;
                }
            }
        }

        public void Replace(ChannelHandler oldHandler, string newName, ChannelHandler newHandler)
        {
            DefaultChannelHandlerContext newContext = new DefaultChannelHandlerContext(this, newName, newHandler);
            DefaultChannelHandlerContext context = Head;
            if (context != null && context == oldHandler)
            {
                newContext.Prev = context.Prev;
                newContext.Next = context.Next;

                DefaultChannelHandlerContext prev = context.Prev;
                DefaultChannelHandlerContext next = context.Next;
                if (null != prev)
                {
                    prev.Next = newContext;
                }
                if (null != next)
                {
                    next.Prev = newContext;
                }
            }
        }

        public void FireChannelActive()
        {
            Head?.FireChannelActive();
        }

        public void FireChannelInactive()
        {
            Head?.FireChannelInactive();
        }

        public void FireChannelRead(object msg)
        {
            Head?.FireChannelRead(msg);
        }

        public void FireChannelRegistered()
        {
            Head?.FireChannelRegistered();
        }

        public void FireChannelUnregistered()
        {
            Head?.FireChannelUnregistered();
        }

        public void FireExceptionCaught(Exception e)
        {
            Head?.FireExceptionCaught(e);
        }

        public ChannelFuture Write(object msg)
        {
            return Tail?.Write(msg);
        }
    }
}
