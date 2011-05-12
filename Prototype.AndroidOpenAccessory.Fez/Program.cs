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

        static byte[] StringToUtf8NulTerminatedByteArray(string value)
        {
            var utf8Bytes = System.Text.Encoding.UTF8.GetBytes(value);
            var finalBytes = new byte[utf8Bytes.Length + 1];
            System.Array.Copy(utf8Bytes, finalBytes, utf8Bytes.Length);

            return finalBytes;
        }

        static void SetString(USBH_RawDevice openedDevice, AndroidAccessoryStringTypes stringType, string value)
        {
            var utf8Bytes = StringToUtf8NulTerminatedByteArray(value);
            openedDevice.SendSetupTransfer(
                (byte)(UsbRequestType.Vendor),
                (byte)AndroidAccessoryUsbCommands.SetString,
                0,
                (ushort) stringType,
                utf8Bytes,
                0,
                utf8Bytes.Length
                );
        }

        static void StartAccessoryMode(USBH_RawDevice openedDevice)
        {
            openedDevice.SendSetupTransfer(
                (byte)(UsbRequestType.Vendor),
                (byte)AndroidAccessoryUsbCommands.StartAccessoryMode,
                0,
                0
                );
        }

        public static void Main()
        {
            USBH_RawDevice openedDevice = null;
            var deviceOpened = new ManualResetEvent(false);

            Debug.Print("Running");
            // Subscribe to USBH events.
            USBHostController.DeviceConnectedEvent += (device) => { Debug.Print("Device connected Vendor ID = " + device.VENDOR_ID + ", Product ID = " + device.PRODUCT_ID); openedDevice = new USBH_RawDevice(device); deviceOpened.Set(); };
            Debug.Print("Listening for events");

            deviceOpened.WaitOne();
            deviceOpened.Reset();
            Debug.Print("Got our raw device");

            Debug.Print("Vendor ID = " + openedDevice.VENDOR_ID + ", Product ID = " + openedDevice.PRODUCT_ID);

            Debug.Print("Protocol version = " + GetProtocol(openedDevice));

            // !!! THIS Thread.Sleep IS A HACK
            // It's not clear what the problem is, but if you don't pause here
            // then the PID on restart is the old PID. I don't know if this is
            // something stupid in the USB stack (someone kept old device info
            // around) or if this is something stupid on the Android side or
            // if it's something stupid I'm doing. But this makes it feel
            // better in my environment. I would love if it went away.

            Thread.Sleep(375);
            SetString(openedDevice, AndroidAccessoryStringTypes.Description, "Prototype Engineering NETMF bridge");
            SetString(openedDevice, AndroidAccessoryStringTypes.ManufacturerName, "Prototype Engineering, LLC");
            SetString(openedDevice, AndroidAccessoryStringTypes.ModelName, "Model 1 baby");
            SetString(openedDevice, AndroidAccessoryStringTypes.SerialNumber, "12345");
            SetString(openedDevice, AndroidAccessoryStringTypes.Uri, "http://http://prototype-eng.com/");
            SetString(openedDevice, AndroidAccessoryStringTypes.Version, "1.2.3");
            Debug.Print("Strings set");

            StartAccessoryMode(openedDevice);

            Debug.Print("Waiting for re-enumeration");
            deviceOpened.WaitOne();
            deviceOpened.Reset();
            Debug.Print("Got our new device");

            Debug.Print("Vendor ID = " + openedDevice.VENDOR_ID + ", Product ID = " + openedDevice.PRODUCT_ID);
        }

        // http://www.beyondlogic.org/usbnutshell/usb6.shtml
        // http://forum.pololu.com/viewtopic.php?f=16&t=3154
        // http://developer.android.com/guide/topics/usb/adk.html
    }
}
