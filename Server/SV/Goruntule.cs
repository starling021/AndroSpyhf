using System.Windows.Forms;

namespace SV
{
    public partial class Goruntule : Form
    {
        public Goruntule(string baslik, string icerik)
        {
            InitializeComponent();
            Text = "You are reading: " + baslik;
            richTextBox1.Text = icerik;
        }
    }
}
