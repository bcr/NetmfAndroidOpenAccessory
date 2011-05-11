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
            Debug.Print("Running");
            // Subscribe to USBH events.
            USBHostController.DeviceConnectedEvent += DeviceConnectedEvent;
            USBHostController.DeviceDisconnectedEvent += DeviceDisconnectedEvent;
            Debug.Print("Listening for events");

            // Sleep forever
            Thread.Sleep(Timeout.Infinite);
        }

        static void DeviceConnectedEvent(USBH_Device device)
        {
            Debug.Print("Device connected...");
            Debug.Print("ID: " + device.ID + ", Interface: " + device.INTERFACE_INDEX + ", Type: " + device.TYPE);
            Debug.Print("Vendor ID: " + device.VENDOR_ID + ", Product ID:" + device.PRODUCT_ID);
        }

        static void DeviceDisconnectedEvent(USBH_Device device)
        {
            Debug.Print("Device disconnected...");
            Debug.Print("ID: " + device.ID + ", Interface: " + device.INTERFACE_INDEX + ", Type: " + device.TYPE);
            Debug.Print("Vendor ID: " + device.VENDOR_ID + ", Product ID:" + device.PRODUCT_ID);
        }
    }
}
