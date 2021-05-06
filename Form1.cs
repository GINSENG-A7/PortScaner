using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LB5
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            progressBar1.Maximum = 0;
            progressBar1.Value = 0;

            string localhost = Dns.GetHostName();
            label2.Text = localhost;
            string str = "";
            IPAddress[] localIPs = Dns.GetHostAddresses(localhost);
            var IPv4s = localIPs.Where(p => p.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            label3.Text = "";
            if (localIPs.Length > 0)
            {
                label5.Text = IPv4s.Count().ToString();
                foreach (IPAddress ip in IPv4s)
                {
                    str += ip.ToString();
                }
                label3.Text = str;
            }

            PingOptions options = new PingOptions(64, true);
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 1000;

            string[] buf = str.Split('.');

            for (int i = 1; i < 255; i++)
            {
                if(i == Convert.ToInt32(buf[3]))
                {
                    continue;
                }
                Ping pingSender = new Ping();
                pingSender.PingCompleted += new PingCompletedEventHandler(PingCompletedCallback);
                pingSender.SendAsync(buf[0] + "." + buf[1] + "." + buf[2] + "." + i.ToString(), timeout, buffer, options);
            }
        }

        private void CheckPort(IPAddress address, int Port)
        {
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.BeginConnect(address, Port, new AsyncCallback(ConnectCallBack), client);
        }

        private void ConnectCallBack(IAsyncResult result)
        {
            progressBar1.Invoke(new Action(() => progressBar1.Value++)) ;
            Socket sClient = (Socket)result.AsyncState;
            try
            {
                sClient.EndConnect(result);

                string address = sClient.RemoteEndPoint.ToString().Split(':')[0];

                treeView1.Invoke(new Action(() => treeView1.Nodes[address].Nodes.Add($"Порт открыт {sClient.RemoteEndPoint.ToString()}")));
            }
            catch
            {

            }
        }

        private void PingCompletedCallback(object sender, PingCompletedEventArgs e)
        {
            PingReply reply = e.Reply;
            DisplayReply(reply);
        }
        public void DisplayReply(PingReply reply)
        {
            if (reply == null)
                return;

            if (reply.Status == IPStatus.Success)
            {
                progressBar1.Maximum += 1000;

                treeView1.Nodes.Add(reply.Address.ToString(), "Адрес: " + reply.Address.ToString());

                for (int port = 1; port <= 1000; port++)
                {
                    CheckPort(reply.Address, port);
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
          
            Text = $"Число активных потоков: {System.Diagnostics.Process.GetCurrentProcess().Threads.Count.ToString()}";
        }
    }
}
