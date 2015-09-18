using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AsyncTcpServer
{
    public partial class Form1 : Form
    {
        AsyncTcpServer server;
        bool __stop;
        object __lockObj = new object();
        int __clientSum = 1000;

        TcpClient __userTcpClient;

        void TcpClientGeneral(int num)
        {
            TcpClient[] tcpClient = new TcpClient[num];

            for (int i = 0; i < tcpClient.Length; i++)
            {
                tcpClient[i] = new TcpClient();
                IAsyncResult iar = tcpClient[i].BeginConnect("127.0.0.1", 9999, null, null);
                this.BeginInvoke(new MethodInvoker(delegate
                {
                    this.listBox2.Items.Add("Client ==> ConnectTo 127.0.0.1:9999");
                    this.listBox2.SelectedIndex = this.listBox2.Items.Count - 1;
                }));
            }

            string str;
            Byte[] buf = new Byte[512];

            __stop = false;

            Thread thrd = new Thread(new ThreadStart(delegate
            {
                int idx;

                while (true)
                {
                    idx = 0;

                    foreach (TcpClient item in tcpClient)
                    {
                        if (item.Connected)
                        {
                            idx++;
                            str = idx.ToString(); ;

                            IAsyncResult iar1 = item.GetStream().BeginWrite(Encoding.UTF8.GetBytes(str), 0, str.Length, new AsyncCallback(delegate{
                                 
                            }), null);

                            IAsyncResult iar2 = item.GetStream().BeginRead(buf, 0, buf.Length, new AsyncCallback(delegate{
                                
                                this.BeginInvoke(new MethodInvoker(delegate
                                {
                                    this.listBox2.Items.Add("Client Thread="+Thread.CurrentThread.ManagedThreadId+" : Send " + Encoding.UTF8.GetString(buf).TrimEnd('\0') + " Pressure = " + idx);
                                    this.listBox2.SelectedIndex = this.listBox2.Items.Count - 1;
                                }));                           

                            }), null);                           

                            Thread.Sleep(20);
                        }                        

                        if (__stop)
                        {
                            this.BeginInvoke(new MethodInvoker(delegate
                            {
                                this.listBox1.Items.Clear();
                                this.listBox2.Items.Clear();
                            }));

                            for (int i = 0; i < tcpClient.Length; i++)
                            {
                                string endpoint = tcpClient[i].Client.LocalEndPoint.ToString();

                                this.BeginInvoke(new MethodInvoker(delegate
                                {
                                    this.listBox2.Items.Add("Close : " + endpoint);
                                    this.listBox2.SelectedIndex = this.listBox2.Items.Count - 1;
                                }));

                                tcpClient[i].Close();                                
                                tcpClient[i] = null;
                            }

                            //GC.WaitForFullGCComplete(2000);

                            return;
                        }
                    }

                    Thread.Sleep(50);
                }

            }));

            thrd.IsBackground = true; //由窗体负责销毁线程
            thrd.Start();
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ABC");

            server = new AsyncTcpServer(IPAddress.Parse("127.0.0.1"), 9999);
            server.Encoding = Encoding.UTF8;
            server.ClientConnected += server_ClientConnected;
            server.ClientDisconnected += server_ClientDisconnected;
            server.DatagramReceived += server_DatagramReceived;
            server.Start();
        }

        void server_DatagramReceived(object sender, TcpDatagramReceivedEventArgs<byte[]> e)
        {
            this.server.Send(e.remoteEndPoint,e.Datagram);

            //异步处理，但别用e中的元素，因为可能其会失效
            //MethodInvoker eh = new MethodInvoker(delegate {
            //    Thread.Sleep(3000);
            //    MessageBox.Show("MethodInvoker");
            //});

            //eh.BeginInvoke(null, null, null, null);

            this.BeginInvoke(new MethodInvoker(delegate
            {
                this.listBox1.Items.Add("Server Thread=" + Thread.CurrentThread.ManagedThreadId + " : Rev " + e.remoteEndPoint + "  " + Encoding.UTF8.GetString(e.Datagram));
                this.listBox1.SelectedIndex = this.listBox1.Items.Count - 1;
            }));
            //throw new NotImplementedException();
        }

        void server_ClientDisconnected(object sender, TcpClientDisconnectedEventArgs e)
        {            
            this.BeginInvoke(new MethodInvoker(delegate
            {
                this.listBox1.Items.Add("Server : Close " + e.remoteEndPoint);
                this.listBox1.SelectedIndex = this.listBox1.Items.Count - 1;
            }));
            //throw new NotImplementedException();
        }

        void server_ClientConnected(object sender, TcpClientConnectedEventArgs e)
        {         
            this.BeginInvoke(new MethodInvoker(delegate
            {
                this.listBox1.Items.Add("Server Thread=" + Thread.CurrentThread.ManagedThreadId + " ==> Accept " + e.remoteEndPoint);
                this.listBox1.SelectedIndex = this.listBox1.Items.Count - 1;
            }));
            //throw new NotImplementedException();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            __clientSum = (int)this.numericUpDown1.Value;
            TcpClientGeneral(__clientSum);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            lock (this.__lockObj)
            {
                this.__stop = true;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (server != null)
            {
                this.Text = "Client = " + server.ClientCount + " / " + __clientSum;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {           
            server.Dispose();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckBox).Checked)
            {
                __userTcpClient = new TcpClient("127.0.0.1", 9999);
            }
            else
            {
                if (__userTcpClient != null)
                {
                    __userTcpClient.Client.Close();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string str = this.textBox1.Text + " " + DateTime.Now.ToLocalTime();
            Byte[] buf = new Byte[256];

            //MethodInvoker method = new MethodInvoker(delegate
            //{
            //    Thread.Sleep(4000);
            //    MessageBox.Show("MethodInvoker");
            //});

            //method.BeginInvoke(null, null);

            if (__userTcpClient != null && __userTcpClient.Connected)
            {
                __userTcpClient.GetStream().Write(Encoding.ASCII.GetBytes(str),0,str.Length);
                __userTcpClient.GetStream().Read(buf,0,buf.Length);

                this.listBox3.Items.Add(DateTime.Now.ToLocalTime() + "  " + str);
                this.listBox3.Items.Add(DateTime.Now.ToLocalTime() + "  " + Encoding.ASCII.GetString(buf));
                this.listBox3.SelectedIndex = this.listBox3.Items.Count - 1;
            }            
        }
    }
}
