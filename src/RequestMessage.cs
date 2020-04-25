using System;
namespace DotNetTcpFramework
{
    public class RequestMessage
    {
        public int RequestId { get; set; }
        public string Command { get; set; }
        public string Content { get; set; }

    }
}
