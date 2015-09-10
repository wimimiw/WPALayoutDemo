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

namespace TcpAysc
{
    public partial class Form1 : Form
    {
        TcpServer __TcpServer;

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
            this.Invoke(new EventHandler(delegate
            {
                this.listBox1.Items.Add(tc.Client.RemoteEndPoint + ":" + str);
            }));

            __TcpServer.Send(tc, "Y");

            //Thread.Sleep(2000);
            //throw new NotImplementedException();
        }

        void ts_OnClientAccepct(TcpClient tc)
        {//此函数不建议处理耗时任务，否则会影响再次接收
            this.Invoke(new EventHandler(delegate
            {
                this.listBox1.Items.Add(tc.Client.RemoteEndPoint + ":ACCPECT!");
            }));
            //throw new NotImplementedException();
        }

        void ts_OnClientClose(TcpClient tc)
        {//此函数不建议处理耗时任务，否则会影响再次接收
            this.Invoke(new EventHandler(delegate
            {
                this.listBox1.Items.Add(tc.Client.RemoteEndPoint + ":CLOSE!");
            }));
            //throw new NotImplementedException();
        }

        void ts_OnClientCall(TcpClient tc, string str)
        {//此函数不建议处理耗时任务，否则会影响再次接收
            //this.Invoke(new EventHandler(delegate {
            //    this.listBox1.Items.Add(tc.Client.RemoteEndPoint+":"+str);            
            //}));

            //__TcpServer.Send(tc, "Yours = " + tc.Client.RemoteEndPoint+"\n\r");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.listBox1.SelectedIndex = this.listBox1.Items.Count - 1;
        }
    }
}
