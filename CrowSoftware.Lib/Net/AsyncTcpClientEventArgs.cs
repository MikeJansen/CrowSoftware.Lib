using System;

namespace CrowSoftware.Lib.Net
{
    public class AsyncTcpClientEventArgs: EventArgs
    {
        public AsyncTcpClient Client { get; private set; }
        public AsyncTcpClientState OldState { get; private set; }
        public AsyncTcpClientState NewState { get; private set; }

        public AsyncTcpClientEventArgs(AsyncTcpClient client, AsyncTcpClientState oldState, 
            AsyncTcpClientState newState)
        {
            Client = client;
            OldState = oldState;
            NewState = newState;
        }

    }
}
