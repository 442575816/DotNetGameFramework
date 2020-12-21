using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Linq;

namespace DotNetGameFramework
{
    class Program
    {
        static void Main(string[] args)
        {
            SocketOption option = new SocketOption();
            //SocketServer server = new SocketServer("127.0.0.1", "8088", option);
            //server.ServerHandler = new MyServerHandler();
            //server.Bind();
            //server.AcceptAsync();
            //Console.WriteLine("Start");
            //var task = DoSomething1();
            //DoSomething2();
            //DoSomething3();

            HttpServer httpServer = new HttpServer("127.0.0.1", 8080, option);
            httpServer.Start();



            //Console.WriteLine($"End {task.Result}");
            Console.Read();
        }

        static async Task<int> DoSomething1()
        {
            await Task.Delay(1000);
            Console.WriteLine("DoSomething1");
            return 1;
        }

        static async Task<int> DoSomething2()
        {
            await Task.Delay(2000);
            Console.WriteLine("DoSomething2");
            return 2;
        }

        static async Task<int> DoSomething3()
        {
            await Task.Delay(1000);
            Console.WriteLine("DoSomething3");
            return 3;
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
