using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace SV
{
    public partial class YeniArama : Form
    {
        SoundPlayer sp;
        public string ID;
        public YeniArama(string numara, string cagriTipi, string kurbanIsmi, string id)
        {
            InitializeComponent();
            FormClosing += Bildirim_FormClosing;
            ID = id;
            label1.Text = "Adress: " + numara;
            label2.Text = "Type: " + cagriTipi.ToLower().Replace("gelen", "Incoming").Replace("giden", "Outgoing").
            Replace("arama", "call");
            label3.Text = "Victim Name: " + kurbanIsmi;
            Screen ekran = Screen.FromPoint(Location);
            Location = new Point(ekran.WorkingArea.Right - Width, ekran.WorkingArea.Bottom - Height);
            sp = new SoundPlayer("call.wav"); sp.Play();
        }
        private void Bildirim_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sp != null) { sp.Stop(); sp.Dispose(); }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Close();
        }
    }
}
