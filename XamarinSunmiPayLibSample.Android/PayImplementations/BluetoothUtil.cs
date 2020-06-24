using Android.Bluetooth;
using Java.Util;

namespace XamarinSunmiPayLibSample.Droid.PayImplementations
{
    public class BluetoothUtil
    {
        public static string Innerprinter_Address { get; set; } = "00:11:22:33:44:55";
        public static UUID PRINTER_UUID { get; set; } = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        public static BluetoothAdapter GetBTAdapter()
        {
            return BluetoothAdapter.DefaultAdapter;
        }
        public static BluetoothDevice GetDevice(BluetoothAdapter bluetoothAdapter)
        {
            BluetoothDevice innerprinter_device = null;
            var devices = bluetoothAdapter.BondedDevices;
            foreach (var device in devices)
            {
                if (device.Address.Equals(Innerprinter_Address))
                {
                    innerprinter_device = device;
                    break;
                }
            }
            return innerprinter_device;
        }
        public static BluetoothSocket GetSocket(BluetoothDevice device)
        {
            var socket = device.CreateRfcommSocketToServiceRecord(PRINTER_UUID);
            socket.Connect();
            return socket;
        }
        public static void SendData(byte[] bytes, BluetoothSocket socket)
        {
            var outp = socket.OutputStream;
            outp.Write(bytes, 0, bytes.Length);
            outp.Close();
        }
    }
}