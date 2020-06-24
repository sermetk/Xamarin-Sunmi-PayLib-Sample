using System;
using Sunmi.Paylib;
using Xamarin.Forms;
using XamarinSunmiPayLibSample.Droid.PayImplementations;

[assembly: Dependency(typeof(PayService))]
namespace XamarinSunmiPayLibSample.Droid.PayImplementations
{
    public class PayService : IPayService
    {
        public void Read(ReadType readType)
        {
            CancelRead();
            SunmiPayKernel.Instance.MEMVOptV2.InitEmvProcess();
            SunmiPayKernel.Instance.MReadCardOptV2.CheckCard((int)readType, new CheckCardCallback(), (int)TimeSpan.FromMinutes(15).TotalSeconds);
        }
        public void CancelRead()
        {
            SunmiPayKernel.Instance.MReadCardOptV2.CancelCheckCard();
        }
    }
}
