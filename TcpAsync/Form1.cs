using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace TcpAsync
{
    public partial class Form1 : Form
    {
        bool __stop;
        TcpServer __TcpServer;
        object __lockObj = new object();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            __TcpServer = new TcpServer("127.0.0.1",4000);
            __TcpServer.OnClientRead += ts_OnClientCall;
            __TcpServer.OnClientClose += ts_OnClientClose;
            __TcpServer.OnClientAccepct += ts_OnClientAccepct;
            __TcpServer.OnClientThreadRead += __TcpServer_OnClientThreadRead;
            __TcpServer.Start();
        }

        void __TcpServer_OnClientThreadRead(TcpClient tc, string str)
        {//此处不会影响接收
            //this.Invoke(new EventHandler(delegate
            //{
            //    this.listBox1.Items.Add(tc.Client.RemoteEndPoint + ":" + str);
            //}));

            //__TcpServer.Send(tc, "Y");

            //Thread.Sleep(2000);
            //throw new NotImplementedException();
        }

        void ts_OnClientAccepct(TcpClient tc)
        {//此函数不建议处理耗时任务，否则会影响再次接收
            this.Invoke(new EventHandler(delegate
            {
                this.listBox1.Items.Add("SERVER : "+tc.Client.RemoteEndPoint + ":ACCPECT!");
            }));
            //throw new NotImplementedException();
        }

        void ts_OnClientClose(TcpClient tc)
        {//此函数不建议处理耗时任务，否则会影响再次接收
            this.Invoke(new EventHandler(delegate
            {
                if(tc.Client.Connected)
                    this.listBox1.Items.Add(tc.Client.RemoteEndPoint + ":CLOSE!");
                else
                    this.listBox1.Items.Add(":CLOSE!");
            }));
            //throw new NotImplementedException();
        }

        void ts_OnClientCall(TcpClient tc, string str)
        {//此函数不建议处理耗时任务，否则会影响再次接收
            this.Invoke(new EventHandler(delegate
            {
                if (tc.Client.Connected)
                    this.listBox1.Items.Add("SERVER : " + tc.Client.RemoteEndPoint + ":" + str);
                else
                    this.listBox1.Items.Add("SERVER :" + str);
            }));

            __TcpServer.Send(tc, "Yours\n\r");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.listBox1.SelectedIndex = this.listBox1.Items.Count - 1;
            this.listBox2.SelectedIndex = this.listBox2.Items.Count - 1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TcpClient[] tcpClient = new TcpClient[1000];

            for (int i = 0; i < tcpClient.Length; i++)
            {
                tcpClient[i] = new TcpClient();
                IAsyncResult iar = tcpClient[i].BeginConnect("127.0.0.1", 4000, null, null);               
            }

            string str;
            Byte[] buf = new Byte[512];

            __stop = false;

            Thread thrd = new Thread(new ThreadStart(delegate {

                while (true)
                {
                    foreach (TcpClient item in tcpClient)
                    {
                        if (item.Connected)
                        {
                            str = item.Client.LocalEndPoint.ToString();
                            item.GetStream().BeginWrite(Encoding.ASCII.GetBytes(str), 0, str.Length, null, null);
                            IAsyncResult iar = item.GetStream().BeginRead(buf, 0, buf.Length, null, null);
                            item.GetStream().EndRead(iar);
                            //item.GetStream().Write(Encoding.ASCII.GetBytes(str),0,str.Length);
                            //item.GetStream().Read(buf,0,buf.Length);
                            this.Invoke(new EventHandler(delegate
                            {
                                this.listBox2.Items.Add("Client : " + item.Client.LocalEndPoint + "  " + Encoding.ASCII.GetString(buf));
                            }));
                        }
                    }

                    if (__stop)
                    {
                        for (int i = 0; i < tcpClient.Length; i++)
                        {
                            tcpClient[i].Close();
                            tcpClient[i] = null;
                        }

                        //GC.WaitForFullGCComplete(2000);

                        break;
                    }

                    Thread.Sleep(100);
                }

            }));

            thrd.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            lock (this.__lockObj)
            {
                this.__stop = true;
            }
        }
    }
}
