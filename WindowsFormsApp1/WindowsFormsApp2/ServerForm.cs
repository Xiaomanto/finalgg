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
using System.Data.SqlClient;

namespace WindowsFormsApp2
{
    public partial class ServerForm : Form
    {
        UdpClient udp = new UdpClient(3307);
        IPEndPoint ep;
        IPEndPoint rep = new IPEndPoint(IPAddress.Any,0);

        public ServerForm()
        {
            InitializeComponent();
        }

        public void sndmsg(string msg,string ip)
        {
            byte[] data = Encoding.Unicode.GetBytes(msg);
            ep = new IPEndPoint(IPAddress.Parse(ip), 3000);
            udp.Send(data, data.Length, ep);
        }

        private void ServerForm_Load(object sender, EventArgs e)
        {
            // TODO: 這行程式碼會將資料載入 'database1DataSet1.tb1' 資料表。您可以視需要進行移動或移除。
            this.tb1TableAdapter.Fill(this.database1DataSet1.tb1);

        }

        SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Database1.mdf;Integrated Security=True");
        
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (udp.Available > 0)
            {
                byte[] buffer = udp.Receive(ref rep);
                string rawdata = Encoding.Unicode.GetString(buffer);
                string ip = rep.Address.ToString();
                string[] token = rawdata.Split(':');
                string username = "";
                switch (token[0])
                {
                    case "login":

                        conn.Open();
                        username = token[1];
                        string pwd = token[2];
                        SqlCommand cmd = new SqlCommand("select password from tb1 where Id=@id",conn);
                        cmd.Parameters.Add(new SqlParameter("@id", username));
                        SqlDataReader dr = cmd.ExecuteReader();
                        string password = "";
                        
                        while(dr.Read())
                            password = dr["password"].ToString().Replace(" ","");
                        dr.Close();
                        if (pwd == password)
                        {
                            cmd = new SqlCommand("update tb1 set IP=@ip where Id=@id",conn);
                            cmd.Parameters.Add(new SqlParameter("@ip",ip));
                            cmd.Parameters.Add(new SqlParameter("@id", username));
                            cmd.ExecuteNonQuery();
                        
                            cmd = new SqlCommand("select Id,IP from tb1 where IP IS NOT NULL and IP != @ip",conn);
                            cmd.Parameters.Add(new SqlParameter("@ip", ip));
                            dr = cmd.ExecuteReader();
                            while (dr.Read())
                            {
                                sndmsg("loginMsg:" + username +":"+ dr["IP"].ToString().Replace(" ", ""), dr["IP"].ToString().Replace(" ",""));
                                sndmsg("loginMsg:" + dr["Id"].ToString()+":"+ip, ip);
                            }
                            dr.Close();
                        }
                        else
                        {
                            sndmsg("error:帳號或密碼錯誤",ip);
                        }

                        break;
                    case "logout":
                        conn.Open();
                        username = token[1];
                        cmd = new SqlCommand("update tb1 set IP = NULL where Id=@id ",conn);
                        cmd.Parameters.Add(new SqlParameter("@id", username));
                        cmd.ExecuteNonQuery();

                        cmd = new SqlCommand("select IP from tb1 where IP IS NOT NULL", conn);
                        SqlDataReader dr2 = cmd.ExecuteReader();
                        while (dr2.Read())
                        {
                            sndmsg("logoutMsg:" + username+":"+ dr2["IP"].ToString().Replace(" ", ""), dr2["IP"].ToString().Replace(" ",""));
                        }
                        break;
                }
                this.tb1TableAdapter.Fill(this.database1DataSet1.tb1);
                conn.Close();
            }
        }

        private void ServerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SqlCommand cmd = new SqlCommand("update tb1 set IP = NULL", conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
        }
    }
}
