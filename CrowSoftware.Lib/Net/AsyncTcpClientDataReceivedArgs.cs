using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrowSoftware.Lib.Net
{
    public class AsyncTcpClientDataReceivedArgs: EventArgs
    {
        public AsyncTcpClient Client { get; private set; }
        public byte[] Data { get; private set; }

        public AsyncTcpClientDataReceivedArgs(AsyncTcpClient client, byte[] data)
        {
            Client = client;
            Data = data;
        }
    }
}
