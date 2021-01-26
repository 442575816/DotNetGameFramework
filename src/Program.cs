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
    interface ResultTest<out T>
    {
        string ViewName { get; }
        T Result { get; }
    }

    interface ViewTest<T>
    {
        void Render(ResultTest<T> result);
    }

    class ByteResult<T> : ResultTest<T>
    {
        T _result;

        public ByteResult(T result)
        {
            _result = result;
        }

        public string ViewName => "byte";

        public T Result => _result;
    }

    class ByteView : ViewTest<byte>
    {
        

        public void Render(ResultTest<byte> result)
        {
            var type = typeof(byte);
            Console.WriteLine($" type is {type.Name}");
        }
    }

    class A
    {

    }

    class B : A
    {

    }

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

            Dictionary<String, object> resultDict = new Dictionary<string, object>();
            Dictionary<String, object> viewDict = new Dictionary<string, object>();
            resultDict["byte"] = new ByteResult<byte>(11);
            viewDict["byte"] = new ByteView();

            var result = resultDict["byte"];


            var view = viewDict["byte"];

            Console.WriteLine($"the view is {view}");

            MethodInfo mi = view.GetType().GetMethod("Render");
            mi.Invoke(view, new object[] { result });
            //_view.Render(_result);


            // 测试Services
            ServiceCollection services = new ServiceCollection();
            services.AddSingleton<ByteResult<byte>>();

            var provider = services.BuildServiceProvider();
            Console.WriteLine($"provider:{provider.GetService(typeof(ByteResult<byte>))} ");

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
