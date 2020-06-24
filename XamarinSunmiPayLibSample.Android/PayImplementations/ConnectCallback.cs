using Com.Sunmi.Pay.Hardware.Aidl;

namespace XamarinSunmiPayLibSample.Droid.PayImplementations
{
    public class ConnectCallback : Java.Lang.Object, Sunmi.Paylib.SunmiPayKernel.IConnectCallback
    {
        public void OnConnectPaySDK()
        {
            Sunmi.Paylib.SunmiPayKernel.Instance.MBasicOptV2.SetSysParam(AidlConstants.SysParam.Reserved, "{\"buzzer\":0}"); //Close buzzer
        }
        public void OnDisconnectPaySDK()
        {

        }
    }
}
