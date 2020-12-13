using System;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SV
{
    public partial class Baglanti : Form
    {
        public string ID = ""; Socket socket;
        public Baglanti(Socket sck, string aydi)
        {
            InitializeComponent();
            ID = aydi;
            socket = sck;
            textBox3.Text = Form1.PASSWORD;
        }
        bool isNotEmpty = false;
        private void button1_Click(object sender, EventArgs e)
        {
            isNotEmpty = textBox1.Text != "" && textBox2.Text != "";
            if (isNotEmpty)
            {
                try
                {
                    if (textBox3.Text.Contains("<"))
                    {
                        MessageBox.Show(this, "You can't use this char <. because this is special char for Program", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        textBox3.Text = textBox3.Text.Replace("<", "");
                        return;
                    }
                    if (textBox3.Text.Contains(">"))
                    {
                        MessageBox.Show(this, "You can't use this char >. because this is special char for Program", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        textBox3.Text = textBox3.Text.Replace(">", "");
                        return;
                    }
                    MessageBox.Show(this, "If you are going to renew your old connection password, you must restart the Server panel, otherwise the connection between you and your client will not be possible.",
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Form1.KomutGonder("UPDATE", "[VERI]" + textBox1.Text + "[VERI]" + textBox2.Text + "[VERI]"
                        + numericUpDown1.Value.ToString() + $"[VERI]{textBox3.Text}[0x09]", socket);
                    Close();
                }
                catch (Exception) { }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox3.PasswordChar = (checkBox1.Checked) ? '\0' : '*';
        }
    }
}
