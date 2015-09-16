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
        bool __exit = false;
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
                this.BeginInvoke(new EventHandler(delegate
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

                            IAsyncResult iar1 = item.GetStream().BeginWrite(Encoding.UTF8.GetBytes(str), 0, str.Length, null, null);
                            IAsyncResult iar2 = item.GetStream().BeginRead(buf, 0, buf.Length, new AsyncCallback(delegate{
                                
                                this.BeginInvoke(new EventHandler(delegate
                                {
                                    this.listBox2.Items.Add("Client : " + Encoding.UTF8.GetString(buf).TrimEnd('\0') + "  PRESURE = " + idx);
                                    this.listBox2.SelectedIndex = this.listBox2.Items.Count - 1;
                                }));                           

                            }), null);

                            if (__exit) return;

                            Thread.Sleep(20);
                        }                        

                        if (__stop)
                        {
                            this.BeginInvoke(new EventHandler(delegate
                            {
                                this.listBox1.Items.Clear();
                                this.listBox2.Items.Clear();
                            }));

                            for (int i = 0; i < tcpClient.Length; i++)
                            {
                                string endpoint = tcpClient[i].Client.LocalEndPoint.ToString();

                                this.BeginInvoke(new EventHandler(delegate
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

            thrd.Start();
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            server = new AsyncTcpServer(IPAddress.Parse("127.0.0.1"), 9999);
            server.Encoding = Encoding.UTF8;
            server.ClientConnected += server_ClientConnected;
            server.ClientDisconnected += server_ClientDisconnected;
            server.DatagramReceived += server_DatagramReceived;
            server.Start();
        }

        void server_DatagramReceived(object sender, TcpDatagramReceivedEventArgs<byte[]> e)
        {
            this.server.Send(e.TcpClient,e.TcpClient.Client.RemoteEndPoint.ToString());

            string endpoint = e.TcpClient.Client.LocalEndPoint.ToString();

            this.BeginInvoke(new EventHandler(delegate
            {
                this.listBox1.Items.Add("Server : rev " + endpoint + "  " + Encoding.UTF8.GetString(e.Datagram));
                this.listBox1.SelectedIndex = this.listBox1.Items.Count - 1;
            }));
            //throw new NotImplementedException();
        }

        void server_ClientDisconnected(object sender, TcpClientDisconnectedEventArgs e)
        {
            string endpoint = e.TcpClient.Client.LocalEndPoint.ToString();

            this.BeginInvoke(new EventHandler(delegate
            {
                this.listBox1.Items.Add("Server : close " + endpoint);
                this.listBox1.SelectedIndex = this.listBox1.Items.Count - 1;
            }));
            //throw new NotImplementedException();
        }

        void server_ClientConnected(object sender, TcpClientConnectedEventArgs e)
        {
            string endpoint = e.TcpClient.Client.LocalEndPoint.ToString();

            this.BeginInvoke(new EventHandler(delegate
            {
                this.listBox1.Items.Add("Server ==> Accept " + endpoint);
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
            string str = this.textBox1.Text;
            Byte[] buf = new Byte[256];

            if (__userTcpClient != null && __userTcpClient.Connected)
            {
                __userTcpClient.GetStream().Write(Encoding.ASCII.GetBytes(str),0,str.Length);
                __userTcpClient.GetStream().Read(buf,0,buf.Length);

                this.listBox3.Items.Add(DateTime.Now.ToLocalTime() + "  " + str);
                this.listBox3.Items.Add(DateTime.Now.ToLocalTime() + "  " + Encoding.ASCII.GetString(buf));
                this.listBox3.SelectedIndex = this.listBox3.Items.Count - 1;
            }
        }

        //     Console.WriteLine("TCP server has been started.");
        //     Console.WriteLine("Type something to send to client...");
        //     while (true)
        //     {
        //       string text = Console.ReadLine();
        //       server.SendAll(text);
        //     }
        //   }
        //   static void server_ClientConnected(object sender, TcpClientConnectedEventArgs e)
        //   {
        //     Logger.Debug(string.Format(CultureInfo.InvariantCulture, 
        //       "TCP client {0} has connected.", 
        //       e.TcpClient.Client.RemoteEndPoint.ToString()));
        //   }
        //   static void server_ClientDisconnected(object sender, TcpClientDisconnectedEventArgs e)
        //   {
        //     Logger.Debug(string.Format(CultureInfo.InvariantCulture, 
        //       "TCP client {0} has disconnected.", 
        //       e.TcpClient.Client.RemoteEndPoint.ToString()));
        //   }
        //   static void server_PlaintextReceived(object sender, TcpDatagramReceivedEventArgs<string> e)
        //   {
        //     if (e.Datagram != "Received")
        //     {
        //       Console.Write(string.Format("Client : {0} --> ", 
        //         e.TcpClient.Client.RemoteEndPoint.ToString()));
        //       Console.WriteLine(string.Format("{0}", e.Datagram));
        //       server.Send(e.TcpClient, "Server has received you text : " + e.Datagram);
        //     }
        //   }
    }
}
