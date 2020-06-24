using Xamarin.Forms;
using XamarinSunmiPayLibSample.Droid.PayImplementations;

[assembly: Dependency(typeof(PrintService))]
namespace XamarinSunmiPayLibSample.Droid.PayImplementations
{
    public class PrintService : IPrinterService

    {
        public void Print(byte[] data)
        {
            var btAdapter = BluetoothUtil.GetBTAdapter();
            BluetoothIsEnabled();
            var device = BluetoothUtil.GetDevice(btAdapter);            
            var socket = BluetoothUtil.GetSocket(device);
            BluetoothUtil.SendData(data, socket);
        }
        private bool BluetoothIsEnabled()
        {
            var btAdapter = BluetoothUtil.GetBTAdapter();
            if (btAdapter == null)
                return false;
            if (!btAdapter.IsEnabled)
                btAdapter.Enable();
            while (!btAdapter.IsEnabled)
            {
                System.Threading.Thread.Sleep(30);
            }
            return true;
        }
    }
}