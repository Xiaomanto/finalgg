using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Tag = null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1.sndmsg(this.Tag.ToString(), "message:" + this.Text + ":" + textBox1.Text);
            textBox2.Text += textBox1.Text + Environment.NewLine + Environment.NewLine;
            textBox1.Text = "";
        }
    }
}
