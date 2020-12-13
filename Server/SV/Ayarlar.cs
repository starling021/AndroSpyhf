using System;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SV
{
    public partial class Ayarlar : Form
    {
        Socket sock; public string ID = "";
        public Ayarlar(Socket sck, string aydi)
        {
            InitializeComponent();
            sock = sck; ID = aydi;
        }
        public void bilgileriIsle(string s1)
        {
            string[] ayristir_ = s1.Split('=');
            trackBar1.Maximum = int.Parse(ayristir_[0].Split('/')[1]);
            trackBar1.Value = int.Parse(ayristir_[0].Split('/')[0]);
            groupBox1.Text = "Ringtone " + ayristir_[0];
            //
            if (ayristir_[0].Split('/')[0] == "0") { groupBox3.Enabled = false; }
            else { groupBox3.Enabled = true; }
            //
            trackBar2.Maximum = int.Parse(ayristir_[1].Split('/')[1]);
            trackBar2.Value = int.Parse(ayristir_[1].Split('/')[0]);
            groupBox2.Text = "Media " + ayristir_[1];
            //
            trackBar3.Maximum = int.Parse(ayristir_[2].Split('/')[1]);
            trackBar3.Value = int.Parse(ayristir_[2].Split('/')[0]);
            groupBox3.Text = "Notification " + ayristir_[2];
            //
            trackBar4.Value = int.Parse(ayristir_[3]);
            groupBox4.Text = "Screen Brightness " + ayristir_[3] + "/255";
        }
        private void button1_Click(object sender, EventArgs e)
        {
            yenile();
        }
        public void yenile()
        {
            try
            {
                Form1.KomutGonder("VOLUMELEVELS", "[VERI][0x09]", sock);
            }
            catch (Exception) { }
        }
        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                Form1.KomutGonder("ZILSESI", "[VERI]" + trackBar1.Value.ToString() + "[VERI][0x09]", sock);
                yenile();
            }
            catch (Exception) { }
        }

        private void trackBar2_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                Form1.KomutGonder("MEDYASESI", "[VERI]" + trackBar2.Value.ToString() + "[VERI][0x09]", sock);
                yenile();
            }
            catch (Exception) { }
        }

        private void trackBar3_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                Form1.KomutGonder("BILDIRIMSESI", "[VERI]" + trackBar3.Value.ToString() + "[VERI][0x09]", sock);
                yenile();
            }
            catch (Exception) { }
        }

        private void trackBar4_MouseUp(object sender, MouseEventArgs e)
        {
            //PARLAKLIK
            try
            {
                Form1.KomutGonder("PARLAKLIK", "[VERI]" + trackBar4.Value.ToString() + "[VERI][0x09]", sock);
                yenile();
            }
            catch (Exception) { }
        }
    }
}
