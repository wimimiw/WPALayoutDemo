using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace TcpAysc
{
    public class TcpServer
    {
        TcpListener __tcpListener;
        List<TcpClient> __tcpClientList;
        Dictionary<TcpClient, Byte[]> __dtRev = new Dictionary<TcpClient, Byte[]>();
        public delegate void OnClientCallEventHandel(TcpClient tc,string str);
        public event OnClientCallEventHandel OnClientCall;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public TcpServer(string ip,int port)
        {
            __tcpClientList = new List<TcpClient>();
            __tcpListener = new TcpListener(IPAddress.Parse(ip),port);

            Thread thrd = new Thread(new ThreadStart(delegate {

                while (true)
                {
                    foreach (TcpClient item in this.__tcpClientList)
                    {
                        if (!item.Connected)
                        {
                            item.Close();
                            this.__tcpClientList.Remove(item);
                        }
                    }

                    Thread.Sleep(50);
                }
            }));

            thrd.Start();
        }

        ~TcpServer()
        {
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
            }
            catch
            {
 
            }
        }

        //异步中断函数只用事件发生时才响应
        void AsyncCallBackAccept(IAsyncResult iar)
        {
            try
            {
                TcpListener tl = iar.AsyncState as TcpListener;
                TcpClient tc = tl.EndAcceptTcpClient(iar);
                this.__tcpClientList.Add(tc);
                Byte[] buf = new Byte[512];
                __dtRev.Add(tc, buf);
                tc.GetStream().BeginRead(buf,0,buf.Length,new AsyncCallback(AsyncCallBackRead),tc);               
                //注册异步中断处理
                tl.BeginAcceptTcpClient(new AsyncCallback(AsyncCallBackAccept), tl);
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
                Byte[] buf = __dtRev[tc];
                string data = "";

                data = String.Concat(data, Encoding.ASCII.GetString(buf, 0, revCnt));                
                ns.BeginRead(buf, 0, buf.Length, new AsyncCallback(AsyncCallBackRead), tc);

                if (this.OnClientCall != null) this.OnClientCall(tc,data);
            }
            catch
            {
 
            }
        }
    }
}
