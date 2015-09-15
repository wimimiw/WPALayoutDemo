using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace TcpAsync
{
    public class TcpServer
    {
        public class CtrlMemb
        {
            public List<Byte> Tst = new List<byte>();
            public Byte[] Rev = new Byte[512];
            public StringBuilder Revs = new StringBuilder(256);
            public ManualResetEvent Mre = new ManualResetEvent(false);
        }

        object __objLock = new object();
        TcpListener __tcpListener;
        List<TcpClient> __clientList;

        //Dictionary<int, List<Byte>> __dtTst = new Dictionary<int, List<Byte>>();
        Dictionary<TcpClient, List<Byte>> __dtTst = new Dictionary<TcpClient, List<Byte>>();
        Dictionary<TcpClient, Byte[]> __dtRev = new Dictionary<TcpClient, Byte[]>();
        Dictionary<TcpClient, StringBuilder> __dtRevs = new Dictionary<TcpClient, StringBuilder>();
        Dictionary<TcpClient, ManualResetEvent> __dtMRE = new Dictionary<TcpClient, ManualResetEvent>();

        Dictionary<TcpClient, CtrlMemb> __dtCtrl = new Dictionary<TcpClient, CtrlMemb>();

        public delegate void OnClientCallEventHandel(TcpClient tc,string str);
        public delegate void OnClientCloseEventHandel(TcpClient tc);
        public delegate void OnClientAccpectEventHandel(TcpClient tc);
        public event OnClientCallEventHandel OnClientRead;
        public event OnClientCloseEventHandel OnClientClose;
        public event OnClientAccpectEventHandel OnClientAccepct;

        public event OnClientCallEventHandel OnClientThreadRead;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public TcpServer(string ip,int port)
        {
            //__clientList = new List<TcpClient>();
            __tcpListener = new TcpListener(IPAddress.Parse(ip),port);
        }

        ~TcpServer()
        {
            if(__tcpListener!=null)
                __tcpListener.Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            try
            {
                __tcpListener.Start();
                //注册异步中断处理
                __tcpListener.BeginAcceptTcpClient(new AsyncCallback(AsyncCallBackAccept), __tcpListener);

                Thread thrd = new Thread(new ThreadStart(delegate
                {
                    TcpClient tc;

                    while (true)
                    {
                        if (this.__clientList.Count > 0)
                        {
                            for (int i = 0; i < this.__clientList.Count; i++)
                            {
                                tc = this.__clientList[i];
                                
                                if( tc == null || !this.__dtMRE.ContainsKey(tc))continue;
                                
                                if (this.__dtMRE[tc].WaitOne(0))
                                {//线程安全 必须 0ms wait  
                                    this.__dtMRE[tc].Reset();

                                    if (this.OnClientThreadRead != null)
                                        this.OnClientThreadRead(tc, this.__dtRevs[tc].ToString());
                                }
                            }

                            Thread.Sleep(20);
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }

                }));

                //thrd.Start();
            }
            catch
            {
 
            }
        }

        public void Send(TcpClient tc,string str)
        {
            //this.__dtTst[tc].AddRange(Encoding.ASCII.GetBytes(str));

            //tc.GetStream().BeginWrite(
            //    this.__dtTst[tc].ToArray(), 0,
            //    this.__dtTst[tc].Count,
            //    new AsyncCallback(AsyncCallBackWrite), tc);
            tc.GetStream().BeginWrite(Encoding.ASCII.GetBytes(str),0,str.Length,new AsyncCallback(AsyncCallBackWrite),tc);
        }

        //异步中断函数只用事件发生时才响应
        void AsyncCallBackAccept(IAsyncResult iar)
        {
            try
            {
                TcpListener tl = iar.AsyncState as TcpListener;
                TcpClient tc = tl.EndAcceptTcpClient(iar);
                
                Byte[] buf = new Byte[512];
                CtrlMemb cm = new CtrlMemb();
                cm.Rev = buf;
                this.__dtCtrl.Add(tc,cm);

                tc.GetStream().BeginRead(buf,0,buf.Length,new AsyncCallback(AsyncCallBackRead),tc);
                //tc.GetStream().BeginWrite(tbuf, 0, 0, new AsyncCallback(AsyncCallBackWrite), tc);

                if (OnClientAccepct != null)
                    OnClientAccepct(tc);

                //注册异步中断处理
                tl.BeginAcceptTcpClient(new AsyncCallback(AsyncCallBackAccept), tl);
            }
            catch
            {
                
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="iar"></param>
        void AsyncCallBackWrite(IAsyncResult iar)
        {
            try
            {
                TcpClient tc = iar.AsyncState as TcpClient;
                NetworkStream ns = tc.GetStream();
                ns.EndWrite(iar);
                //this.__dtTst[tc].Clear();
            }
            catch
            {

            }
        }

        //异步中断函数只用事件发生时才响应
        void AsyncCallBackRead(IAsyncResult iar)
        {
            try
            {
                TcpClient tc = iar.AsyncState as TcpClient;
                NetworkStream ns = tc.GetStream();
                int revCnt = ns.EndRead(iar);
                Byte[] buf = this.__dtCtrl[tc].Rev;
                string data = "";
                
                data = String.Concat(data, Encoding.ASCII.GetString(buf, 0, revCnt));

                this.__dtCtrl[tc].Revs = new StringBuilder(data);

                ns.BeginRead(buf, 0, buf.Length, new AsyncCallback(AsyncCallBackRead), tc);

                if (this.OnClientRead != null) 
                    this.OnClientRead(tc,data);

                this.__dtCtrl[tc].Mre.Set();
            }
            catch
            {
                TcpClient tc = iar.AsyncState as TcpClient;

                this.__dtCtrl.Remove(tc);

                if (this.OnClientClose != null) 
                    this.OnClientClose(tc);

                if (tc != null)
                    tc.Close();                
            }
        }
    }
}
