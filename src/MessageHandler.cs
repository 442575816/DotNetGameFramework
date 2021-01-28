using System;
using System.Text;
using System.Threading.Tasks;

namespace DotNetGameFramework
{
    public class MessageHandler : ChannelInboundHandlerAdapter
    {
        /// <summary>
        /// Servlet
        /// </summary>
        private Servlet Servlet { get; set; }

        /// <summary>
        /// ServletContext
        /// </summary>
        private ServletContext ServletContext { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="servlet"></param>
        /// <param name="context"></param>
        public MessageHandler(Servlet servlet, ServletContext context)
        {
            Servlet = servlet;
            ServletContext = context;
        }


        /// <inheritdoc/>
        public override void FireChannelRegistered(ChannelHandlerContext context)
        {
            // 默认分配一个Session
            var session = SessionManager.Instance.GetNewSession();
            session.SetPush(new TcpPush(context.Channel));
            context.Channel.AddAttribute(Constants.SESSION_ID, session.Id);

            base.FireChannelActive(context);
        }

        /// <inheritdoc/>
        public override void FireChannelRead(ChannelHandlerContext context, object msg)
        {
            RequestMessage message = (RequestMessage)msg;
            message.SessionId = context.Channel.GetAttribute<string>(Constants.SESSION_ID);


            var request = new TcpRequest(context.Channel, ServletContext, message);
            var response = new TcpResponse(context.Channel);

            Servlet.Service(request, response);
            //string response = $"hello world 111";
            //byte[] bodyBytes = Encoding.UTF8.GetBytes(response);
            //byte[] commandBytes = Encoding.UTF8.GetBytes(message.Command);
            //byte[] array = new byte[32];
            //Array.Copy(commandBytes, array, commandBytes.Length);

            //ByteBuf buff = new ByteBuf();
            //buff.Retain();
            //buff.WriteInt(36 + bodyBytes.Length);
            //buff.WriteBytes(array);
            //buff.WriteInt(0);
            //buff.WriteBytes(bodyBytes);

            //context.Write(buff);
            return;

        }
    }
}
