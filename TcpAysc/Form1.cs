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
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TcpServer ts = new TcpServer("127.0.0.1",4000);
            ts.OnClientCall += ts_OnClientCall;
            ts.Start();
        }

        void ts_OnClientCall(TcpClient tc, string str)
        {
            this.Invoke(new EventHandler(delegate {

                this.listBox1.Items.Add(tc.Client.ToString()+":"+str);
            
            }));

            string tst = "ABCDEFGH";

            tc.GetStream().Write(Encoding.ASCII.GetBytes(tst), 0, tst.Length);
            //throw new NotImplementedException();
        }
    }
}
