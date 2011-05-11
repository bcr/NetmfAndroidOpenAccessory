using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.USBHost;

namespace Prototype.AndroidOpenAccessory.Fez
{
    public class Program
    {
        public static void Main()
        {
            USBH_RawDevice openedDevice = null;
            var deviceOpened = new ManualResetEvent(false);

            Debug.Print("Running");
            // Subscribe to USBH events.
            USBHostController.DeviceConnectedEvent += (device) => { openedDevice = new USBH_RawDevice(device); deviceOpened.Set(); };
            Debug.Print("Listening for events");

            deviceOpened.WaitOne();
            deviceOpened.Reset();
            Debug.Print("Got our raw device");

            var dataBytes = new byte[2];
            openedDevice.SendSetupTransfer(0xC0, 51, 0, 1, dataBytes, 0, dataBytes.Length);
            Debug.Print("Received " + dataBytes[0] + ", " + dataBytes[1]);
        }

        // http://www.beyondlogic.org/usbnutshell/usb6.shtml
        // http://forum.pololu.com/viewtopic.php?f=16&t=3154
    }
}
