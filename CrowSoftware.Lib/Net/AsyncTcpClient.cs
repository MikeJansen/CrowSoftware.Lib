using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using Castle.Core.Logging;
using CrowSoftware.Common.Log;

namespace CrowSoftware.Lib.Net
{
    public class AsyncTcpClient : IAsyncTcpClient
    {
        #region Private Enums

        private enum Input
        {
            Initialize,
            Connect,
            Send,
            Reset,
            Disconnect
        }

        #endregion

        #region Private Fields

        private AsyncTcpClientState _state = AsyncTcpClientState.None;
        private string _tag;
        private TcpClient _tcpClient;
        private string _address;
        private int _port;
        private readonly Queue<byte[]> _sendQueue = new Queue<byte[]>();
        private Timer _connectWaitTimer;
        private byte[] _receiveBuffer;

        #endregion

        #region Public Properties

        public ILogManager Log { get; set; }
        public ILogger Logger { get; set; }

        public event EventHandler<AsyncTcpClientEventArgs> StateTransitioned;
        public event EventHandler<AsyncTcpClientDataReceivedArgs> DataReceived;

        public AsyncTcpClientState State
        {
            get { return _state; }
            private set
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                Log.EnterMethod(Logger, currentMethod);
                try
                {
                    // **************************************************

                    AsyncTcpClientState oldState = _state;
                    _state = value;
                    Logger.InfoFormat(CultureInfo.InvariantCulture, "{2}: State transition from '{0}' to '{1}'",
                        oldState, value, _tag);

                    EventHandler<AsyncTcpClientEventArgs> handler = StateTransitioned;
                    if (handler != null)
                    {
                        handler(this, new AsyncTcpClientEventArgs(this, oldState, value));
                    }

                    // **************************************************
                }
                catch (Exception ex)
                {
                    Logger.Error("Unhandled exception. Rethrowing.", ex);
                    throw;
                }
                finally
                {
                    Log.ExitMethod(Logger, currentMethod);
                }

            }
        }

        public Exception Exception { get; private set; }
        public string Address { get { return _address; } }
        public int Port { get { return _port; } }
        public string Tag { get { return _tag; } }

        #endregion

        #region Public Methods

        public void Initialize(string tag, string address, int port)
        {
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            Log.EnterMethod(Logger, currentMethod);
            try
            {
                // **************************************************

                RunStateMachine(Input.Initialize, tag, address, port);

                // **************************************************
            }
            catch (Exception ex)
            {
                Logger.Error("Unhandled exception. Rethrowing.", ex);
                throw;
            }
            finally
            {
                Log.ExitMethod(Logger, currentMethod);
            }

        }

        public void Connect()
        {
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            Log.EnterMethod(Logger, currentMethod);
            try
            {
                // **************************************************

                RunStateMachine(Input.Connect);

                // **************************************************
            }
            catch (Exception ex)
            {
                Logger.Error("Unhandled exception. Rethrowing.", ex);
                throw;
            }
            finally
            {
                Log.ExitMethod(Logger, currentMethod);
            }

        }

        public void Send(byte[] data)
        {
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            Log.EnterMethod(Logger, currentMethod);
            try
            {
                // **************************************************

                RunStateMachine(Input.Send, data);

                // **************************************************
            }
            catch (Exception ex)
            {
                Logger.Error("Unhandled exception. Rethrowing.", ex);
                throw;
            }
            finally
            {
                Log.ExitMethod(Logger, currentMethod);
            }

        }

        public void Disconnect()
        {
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            Log.EnterMethod(Logger, currentMethod);
            try
            {
                // **************************************************

                RunStateMachine(Input.Disconnect);

                // **************************************************
            }
            catch (Exception ex)
            {
                Logger.Error("Unhandled exception. Rethrowing.", ex);
                throw;
            }
            finally
            {
                Log.ExitMethod(Logger, currentMethod);
            }

        }

        public void Reset()
        {
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            Log.EnterMethod(Logger, currentMethod);
            try
            {
                // **************************************************

                RunStateMachine(Input.Reset);

                // **************************************************
            }
            catch (Exception ex)
            {
                Logger.Error("Unhandled exception. Rethrowing.", ex);
                throw;
            }
            finally
            {
                Log.ExitMethod(Logger, currentMethod);
            }

        }

        #endregion

        #region Private Methods

