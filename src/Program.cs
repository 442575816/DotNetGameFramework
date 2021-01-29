using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using System.Collections.Concurrent;

namespace DotNetGameFramework
{

    /// <summary>
    /// out 协变 只能是返回结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICustomerListOut<out T>
    {
        T Get();
    }

    public class CustomerListOut<T> : ICustomerListOut<T>
    {
        public T Get()
        {
            return default(T);
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            //SocketOption option = new SocketOption();
            //SocketServer server = new SocketServer("127.0.0.1", "8088", option);
            //server.ServerHandler = new MyServerHandler();
            //server.Bind();
            //server.AcceptAsync();
            //Console.WriteLine("Start");
            //var task = DoSomething1();
            //DoSomething2();
            //DoSomething3();

            //HttpServer httpServer = new HttpServer("127.0.0.1", 8080, option);
            //httpServer.Start();



            //Console.WriteLine($"End {task.Result}");

           

            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict["a"] = "1";
            //var b = dict["b"];

            int? b1 = null;
            Console.WriteLine($"b1 is {b1 == 0}");

            Console.WriteLine($"session is {SessionManager.Instance}");

            int count =16;
            byte[] bytes = new byte[count];
            RandomNumberGenerator.Fill(bytes);

            Stopwatch stopwatch = Stopwatch.StartNew();
            HashSet<string> set = new HashSet<string>();
            int num = 0;
            for (var i = 1; i < 100; i++)
            {
                var id = SessionManager.Instance.CreateNewSessionId();
                if (set.Contains(id))
                {
                    num++;
                    continue;
                }
                set.Add(id);

            }
            stopwatch.Stop();
            Console.WriteLine($"create time:{stopwatch.ElapsedMilliseconds}, num:{num}");

            Console.WriteLine($"uuid:{SessionManager.Instance.CreateNewSessionId()}");
            Console.WriteLine($"uuid:{SessionManager.Instance.CreateNewSessionId()}");
            Console.WriteLine($"uuid:{SessionManager.Instance.CreateNewSessionId()}");
            Console.WriteLine($"uuid:{SessionManager.Instance.CreateNewSessionId()}");

            Dictionary<string, int> dict1 = new Dictionary<string, int>();
            dict1.TryAdd("1", 1);
            Console.WriteLine($"map try add:{dict1.TryAdd("1", 2)}");
            Console.WriteLine($"map try add:{dict1["1"]}");
            Console.WriteLine($"map try add:{dict1.TryAdd("2", 2)}");

            Console.WriteLine($"timestamp:{TimeUtil.Timestamp}");

            Thread.Sleep(1000);

            Console.WriteLine($"timestamp:{TimeUtil.Timestamp}");

            Console.WriteLine($"timestamp to datetime:{TimeUtil.GetDateTime(TimeUtil.Timestamp)}, {DateTime.Now}");


            ConcurrentDictionary<string, string> dict2 = null;
            Interlocked.CompareExchange(ref dict2, new ConcurrentDictionary<string, string>(), null);

            Console.WriteLine($"cas dict :{dict2.GetHashCode()}");

            Interlocked.CompareExchange(ref dict2, new ConcurrentDictionary<string, string>(), null);
            dict2["a"] = "1";
            dict2["b"] = "2";

            foreach(var item in dict2)
            {
                if (item.Key == "a")
                {
                    dict2.TryRemove("a", out _);
                    continue;
                }
                Console.WriteLine($"dict remove :{item.Value}");
            }


            Console.WriteLine($"cas dict :{dict2.GetHashCode()}");

            ByteBuf buf = new ByteBuf(50);
            buf.WriteInt(46);

            byte[] bs = Encoding.UTF8.GetBytes("helloworld");
            byte[] bs1 = new byte[32];
            Array.Copy(bs, bs1, bs.Length);

            buf.WriteBytes(bs1);
            buf.WriteInt(0);
            buf.WriteBytes(Encoding.UTF8.GetBytes("helloworld"));
            Console.WriteLine();
            foreach (byte b in buf.Data)
            {
                var hex = string.Format("{0:X}", b);
                if (hex.Length < 2)
                {
                    hex = "0" + hex;
                }
                Console.Write(hex);
            }
            Console.WriteLine();


            Console.WriteLine($"regex:{HttpUtil.GetCommand("/root1/helloworld.action")}");

            StartTcpServer();

            //var waitForStop = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            //waitForStop.Task.GetAwaiter().GetResult();
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


        static void StartTcpServer()
        {
            //ThreadPool.SetMaxThreads(8, 4);
            //ThreadPool.SetMinThreads(8, 4);
            var servlet = new DispatchServlet();
            var servletConfig = new XmlServletConfig("servlet.xml");
            var context = new DefaultServletContext();

            servlet.Init(servletConfig, context);
            SessionManager.Instance.ServletConfig = servletConfig;
            SessionManager.Instance.StartSessionCheckThread();
            SessionManager.Instance.SessionEvent += (eventType, sessionEvent) =>
            {
                //Console.WriteLine($"eventType:{eventType}");
            };
            SessionManager.Instance.SessionAttributeEvent += (eventType, SessionAttributeEvent) =>
            {
                //Console.WriteLine($"eventType:{eventType}");
            };
            servlet.AddHandler("helloworld", (request, response) =>
            {
                return new ByteResult(Encoding.UTF8.GetBytes("helloworld"));
            });
            servlet.AddView(new ByteView());

            SocketOption option = new SocketOption();
            WrapperUtil.ByteBufPool = new ByteBufPool<ByteBuf>(() => new ByteBuf(100), 5, 10);
            SocketServer server = new SocketServer("127.0.0.1", "8010", option);
            server.ServerHandler = new MyServerHandler(servlet, servletConfig, context);
            server.Bind();
            server.AcceptAsync();

            HttpServer httpServer = new HttpServer("127.0.0.1", 8080, option, new HttpDefaultHandler(servlet, context));
            httpServer.Start();
        }
    }

    

    class MyServerHandler : ServerHandler
    {
        private DispatchServlet servlet;
        private XmlServletConfig servletConfig;
        private DefaultServletContext context;

        public MyServerHandler(DispatchServlet servlet, XmlServletConfig servletConfig, DefaultServletContext context)
        {
            this.servlet = servlet;
            this.servletConfig = servletConfig;
            this.context = context;
        }

        public void FireExceptionCaught(Exception e)
        {
            Console.WriteLine(e);
        }

        public void InitChannel(SocketServerChannel channel)
        {
            channel.ChannelPipeline.AddLast("decoder", new MessageDecoder());
            channel.ChannelPipeline.AddLast("handler", new MessageHandler(servlet, context));
            //Console.WriteLine($"new connnect {channel}");
        }
    }
}
