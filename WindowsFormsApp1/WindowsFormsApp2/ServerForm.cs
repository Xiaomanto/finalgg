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
using System.Security.Cryptography;

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
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] cipher = rsa.Encrypt(Encoding.Unicode.GetBytes(msg), false);
            string cipherstr = Convert.ToBase64String(cipher);
            ep = new IPEndPoint(IPAddress.Parse(ip), 3000);
            byte[] data = Encoding.Unicode.GetBytes(cipherstr + ":" + rsa.ToXmlString(true));
            udp.Send(data, data.Length, ep);
        }

        private void ServerForm_Load(object sender, EventArgs e)
        {
            // TODO: 這行程式碼會將資料載入 'database1DataSet.tb1' 資料表。您可以視需要進行移動或移除。
            this.tb1TableAdapter.Fill(this.database1DataSet.tb1);

        }

        SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Database1.mdf;Integrated Security=True");
        HMACMD5 md5 = new HMACMD5();

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (udp.Available > 0)
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                byte[] buffer = udp.Receive(ref rep);
                string rawdata = Encoding.Unicode.GetString(buffer);
                string[] cipherstr = rawdata.Split(':');
                rsa.FromXmlString(cipherstr[1]);
                byte[] plain = rsa.Decrypt(Convert.FromBase64String(cipherstr[0]),false);
                rawdata = Encoding.Unicode.GetString(plain);
                string ip = rep.Address.ToString();
                string[] token = rawdata.Split(':');
                string username = "";
                switch (token[0])
                {
                    case "login":
                        if (conn.State == ConnectionState.Closed)
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
                        conn.Close();
                        md5.Key = Encoding.Unicode.GetBytes(username.Substring(0,8));
                        string hashpwd = BitConverter.ToString(md5.ComputeHash(Encoding.Unicode.GetBytes(pwd))).Replace("-","").ToUpper();
                        if (hashpwd == password)
                        {
                            cmd = new SqlCommand("update tb1 set IP=@ip where Id=@id",conn);
                            cmd.Parameters.Add(new SqlParameter("@ip",ip));
                            cmd.Parameters.Add(new SqlParameter("@id", username));
                            conn.Open();
                            cmd.ExecuteNonQuery();
                            conn.Close();
                            cmd = new SqlCommand("select Id,IP from tb1 where IP IS NOT NULL and IP != @ip",conn);
                            cmd.Parameters.Add(new SqlParameter("@ip", ip));
                            conn.Open();
                            dr = cmd.ExecuteReader();
                            while (dr.Read())
                            {
                                sndmsg("loginMsg:" + username +":"+ dr["IP"].ToString().Replace(" ", ""), dr["IP"].ToString().Replace(" ",""));
                                sndmsg("loginMsg:" + dr["Id"].ToString()+":"+ip, ip);
                            }
                            dr.Close();
                            conn.Close();
                            sndmsg("success:登入成功",ip);
                        }
                        else
                        {
                            if(password == null || password == "" || password == "NULL")
                                sndmsg("error:帳號不存在，請先註冊", ip);
                            else
                                sndmsg("error:帳號或密碼錯誤",ip);
                        }

                        break;
                    case "register":
                        SqlCommand cmd1 = new SqlCommand("select * from tb1 where Id = @id",conn);
                        cmd1.Parameters.Add(new SqlParameter("@id", token[1]));
                        if (conn.State == ConnectionState.Closed)
                            conn.Open();
                        SqlDataReader dr1 = cmd1.ExecuteReader();
                        while (dr1.Read())
                        {
                            sndmsg("error:帳號已存在請重新註冊",ip);
                            dr1.Close();
                            conn.Close();
                            return;
                            break;
                        }
                        dr1.Close();
                        conn.Close();
                        cmd1 = new SqlCommand("insert into tb1 values(@id,@password,NULL)",conn);
                        cmd1.Parameters.Add(new SqlParameter("@id", token[1]));
                        cmd1.Parameters.Add(new SqlParameter("@password", token[2]));
                        conn.Open();
                        cmd1.ExecuteNonQuery();
                        conn.Close();
                        sndmsg("error:註冊成功", ip);
                        break;
                    case "logout":
                        if (conn.State == ConnectionState.Closed)
                            conn.Open();
                        username = token[1];
                        SqlCommand cmd2 = new SqlCommand("update tb1 set IP = NULL where Id=@id ",conn);
                        cmd2.Parameters.Add(new SqlParameter("@id", username));
                        cmd2.ExecuteNonQuery();
                        conn.Close();
                        cmd = new SqlCommand("select IP from tb1 where IP IS NOT NULL", conn);
                        conn.Open();
                        SqlDataReader dr2 = cmd.ExecuteReader();
                        while (dr2.Read())
                        {
                            sndmsg("logoutMsg:" + username+":"+ dr2["IP"].ToString().Replace(" ", ""), dr2["IP"].ToString().Replace(" ",""));
                        }
                        dr2.Close();
                        conn.Close();
                        break;
                    case "message":
                        if (conn.State == ConnectionState.Closed);
                            conn.Open();
                        SqlCommand cmd3 = new SqlCommand("select IP from tb1 where Id=@id",conn);
                        cmd3.Parameters.Add(new SqlParameter("@id", token[1]));
                        SqlDataReader dr3 = cmd3.ExecuteReader();
                        string targetip = "";
                        while (dr3.Read())
                        {
                            targetip = dr3["IP"].ToString().Replace(" ","");
                        }
                        dr3.Close();
                        conn.Close();
                        sndmsg("message:" + token[1] + ":" + token[2],targetip);
                        break;
                }
                this.tb1TableAdapter.Fill(this.database1DataSet.tb1);
            }
        }

        private void ServerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SqlCommand cmd = new SqlCommand("update tb1 set IP = NULL", conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