        private void RunStateMachine(Input input, params object[] args)
        {
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            Log.EnterMethod(Logger, currentMethod);
            try
            {
                // **************************************************

                bool isUnhandledInput = false;
                switch (_state)
                {
                    case AsyncTcpClientState.None:
                        switch (input)
                        {
                            case Input.Initialize:
                                _tag = (string)args[0];
                                _address = (string)args[1];
                                _port = (int)args[2];
                                State = AsyncTcpClientState.Disconnected;
                                break;
                            case Input.Connect:
                            case Input.Send:
                            case Input.Reset:
                            case Input.Disconnect:
                            default:
                                isUnhandledInput = true;
                                break;
                        }
                        break;
                    case AsyncTcpClientState.Error:
                        switch (input)
                        {
                            case Input.Initialize:
                            case Input.Connect:
                            case Input.Send:
                            case Input.Reset:
                            case Input.Disconnect:
                            default:
                                isUnhandledInput = true;
                                break;
                        }
                        break;
                    case AsyncTcpClientState.Disconnected:
                        switch (input)
                        {
                            case Input.Connect:
                                BeginConnect();
                                break;
                            case Input.Send:
                                _sendQueue.Enqueue((byte[])args[0]);
                                BeginConnect();
                                break;
                            case Input.Initialize:
                            case Input.Reset:
                            case Input.Disconnect:
                            default:
                                isUnhandledInput = true;
                                break;
                        }
                        break;
                    case AsyncTcpClientState.Connected:
                        switch (input)
                        {
                            case Input.Send:
                                _sendQueue.Enqueue((byte[])args[0]);
                                BeginSend();
                                break;
                            case Input.Disconnect:
                                DoDisconnect();
                                break;
                            case Input.Reset:
                                DoDisconnect();
                                BeginConnect();
                                break;
                            case Input.Connect:
                                break;
                            case Input.Initialize:
                            default:
                                isUnhandledInput = true;
                                break;
                        }
                        break;
                    case AsyncTcpClientState.Connecting:
                    case AsyncTcpClientState.ConnectingError:
                    case AsyncTcpClientState.ConnectingErrorWaiting:
                    case AsyncTcpClientState.Sending:
                    case AsyncTcpClientState.SendError:
                    case AsyncTcpClientState.ReceiveError:
                    case AsyncTcpClientState.Resetting:
                    case AsyncTcpClientState.Disconnecting:
                        switch (input)
                        {
                            case Input.Send:
                                _sendQueue.Enqueue((byte[])args[0]);
                                break;
                            case Input.Initialize:
                            case Input.Connect:
                            case Input.Reset:
                                DoDisconnect();
                                BeginConnect();
                                break;
                            case Input.Disconnect:
                                DoDisconnect();
                                break;
                            default:
                                isUnhandledInput = true;
                                break;
                        }
                        break;
                    default:
                        isUnhandledInput = true;
                        break;
                }

                // **************************************************
            }
            catch (Exception ex)
            {
                Logger.Error("Unhandled exception. Rethrowing.", ex);
                throw;
            }
            finally
            {
                Log.ExitMethod(Logger, currentMethod);
            }

        }

        private void DoDisconnect()
        {
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            Log.EnterMethod(Logger, currentMethod);
            try
            {
                // **************************************************

                if (_tcpClient != null)
                {
                    State = AsyncTcpClientState.Disconnecting;
                    _tcpClient.Close();
                    _tcpClient = null;
                    _receiveBuffer = null;
                    State = AsyncTcpClientState.Disconnected;
                }

                // **************************************************
            }
            catch (Exception ex)
            {
                Logger.Error("Unhandled exception. Rethrowing.", ex);
                throw;
            }
            finally
            {
                Log.ExitMethod(Logger, currentMethod);
            }

        }

        private void BeginConnect()
        {
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            Log.EnterMethod(Logger, currentMethod);
            try
            {
                // **************************************************

                State = AsyncTcpClientState.Connecting;
                _tcpClient = new TcpClient();
                _tcpClient.NoDelay = true;
                IAsyncResult asyncResult = _tcpClient.BeginConnect(_address, _port, ConnectCallback, null);
                if (asyncResult.CompletedSynchronously)
                {
                    ConnectCallback(asyncResult);
                }

                // **************************************************
            }
            catch (Exception ex)
            {
                ConnectionError(ex);
            }
            finally
            {
                Log.ExitMethod(Logger, currentMethod);
            }

        }

        private void ConnectionError(Exception ex)
        {
            Logger.ErrorFormat(ex, CultureInfo.InvariantCulture, "{0}: Error during connection.", _tag);
            Exception = ex;
            State = AsyncTcpClientState.ConnectingErrorWaiting;
            _connectWaitTimer = new Timer(ConnectWaitTimerHandler, null, 10000, Timeout.Infinite);
        }

        private void ConnectCallback(IAsyncResult asyncResult)
        {
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            Log.EnterMethod(Logger, currentMethod);
            try
            {
                // **************************************************

                if (_tcpClient != null)
                {
                    _tcpClient.EndConnect(asyncResult);
                    State = AsyncTcpClientState.Connected;

                    _receiveBuffer = new byte[_tcpClient.ReceiveBufferSize];
                    BeginReceive();

                    if (_sendQueue.Count > 0)
                    {
                        BeginSend();
                    }
                }

                // **************************************************
            }
            catch (Exception ex)
            {
                ConnectionError(ex);
            }
            finally
            {
                Log.ExitMethod(Logger, currentMethod);
            }

        }

