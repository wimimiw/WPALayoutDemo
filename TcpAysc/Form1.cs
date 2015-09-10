using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;

namespace TcpAysc
{
    public partial class Form1 : Form
    {
        TcpServer ts;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ts = new TcpServer("127.0.0.1",4000);
            ts.OnClientCall += ts_OnClientCall;
            ts.OnClientClose += ts_OnClientClose;
            ts.Start();
        }

        void ts_OnClientClose(TcpClient tc)
        {
            this.Invoke(new EventHandler(delegate
            {
                this.listBox1.Items.Add(tc.Client.LocalEndPoint + "——" + tc.Client.RemoteEndPoint + ":CLOSE!");

            }));
            //throw new NotImplementedException();
        }

        void ts_OnClientCall(TcpClient tc, string str)
        {
            this.Invoke(new EventHandler(delegate {
                this.listBox1.Items.Add(tc.Client.LocalEndPoint+"——"+tc.Client.RemoteEndPoint+":"+str);            
            }));

            ts.Send(tc, "Yours = " + tc.Client.RemoteEndPoint+"\n\r");
        }
    }
}
