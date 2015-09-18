using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace AsyncTcpServer
{
    public class TcpClientConnectedEventArgs:EventArgs
    {
        public TcpClientConnectedEventArgs(string remoteEndPoint)
        {
            this.remoteEndPoint = remoteEndPoint;
        }

        public string remoteEndPoint { get; private set; }
    }

    public class TcpClientDisconnectedEventArgs:EventArgs
    {
        public TcpClientDisconnectedEventArgs(string remoteEndPoint)
        {
            this.remoteEndPoint = remoteEndPoint;
        }

        public string remoteEndPoint { get; private set; }
    }

    public class TcpDatagramReceivedEventArgs<T>:EventArgs
    {
        public T Datagram{ get; private set; }
        public string remoteEndPoint { get; private set; }

        public TcpDatagramReceivedEventArgs(string remoteEndPoint, T dataGram)
        {
            this.remoteEndPoint = remoteEndPoint;
            Datagram = dataGram;
        }        
    }

    /// <summary>
    /// 异步TCP服务器
    /// </summary>
    public class AsyncTcpServer : IDisposable
    {
        #region TcpClientState

        public class TcpClientState
        {
            public TcpClientState(TcpClient tcpc, Byte[] buf)
            {
                this.Buffer = buf;
                this.TcpClient = tcpc;
                this.remoteEndPoint = tcpc.Client.RemoteEndPoint.ToString();
                this.NetworkStream = tcpc.GetStream();
            }

            public Byte[] Buffer;
            public string remoteEndPoint;
            public TcpClient TcpClient;
            public NetworkStream NetworkStream;
        }

        #endregion

        #region Fields

        private TcpListener listener;
        private Dictionary<string,TcpClientState> clients;
        private bool disposed = false;
        public bool block = false;

        #endregion

        #region Ctors

        /// <summary>
        /// 异步TCP服务器
        /// </summary>
        /// <param name="listenPort">监听的端口</param>
        public AsyncTcpServer(int listenPort)
            : this(IPAddress.Any, listenPort)
        {
        }

        /// <summary>
        /// 异步TCP服务器
        /// </summary>
        /// <param name="localEP">监听的终结点</param>
        public AsyncTcpServer(IPEndPoint localEP)
            : this(localEP.Address, localEP.Port)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        ~AsyncTcpServer()
        {
 
        }

        /// <summary>
        /// 异步TCP服务器
        /// </summary>
        /// <param name="localIPAddress">监听的IP地址</param>
        /// <param name="listenPort">监听的端口</param>
        public AsyncTcpServer(IPAddress localIPAddress, int listenPort)
        {
            Address = localIPAddress;
            Port = listenPort;
            this.Encoding = Encoding.Default;

            clients = new Dictionary<string, TcpClientState>();

            listener = new TcpListener(Address, Port);
            listener.AllowNatTraversal(true);

            Thread thrd = new Thread(new ThreadStart(
                delegate {

                    Byte[] buf = new Byte[1];

                    //检测是否有死链接（网线断开）
                    Timer timer = new Timer(new TimerCallback(delegate
                    {
                        if (this.IsRunning)
                        {
                            foreach (var item in this.clients)
                            {
                                if (!item.Value.TcpClient.Connected) continue;

                                lock (item.Value)
                                {
                                    try
                                    {
                                        item.Value.NetworkStream.Write(buf, 0, 0);
                                    }
                                    catch
                                    {
                                        item.Value.TcpClient.Close();
                                        this.clients.Remove(item.Key);
                                    }
                                }                                
                            }
                        }
                    }),null,0,10000);

                    while (true)
                    {
                        lock (this.clients)
                        {
                            this.ClientCount = this.clients.Count;
                        }

                        if (this.listener == null) break;

                        Thread.Sleep(100);
                    }
                }
            ));

            thrd.Start();
        }

        #endregion

        #region Properties

        /// <summary>
        /// 服务器是否正在运行
        /// </summary>
        public bool IsRunning { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public int ClientCount { get; private set; }
        /// <summary>
        /// 监听的IP地址
        /// </summary>
        public IPAddress Address { get; private set; }
        /// <summary>
        /// 监听的端口
        /// </summary>
        public int Port { get; private set; }
        /// <summary>
        /// 通信使用的编码
        /// </summary>
        public Encoding Encoding { get; set; }

        #endregion

        #region Server

        /// <summary>
        /// 启动服务器
        /// </summary>
        /// <returns>异步TCP服务器</returns>
        public AsyncTcpServer Start()
        {
            if (!IsRunning)
            {
                IsRunning = true;
                listener.Start();
                listener.BeginAcceptTcpClient(
                  new AsyncCallback(HandleTcpClientAccepted), listener);
            }
            return this;
        }

        /// <summary>
        /// 启动服务器
        /// </summary>
        /// <param name="backlog">
        /// 服务器所允许的挂起连接序列的最大长度
        /// </param>
        /// <returns>异步TCP服务器</returns>
        public AsyncTcpServer Start(int backlog)
        {
            if (!IsRunning)
            {
                IsRunning = true;
                listener.Start(backlog);
                listener.BeginAcceptTcpClient(
                  new AsyncCallback(HandleTcpClientAccepted), listener);
            }
            return this;
        }

        /// <summary>
        /// 停止服务器
        /// </summary>
        /// <returns>异步TCP服务器</returns>
        public AsyncTcpServer Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                listener.Stop();

                lock (this.clients)
                {
                    foreach (var item in this.clients)
                    {
                        item.Value.TcpClient.Client.Disconnect(false);
                    }
                    this.clients.Clear();
                }

            }
            return this;
        }

        #endregion

        #region Receive

        private void HandleTcpClientAccepted(IAsyncResult ar)
        {
            if (IsRunning)
            {
                TcpListener tcpListener = (TcpListener)ar.AsyncState;

                TcpClient tcpClient = tcpListener.EndAcceptTcpClient(ar);
                byte[] buffer = new byte[tcpClient.ReceiveBufferSize];

                TcpClientState internalClient
                  = new TcpClientState(tcpClient, buffer);

                lock (this.clients)
                {
                    this.clients.Add(internalClient.remoteEndPoint, internalClient);
                    RaiseClientConnected(internalClient.remoteEndPoint);
                }

                NetworkStream networkStream = internalClient.NetworkStream;
                networkStream.BeginRead(
                  internalClient.Buffer,
                  0,
                  internalClient.Buffer.Length,
                  HandleDatagramReceived,
                  internalClient);

                tcpListener.BeginAcceptTcpClient(
                  new AsyncCallback(HandleTcpClientAccepted), ar.AsyncState);
            }
        }

        private void HandleDatagramReceived(IAsyncResult ar)
        {
            if (IsRunning)
            {
                try
                {
                    TcpClientState internalClient = (TcpClientState)ar.AsyncState;
                    NetworkStream networkStream = internalClient.NetworkStream;

                    int numberOfReadBytes = 0;
                    try
                    {
                        numberOfReadBytes = networkStream.EndRead(ar);
                    }
                    catch
                    {
                        numberOfReadBytes = 0;
                    }

                    if (numberOfReadBytes == 0)
                    {
                        // connection has been closed
                        lock (this.clients)
                        {
                            this.clients.Remove(internalClient.remoteEndPoint);
                            RaiseClientDisconnected(internalClient.remoteEndPoint);
                            internalClient.TcpClient.Close();
                            return;
                        }
                    }

                    // received byte and trigger event notification
                    byte[] receivedBytes = new byte[numberOfReadBytes];
                    Buffer.BlockCopy(
                      internalClient.Buffer, 0,
                      receivedBytes, 0, numberOfReadBytes);

                    RaiseDatagramReceived(internalClient.remoteEndPoint, receivedBytes);
                    RaisePlaintextReceived(internalClient.remoteEndPoint, receivedBytes);

                    // continue listening for tcp datagram packets
                    networkStream.BeginRead(
                      internalClient.Buffer,
                      0,
                      internalClient.Buffer.Length,
                      HandleDatagramReceived,
                      internalClient);
                }
                catch
                {
                    throw new ArgumentNullException("tcpClient Async Read Failed!");
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// 接收到数据报文事件
        /// </summary>
        public event EventHandler<TcpDatagramReceivedEventArgs<byte[]>> DatagramReceived;
        /// <summary>
        /// 接收到数据报文明文事件
        /// </summary>
        public event EventHandler<TcpDatagramReceivedEventArgs<string>> PlaintextReceived;

        private void RaiseDatagramReceived(string remoteEndPoint, byte[] datagram)
        {
            if (DatagramReceived != null)
            {
                if (this.block)
                {
                    DatagramReceived(this, new TcpDatagramReceivedEventArgs<byte[]>(remoteEndPoint, datagram));
                }
                else
                {
                    EventHandler eh = new EventHandler(delegate{});

                    eh.BeginInvoke(null, null, new AsyncCallback(delegate(IAsyncResult iar) {
                        DatagramReceived(this, new TcpDatagramReceivedEventArgs<byte[]>(remoteEndPoint, datagram));
                        eh.EndInvoke(iar);                    
                    }), null);
                }
            }
        }

        private void RaisePlaintextReceived(string remoteEndPoint, byte[] datagram)
        {
            if (PlaintextReceived != null)
            {
                if (this.block)
                {
                    PlaintextReceived(this, new TcpDatagramReceivedEventArgs<string>(
                      remoteEndPoint, this.Encoding.GetString(datagram, 0, datagram.Length)));
                }
                else
                {
                    EventHandler eh = new EventHandler(delegate
                    {
                        PlaintextReceived(this, new TcpDatagramReceivedEventArgs<string>(
                          remoteEndPoint, this.Encoding.GetString(datagram, 0, datagram.Length)));
                    });

                    eh.BeginInvoke(null, null, null, null);
                }
            }
        }

        /// <summary>
        /// 与客户端的连接已建立事件
        /// </summary>
        public event EventHandler<TcpClientConnectedEventArgs> ClientConnected;
        /// <summary>
        /// 与客户端的连接已断开事件
        /// </summary>
        public event EventHandler<TcpClientDisconnectedEventArgs> ClientDisconnected;

        private void RaiseClientConnected(string remoteEndPoint)
        {
            if (ClientConnected != null)
            {
                if (this.block)
                {
                    ClientConnected(this, new TcpClientConnectedEventArgs(remoteEndPoint));
                }
                else
                {
                    EventHandler eh = new EventHandler(delegate
                    {
                        ClientConnected(this, new TcpClientConnectedEventArgs(remoteEndPoint));
                    });

                    eh.BeginInvoke(null, null, null, null);
                }
            }
        }

        private void RaiseClientDisconnected(string remoteEndPoint)
        {
            if (ClientDisconnected != null)
            {
                if (this.block)
                {
                    ClientDisconnected(this, new TcpClientDisconnectedEventArgs(remoteEndPoint));
                }
                else
                {
                    EventHandler eh = new EventHandler(delegate
                    {
                        ClientDisconnected(this, new TcpClientDisconnectedEventArgs(remoteEndPoint));
                    });

                    eh.BeginInvoke(null, null, null, null);
                }
            }
        }

        #endregion

        #region Send

        /// <summary>
        /// 发送报文至指定的客户端
        /// </summary>
        /// <param name="tcpClient">客户端</param>
        /// <param name="datagram">报文</param>
        public void Send(string remoteEndPoint, byte[] datagram)
        {
            TcpClient tcpClient = this.clients[remoteEndPoint].TcpClient;

            if (!IsRunning)
                throw new InvalidProgramException("This TCP server has not been started.");

            if (tcpClient == null)
                throw new ArgumentNullException("tcpClient");

            if (datagram == null)
                throw new ArgumentNullException("datagram");

            tcpClient.GetStream().BeginWrite(
              datagram, 0, datagram.Length, HandleDatagramWritten, tcpClient);
        }

        private void HandleDatagramWritten(IAsyncResult ar)
        {
            ((TcpClient)ar.AsyncState).GetStream().EndWrite(ar);
        }

        /// <summary>
        /// 发送报文至指定的客户端
        /// </summary>
        /// <param name="tcpClient">客户端</param>
        /// <param name="datagram">报文</param>
        public void Send(string remoteEndPoint, string datagram)
        {
            Send(remoteEndPoint, this.Encoding.GetBytes(datagram));
        }

        /// <summary>
        /// 发送报文至所有客户端
        /// </summary>
        /// <param name="datagram">报文</param>
        public void SendAll(byte[] datagram)
        {
            if (!IsRunning)
                throw new InvalidProgramException("This TCP server has not been started.");

            foreach (var item in this.clients)
            {
                Send(item.Key, datagram);
            }
        }

        /// <summary>
        /// 发送报文至所有客户端
        /// </summary>
        /// <param name="datagram">报文</param>
        public void SendAll(string datagram)
        {
            if (!IsRunning)
                throw new InvalidProgramException("This TCP server has not been started.");

            SendAll(this.Encoding.GetBytes(datagram));
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, 
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release 
        /// both managed and unmanaged resources; <c>false</c> 
        /// to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    try
                    {
                        Stop();

                        if (listener != null)
                        {
                            listener = null;
                        }
                    }
                    catch (SocketException ex)
                    {
                        //ExceptionHandler.Handle(ex);
                    }
                }

                disposed = true;
            }
        }

        #endregion
    }
}
