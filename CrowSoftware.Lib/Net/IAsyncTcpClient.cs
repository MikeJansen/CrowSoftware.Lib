using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrowSoftware.Lib.Net
{
    public interface IAsyncTcpClient
    {
        AsyncTcpClientState State { get; }
        string Address { get; }
        int Port { get; }
        string Tag { get; }

        void Initialize(string tag, string address, int port);
        void Connect();
        void Send(byte[] data);
        void Disconnect();
        void Reset();

        event EventHandler<AsyncTcpClientEventArgs> StateTransitioned;
        event EventHandler<AsyncTcpClientDataReceivedArgs> DataReceived;
    }

    public enum AsyncTcpClientState
    {
        /// <summary>
        /// Uninitialized
        /// </summary>
        None,

        /// <summary>
        /// The object is in an invalid state
        /// </summary>
        Error,

        /// <summary>
        /// Initialized but disconnected
        /// </summary>
        Disconnected,

        /// <summary>
        /// In progress of making a connection to client
        /// </summary>
        Connecting,

        /// <summary>
        /// In progress of making a connection to client, previous attempt failed
        /// </summary>
        ConnectingError,

        /// <summary>
        /// Previous connect attempt failed, waiting before retrying
        /// </summary>
        ConnectingErrorWaiting,

        /// <summary>
        /// Successfully connect to client
        /// </summary>
        Connected,

        /// <summary>
        /// Sending data to client
        /// </summary>
        Sending,

        /// <summary>
        /// Error sending, connection will be reset
        /// </summary>
        SendError,

        
        /// <summary>
        /// Error receiving, connection will be reset
        /// </summary>
        ReceiveError,

        /// <summary>
        /// Resetting the connection (currently disconnecting; will be followed by a connect)
        /// </summary>
        Resetting,

        /// <summary>
        /// Disconnecting the connection
        /// </summary>
        Disconnecting
    }
}
