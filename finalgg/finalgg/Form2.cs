using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace finalgg
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Tag = null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1.sndmsg(this.Tag.ToString(), "message:" + Form1.username + ":" + textBox1.Text);
            textBox2.Text += textBox1.Text + Environment.NewLine;
        }
    }
}
