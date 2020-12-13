using System;
using System.IO;
using System.Windows.Forms;

namespace SV
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            checkBox1.Checked = lines[lines.Length - 1] != "..." ? true : false;
        }
        string[] lines = File.ReadAllLines("settings.tht");
        private void button1_Click(object sender, EventArgs e)
        {
            lines[lines.Length - 1] = checkBox1.Checked == true ? Environment.CurrentDirectory + "\\sound.wav" : "...";
            File.WriteAllLines("settings.tht", lines);
            Close();
        }
    }
}