        private void BeginReceive()
        {
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            Log.EnterMethod(Logger, currentMethod);
            try
            {
                // **************************************************

                IAsyncResult asyncResult;
                do
                {
                    asyncResult = _tcpClient.GetStream().BeginRead(_receiveBuffer, 0, _receiveBuffer.Length, ReceiveCallback, null);
                    if (asyncResult.CompletedSynchronously)
                    {
                        HandleReceivedData(asyncResult);
                    }
                } while (asyncResult.CompletedSynchronously);

                // **************************************************
            }
            catch (Exception ex)
            {
                ReceiveError(ex);
            }
            finally
            {
                Log.ExitMethod(Logger, currentMethod);
            }

        }

        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            Log.EnterMethod(Logger, currentMethod);
            try
            {
                // **************************************************

                if (_tcpClient != null)
                {
                    HandleReceivedData(asyncResult);
                    BeginReceive();
                }

                // **************************************************
            }
            catch (Exception ex)
            {
                ReceiveError(ex);
            }
            finally
            {
                Log.ExitMethod(Logger, currentMethod);
            }

        }

        private void HandleReceivedData(IAsyncResult asyncResult)
        {
            int bytesRead = _tcpClient.GetStream().EndRead(asyncResult);
            if (bytesRead > 0)
            {
                EventHandler<AsyncTcpClientDataReceivedArgs> handler = DataReceived;
                if (handler != null)
                {
                    byte[] data = new byte[bytesRead];
                    Array.Copy(_receiveBuffer, data, bytesRead);
                    handler(this, new AsyncTcpClientDataReceivedArgs(this, data));
                }
            }
        }

        private void BeginSend()
        {
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            Log.EnterMethod(Logger, currentMethod);
            try
            {
                // **************************************************

                if (_sendQueue.Count > 0)
                {
                    State = AsyncTcpClientState.Sending;
                    byte[] data = _sendQueue.Dequeue();
                    IAsyncResult asyncResult =
                        _tcpClient.GetStream().BeginWrite(data, 0, data.Length, SendCallback, null);
                    if (asyncResult.CompletedSynchronously)
                    {
                        SendCallback(asyncResult);
                    }
                }

                // **************************************************
            }
            catch (Exception ex)
            {
                SendError(ex);
            }
            finally
            {
                Log.ExitMethod(Logger, currentMethod);
            }

        }

        private void SendCallback(IAsyncResult asyncResult)
        {
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            Log.EnterMethod(Logger, currentMethod);
            try
            {
                // **************************************************

                if (_tcpClient != null)
                {
                    _tcpClient.GetStream().EndWrite(asyncResult);
                    if (_sendQueue.Count > 0)
                    {
                        BeginSend();
                    }
                    else
                    {
                        State = AsyncTcpClientState.Connected;
                    }
                }

                // **************************************************
            }
            catch (Exception ex)
            {
                SendError(ex);
            }
            finally
            {
                Log.ExitMethod(Logger, currentMethod);
            }

        }

        private void SendError(Exception ex)
        {
            Logger.ErrorFormat(ex, CultureInfo.InvariantCulture, "{0}: Error during send.", _tag);
            Exception = ex;
            State = AsyncTcpClientState.SendError;
            Reset();
        }

        private void ReceiveError(Exception ex)
        {
            Logger.ErrorFormat(ex, CultureInfo.InvariantCulture, "{0}: Error during receive.", _tag);
            Exception = ex;
            if (State != AsyncTcpClientState.Resetting && State != AsyncTcpClientState.Disconnecting &&
                State != AsyncTcpClientState.Disconnected)
            {
                State = AsyncTcpClientState.ReceiveError;
                Reset();
            }
        }

        private void ConnectWaitTimerHandler(object state)
        {
            MethodBase currentMethod = MethodBase.GetCurrentMethod();
            Log.EnterMethod(Logger, currentMethod);
            try
            {
                // **************************************************

                if (_tcpClient != null && _connectWaitTimer != null)
                {
                    _connectWaitTimer.Dispose();
                    _connectWaitTimer = null;
                    State = AsyncTcpClientState.ConnectingError;
                    RunStateMachine(Input.Connect);
                }

                // **************************************************
            }
            catch (Exception ex)
            {
                Logger.Error("Unhandled exception.", ex);
            }
            finally
            {
                Log.ExitMethod(Logger, currentMethod);
            }

        }
        #endregion


    }
}

