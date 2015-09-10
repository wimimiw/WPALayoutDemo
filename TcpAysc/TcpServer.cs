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
        object __objLock = new object();
        TcpListener __tcpListener;
        List<TcpClient> __tcpClientList;
        Dictionary<TcpClient, List<Byte>> __dtTst = new Dictionary<TcpClient, List<Byte>>();
        Dictionary<TcpClient, Byte[]> __dtRev = new Dictionary<TcpClient, Byte[]>();
        public delegate void OnClientCallEventHandel(TcpClient tc,string str);
        public delegate void OnClientCloseEventHandel(TcpClient tc);
        public event OnClientCallEventHandel OnClientCall;
        public event OnClientCloseEventHandel OnClientClose;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public TcpServer(string ip,int port)
        {
            __tcpClientList = new List<TcpClient>();
            __tcpListener = new TcpListener(IPAddress.Parse(ip),port);
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

        public void Send(TcpClient tc,string str)
        {
            lock(__objLock)
            {
                this.__dtTst[tc].AddRange(Encoding.ASCII.GetBytes(str));
            }

            tc.GetStream().BeginWrite(
                this.__dtTst[tc].ToArray(), 0,
                this.__dtTst[tc].Count,
                new AsyncCallback(AsyncCallBackWrite), tc);
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
                Byte[] tbuf = new Byte[512];
                this.__dtRev.Add(tc, buf);
                this.__dtTst.Add(tc, new List<Byte>());
                tc.GetStream().BeginRead(buf,0,buf.Length,new AsyncCallback(AsyncCallBackRead),tc);
                //tc.GetStream().BeginWrite(tbuf, 0, 0, new AsyncCallback(AsyncCallBackWrite), tc);
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
                this.__dtTst[tc].Clear();
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
                TcpClient tc = iar.AsyncState as TcpClient;
                this.__tcpClientList.Remove(tc);
                if (this.OnClientClose != null) this.OnClientClose(tc);

                if(tc!=null)
                    tc.Close();
            }
        }
    }
}
