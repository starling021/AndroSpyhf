using System;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SV
{
    public partial class Ekle : Form
    {
        Socket sckt;
        public Ekle(Socket sck)
        {
            InitializeComponent();
            sckt = sck;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("REHBERISIM", "[VERI]" + textBox1.Text + "=" + textBox2.Text + "=[VERI][0x09]", sckt);
            }
            catch (Exception) { }
            Close();
        }
    }
}
