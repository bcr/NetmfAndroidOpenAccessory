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
        enum AndroidAccessoryUsbCommands : byte
        {
            None = 0,
            GetProtocol = 51,
            SetString = 52,
            StartAccessoryMode = 53,
        }

        enum AndroidAccessoryStringTypes : byte
        {
            None = 0,
            ManufacturerName = 1,
            ModelName = 2,
            Description = 3,
            Version = 4,
            Uri = 5,
            SerialNumber = 6,
        }

        [Flags]
        enum UsbRequestType : byte
        {
            None = 0x00,
            DeviceToHost = 0x80,
            Vendor = 0x40,
        }

        public static int GetProtocol(USBH_RawDevice openedDevice)
        {
            var dataBytes = new byte[2];
            openedDevice.SendSetupTransfer(
                (byte) (UsbRequestType.DeviceToHost | UsbRequestType.Vendor),
                (byte) AndroidAccessoryUsbCommands.GetProtocol,
                0,
                1,
                dataBytes,
                0,
                dataBytes.Length
                );

            return (dataBytes[1] << 8) | dataBytes[0];
        }

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

            Debug.Print("Vendor ID = " + openedDevice.VENDOR_ID + ", Product ID = " + openedDevice.PRODUCT_ID);

            Debug.Print("Protocol version = " + GetProtocol(openedDevice));
        }

        // http://www.beyondlogic.org/usbnutshell/usb6.shtml
        // http://forum.pololu.com/viewtopic.php?f=16&t=3154
        // http://developer.android.com/guide/topics/usb/adk.html
    }
}
