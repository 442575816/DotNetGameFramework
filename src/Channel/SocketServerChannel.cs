using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetGameFramework
{
    public class SocketServerChannel : DefaultAttributeMap, Channel
    {
        // 判断平台属性
        private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        // 默认最小分配buff大小
        private static readonly int MinAllocBufferSize = SlabMemoryPool.BlockSize / 2;

        /// <summary>
        /// ChannelId
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// SocketServer
        /// </summary>
        public SocketServer SocketServer { get; }

        /// <summary>
        /// Socket
        /// </summary>
        public Socket Socket { get; private set; }

        /// <summary>
        /// 是否连接中
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// ChannelPipeline
        /// </summary>
        public ChannelPipeline ChannelPipeline { get; private set; }

        /// <summary>
        /// 接受请求
        /// </summary>
        private readonly SocketReceiver _socketReceiver;

        /// <summary>
        /// 发送请求
        /// </summary>
        private readonly SocketSender _socketSender;

        /// <summary>
        /// 接受buffpool
        /// </summary>
        private MemoryPool<byte> MemoryPool { get; }

        /// <summary>
        /// 分配buff之前先等待数据
        /// </summary>
        private readonly bool _waitForData;

        /// <summary>
        /// 发送队列
        /// </summary>
        private ConcurrentQueue<ByteBuf> sendQueue = new ConcurrentQueue<ByteBuf>();

        /// <summary>
        /// 应用往传输层Pipeline
        /// </summary>
        public IDuplexPipe ApplicationToTransport { get; }

        /// <summary>
        /// 传输层往应用层Pipeline
        /// </summary>
        public IDuplexPipe TransportToApplication { get; }

        /// <summary>
        /// Socket读取到应用层的Pipeline
        /// </summary>
        public PipeWriter Input => TransportToApplication.Output;

        /// <summary>
        /// 读取需要写入到Socket的Pipeline
        /// </summary>
        public PipeReader Output => TransportToApplication.Input;

        // 正在处理的任务
        private Task _processingTask;


        private volatile bool _socketDisposed;
        private readonly object _shutdownLock = new object();


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="socketServer"></param>
        public SocketServerChannel(Socket socket,
                                   SocketServer socketServer,
                                   MemoryPool<byte> memoryPool,
                                   PipeScheduler transportScheduler,
                                   bool useInlineSchedulers = false,
                                   bool waitForData = true)
        {
            Id = Guid.NewGuid().ToString();
            SocketServer = socketServer;
            MemoryPool = memoryPool;
            _waitForData = waitForData;

            // 初始化Socket相关
            Socket = socket;


            // 初始化Pipeline
            var applicationScheduler = PipeScheduler.ThreadPool;
            var awaiterScheduler = IsWindows ? transportScheduler : PipeScheduler.Inline;
            if (useInlineSchedulers)
            {
                transportScheduler = PipeScheduler.Inline;
                awaiterScheduler = PipeScheduler.Inline;
                applicationScheduler = PipeScheduler.Inline;
            }

            var inputOptions = new PipeOptions(MemoryPool, applicationScheduler, transportScheduler, socketServer.SocketOptions.MaxReadBufferSize, socketServer.SocketOptions.MaxReadBufferSize / 2, useSynchronizationContext: false);
            var outputOptions = new PipeOptions(MemoryPool, transportScheduler, applicationScheduler, socketServer.SocketOptions.MaxWriteBufferSize, socketServer.SocketOptions.MaxWriteBufferSize / 2, useSynchronizationContext: false);

            var pair = DuplexPipe.CreateConnectionPair(inputOptions, outputOptions);

            ApplicationToTransport = pair.Item1;
            TransportToApplication = pair.Item2;

            _socketReceiver = new SocketReceiver(socket, awaiterScheduler);
            _socketSender = new SocketSender(socket, awaiterScheduler);

            ChannelPipeline = new DefaultChannelPipeline(this);


            IsConnected = true;

            //Console.WriteLine($"{Id} create");
        }

        /// <summary>
        /// 开始工作
        /// </summary>
        public void Start()
        {
            _processingTask = StartAsync();
        }

       
        /// <summary>
        /// 异步开始
        /// </summary>
        /// <returns></returns>
        private async Task StartAsync()
        {
            try
            {
                var recvTask = DoReceive();
                var sendTask = DoSend();
                var recvToAppTask = DoAppDataReceive();
                await recvTask;
                await sendTask;
                await recvToAppTask;

                _socketReceiver.Dispose();
                _socketSender.Dispose();

                //Console.WriteLine($"{Id} closed");
            } catch (Exception e)
            {
                ChannelPipeline.FireExceptionCaught(new Exception($"Unexpceted exception in {Id}", e));
            }
        }


        #region socket接受数据
        /// <summary>
        /// 开始接收任务
        /// </summary>
        /// <returns></returns>
        private async Task DoReceive()
        {
            Exception error = null;

            try
            {
                await ProcessReceives();
            }
            catch (SocketException ex) when (IsConnectionResetError(ex.SocketErrorCode))
            {
                // This could be ignored if _shutdownReason is already set.
                error = new Exception(ex.Message, ex);

            }
            catch (Exception ex)
                when ((ex is SocketException socketEx && IsConnectionAbortError(socketEx.SocketErrorCode)) ||
                       ex is ObjectDisposedException)
            {
                // This exception should always be ignored because _shutdownReason should be set.
                error = ex;

            }
            catch (Exception ex)
            {
                // This is unexpected.
                error = ex;
            }
            finally
            {
                //Console.WriteLine($"{Id} read finish");

                // If Shutdown() has already bee called, assume that was the reason ProcessReceives() exited.
                Input.Complete(error);

                // 触发链接关闭
                Close();
            }

        }

        /// <summary>
        /// 处理信息接收
        /// </summary>
        /// <returns></returns>
        private async Task ProcessReceives()
        {
            var input = Input;
            while (true)
            {
                if (!IsConnected)
                {
                    break;
                }

                if (_waitForData)
                {
                    await _socketReceiver.WaitForDataAsync();
                }

                var buffer = input.GetMemory(MinAllocBufferSize);
                var bytesReceived = await _socketReceiver.ReceiveAsync(buffer);

                if (bytesReceived == 0)
                {
                    break;
                }
                input.Advance(bytesReceived);

                var flushTask = input.FlushAsync();
                var result = await flushTask;

                if (result.IsCompleted || result.IsCanceled)
                {
                    // Pipe consumer is shut down, do we stop writing
                    break;
                }
            }
        }
        #endregion

        #region socket发送数据

        /// <summary>
        /// 异步发送数据
        /// </summary>
        /// <param name="buf"></param>
        public void SendAsync(ByteBuf buf)
        {
            try
            {
                if (!IsConnected)
                {
                    return;
                }

                if (buf.ReadableBytes == 0)
                {
                    return;
                }

                var output = ApplicationToTransport.Output;
                var buff = output.GetMemory(MinAllocBufferSize);

                // FIXME 是否不是拷贝，会在没发送之前被改写
                buf.Data.AsMemory<byte>().Slice(buf.ReaderIndex, buf.ReadableBytes).CopyTo(buff);
                output.Advance(buf.ReadableBytes);
                output.FlushAsync();
            }
            finally
            {
                buf.Release();
            }
        }

        /// <summary>
        /// 处理消息发送
        /// </summary>
        /// <returns></returns>
        private async Task DoSend()
        {
            Exception error = null;

            try
            {
                await ProcessSends();
            }
            catch (SocketException ex) when (IsConnectionResetError(ex.SocketErrorCode))
            {
                error = new Exception(ex.Message, ex);
            }
            catch (Exception ex)
                when ((ex is SocketException socketEx && IsConnectionAbortError(socketEx.SocketErrorCode)) ||
                       ex is ObjectDisposedException)
            {
                // This should always be ignored since Shutdown() must have already been called by Abort().
                error = ex;
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                //Console.WriteLine($"{Id} send finish");

                Shutdown();

                // Complete the output after disposing the socket
                Output.Complete(error);

                // Cancel any pending flushes so that the input loop is un-paused
                Input.CancelPendingFlush();
            }
        }

        /// <summary>
        /// 处理发送
        /// </summary>
        /// <returns></returns>
        private async Task ProcessSends()
        {
            var output = Output;
            while (true)
            {
                if (!IsConnected)
                {
                    break;
                }

                var result = await output.ReadAsync();
                if (result.IsCanceled)
                {
                    break;
                }

                var buffer = result.Buffer;
                var end = buffer.End;
                var isCompleted = result.IsCompleted;
                if (!buffer.IsEmpty)
                {
                    await _socketSender.SendAsync(buffer);
                }

                output.AdvanceTo(end);

                if (isCompleted)
                {
                    break;
                }
            }
        }
        #endregion

        #region socket接受到的数据发送给应用层
        /// <summary>
        /// 处理消息发送
        /// </summary>
        /// <returns></returns>
        private async Task DoAppDataReceive()
        {
            Exception error = null;

            try
            {
                await ProcessAppDataReceives();
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                //Console.WriteLine($"{Id} app data read finish");

                // 结束流
                ApplicationToTransport.Input.Complete(error);
            }
        }

        /// <summary>
        /// 接收数据到应用层
        /// </summary>
        /// <returns></returns>
        public async Task ProcessAppDataReceives()
        {
            var input = ApplicationToTransport.Input;
            while (true)
            {

                var result = await input.ReadAsync();
                if (result.IsCanceled)
                {
                    break;
                }

                var buffer = result.Buffer;
                var isCompleted = result.IsCompleted;
                if (!buffer.IsEmpty)
                {
                    //await Task.Run(() =>
                    //{

                    //});
                    ChannelPipeline.FireChannelRead(buffer);
                }

                if (isCompleted)
                {
                    break;
                }
            }

        }
        #endregion

        /// <summary>
        /// 写消息
        /// </summary>
        /// <param name="msg"></param>
        public ChannelFuture Write(object msg)
        {
            return ChannelPipeline.Write(msg);
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            // 关闭流
            ApplicationToTransport.Input.Complete();
            ApplicationToTransport.Output.Complete();
        }


        /// <summary>
        /// 断开连接
        /// </summary>
        public void Shutdown()
        {
            if (_socketDisposed)
            {
                return;
            }

            //Console.WriteLine($"{Id} shutdown");

            lock (_shutdownLock)
            {
                if (_socketDisposed)
                {
                    return;
                }

               

                // Make sure to close the connection only after the _aborted flag is set.
                // Without this, the RequestsCanBeAbortedMidRead test will sometimes fail when
                // a BadHttpRequestException is thrown instead of a TaskCanceledException.
                _socketDisposed = true;
                IsConnected = false;

                // 关闭流
                TransportToApplication.Output.CancelPendingFlush();
                TransportToApplication.Output.Complete();
                

                ApplicationToTransport.Output.CancelPendingFlush();
                ApplicationToTransport.Output.Complete();

                TransportToApplication.Input.Complete();
                ApplicationToTransport.Input.Complete();
                

                try
                {
                    // Try to gracefully close the socket even for aborts to match libuv behavior.
                    Socket.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                    // Ignore any errors from Socket.Shutdown() since we're tearing down the connection anyway.
                }

                try
                {
                    Socket.Dispose();
                } catch
                {
                    // Ignore any errors from Socket.Dispose() since we're tearing down the connection anyway.
                }

                ChannelPipeline.FireChannelInactive();
                SocketServer.UnregisteredChannel(this);
                ChannelPipeline.FireChannelUnregistered();
            }
        }

        /// <summary>
        /// 是否是连接被重置的错误
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        private static bool IsConnectionResetError(SocketError errorCode)
        {
            return errorCode == SocketError.ConnectionReset ||
                   errorCode == SocketError.Shutdown ||
                   (errorCode == SocketError.ConnectionAborted && IsWindows);
        }

        /// <summary>
        /// 是否是连接被终止的错误
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        private static bool IsConnectionAbortError(SocketError errorCode)
        {
            // Calling Dispose after ReceiveAsync can cause an "InvalidArgument" error on *nix.
            return errorCode == SocketError.OperationAborted ||
                   errorCode == SocketError.Interrupted ||
                   (errorCode == SocketError.InvalidArgument && !IsWindows);
        }
    }
}