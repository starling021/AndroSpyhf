using System;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SV
{
    public partial class DownloadManager : Form
    {
        Socket sck;
        public string ID;
        public DownloadManager(Socket socket, string ident)
        {
            InitializeComponent();
            sck = socket;
            ID = ident;
        }
        //
        private void button1_Click(object sender, EventArgs e)
        {
           Form1.KomutGonder("DOWNFILE", "[VERI]" + textBox1.Text + "[VERI]" + textBox2.Text + "[VERI][0x09]", sck);
        }
    }
}
