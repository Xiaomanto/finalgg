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
namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        static UdpClient udp;
        static IPEndPoint sep;
        string serverip = "172.18.241.126";
        IPEndPoint rep = new IPEndPoint(IPAddress.Any, 0);
        Form2[] friends = new Form2[10];
        public static string username = "";
        public static void sndmsg(string ip,string msg)
        {
            sep = new IPEndPoint(IPAddress.Parse(ip), 3307);
            byte[] data = Encoding.Unicode.GetBytes(msg);
            udp.Send(data, data.Length, sep);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            username = textBox1.Text;
            string password = textBox2.Text;
            if(username.Replace(" ","") == null || username.Replace(" ", "") == "")
            {
                MessageBox.Show("帳號不可為空"); return;
            }
            if(password.Replace(" ", "") == null || password.Replace(" ", "") == "")
            {
                MessageBox.Show("密碼不可為空"); return;
            }
            if (udp == null)
            {
                udp = new UdpClient(3000);
                timer1.Enabled = true;
                sndmsg(serverip, "login:" + username + ":" + password);
                button1.Visible = false;
                button2.Visible = true;
            }
            else
            {
                MessageBox.Show("You are already login,You must be logout first!!");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(udp.Available > 0)
            {
                byte[] data = udp.Receive(ref rep);
                string rawstring = Encoding.Unicode.GetString(data);
                string[] token = rawstring.Split(':');
                switch (token[0])
                {
                    case "loginMsg":
                        if (username != token[1])
                        {
                            listBox1.Items.Add(token[1] + ":" + token[2]);
                        }
                        break;
                    case "logoutMsg":
                        listBox1.Items.Remove(token[1] + ":" + token[2]);
                        break;
                    case "message":
                        string friendname = token[1];
                        string msg = token[2];
                        bool talking = false;

                        for (int i = 0; i < friends.Length; i++)
                        {
                            if (token[2] == (string)friends[i].Tag)
                            {
                                talking = true;
                                friends[i].textBox2.Text += msg + Environment.NewLine + Environment.NewLine;
                                friends[i].Select();
                            }
                        }
                        if (!talking)
                        {
                            int i = 0;
                            while (friends[i].Tag != null) i++;
                            friends[i] = new Form2();
                            friends[i].Tag = serverip;
                            friends[i].Text = friendname;
                            friends[i].textBox2.Text += msg + Environment.NewLine + Environment.NewLine;
                            friends[i].Show();
                        }
                        break;
                    case "error":
                        if (token[1].Contains("帳號"))
                        {
                            timer1.Enabled = false;
                            udp.Close();
                            udp = null;
                            listBox1.Items.Clear();
                            button2.Visible = false;
                            button1.Visible = true;
                        }
                        MessageBox.Show(token[1]);
                        break;
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (udp != null)
                sndmsg(serverip, "logout:" + username);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (udp != null)
            {
                sndmsg(serverip, "logout:" + username);
                timer1.Enabled = false;
                udp.Close();
                udp = null;
                listBox1.Items.Clear();
                button2.Visible = false;
                button1.Visible = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            for(int i = 0; i < friends.Length; i++)
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
                while (friends[i].Tag != null) i++;
                friends[i] = new Form2();
                friends[i].Tag = serverip;
                friends[i].Text = friendname;
                friends[i].Show();
            }
        }
    }
}
