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

namespace finalgg
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        static UdpClient udp;
        static IPEndPoint sep;
        IPEndPoint rep = new IPEndPoint(IPAddress.Any, 0);

        public static string username = "";

        public static void sndmsg(string ip, string msg)
        {
            sep = new IPEndPoint(IPAddress.Parse(ip), 3000);
            byte[] data = Encoding.Unicode.GetBytes(msg);
            udp.Send(data, data.Length, sep);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            udp = new UdpClient(3000);
            timer1.Enabled = true;
            username = textBox1.Text;
            sndmsg("255.255.255.255", "login:"+username);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(udp.Available > 0)
            {
                byte[] data = udp.Receive(ref rep);
                string rawstring = Encoding.Unicode.GetString(data);
                string remoteip = rep.Address.ToString();
                string[] token = rawstring.Split(':');
                switch (token[0])
                {
                    case "login":
                        if(username != token[1])
                        {
                            listBox1.Items.Add(token[1] + ":" + remoteip);
                            sndmsg(remoteip, "reply:" + username);
                        }
                        break;
                    case "reply":
                        listBox1.Items.Add(token[1] + ":" + remoteip);
                        break;
                    case "logout":
                        listBox1.Items.Remove(token[1] + ":" + remoteip);
                        break;
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (udp != null)
                sndmsg("255.255.255.255", "logout:" + username);
        }
        Form2[] friends = new Form2[10];
        private void Form1_Load(object sender, EventArgs e)
        {
            for(int i=0;i < friends.Length; i++)
            {
                friends[i] = new Form2();
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            bool talking = false;
            string friendname = listBox1.SelectedItem.ToString().Split(':')[0];
            string friendip = listBox1.SelectedItem.ToString().Split(':')[1];
            for(int i = 0; i < friends.Length; i++)
            {
                if(friendip == (string)friends[i].Tag)
                {
                    talking = true;
                    friends[i].Select();
                }
            }
            if (!talking)
            {
                int i = 0;
                while (friends[i].Tag != null)
                    i++;
                friends[i] = new Form2();
                friends[i].Tag = friendip;
                friends[i].Text = friendname;
                friends[i].Show();
            }
        }
    }
}
