using System;
using System.Threading.Tasks;

namespace DotNetGameFramework
{
    class Program
    {
        static void Main(string[] args)
        {
            SocketServer server = new SocketServer("127.0.0.1", "8088");
            server.ServerHandler = new MyServerHandler();
            server.Start();
            Console.Read();
        }
    }

    class MyServerHandler : ServerHandler
    {
        public void FireExceptionCaught(Exception e)
        {
            Console.WriteLine(e);
        }

        public void InitChannel(SocketServerChannel channel)
        {
            channel.ChannelPipeline.AddLast("decoder", new MessageDecoder());
            channel.ChannelPipeline.AddLast("handler", new MessageHandler());
            Console.WriteLine($"new connnect {channel}");
        }
    }
}
