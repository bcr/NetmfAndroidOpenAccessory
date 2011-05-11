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

        // http://www.beyondlogic.org/usbnutshell/usb6.shtml

        static void DeviceConnectedEvent(USBH_Device device)
        {
            Debug.Print("Device connected...");
            Debug.Print("ID: " + device.ID + ", Interface: " + device.INTERFACE_INDEX + ", Type: " + device.TYPE);
            Debug.Print("Vendor ID: " + device.VENDOR_ID + ", Product ID:" + device.PRODUCT_ID);
            // If it's an Android device, we can party
            // Make sure it's in accessory mode
            // Ask him if he supports accessory mode
            // Open the device
            var openedDevice = new USBH_RawDevice(device);
            var dataBytes = new byte[2];
            Debug.Print("Going to SendSetupTransfer");
            dataBytes[0] = 0xBE;
            dataBytes[1] = 0xEF;
            //openedDevice.SendSetupTransfer(0x40, 51, 0, 0);
            openedDevice.SendSetupTransfer(0x40, 51, 0, 0, dataBytes, 0, dataBytes.Length);
            Debug.Print("Back from SendSetupTransfer");
            //var endpoint0 = openedDevice.GetConfigurationDescriptors(0).interfaces[0].endpoints[0];
            //Debug.Print((endpoint0 != null) ? "Endpoint is NOT null" : "GAA ENDPOINT IS NULL");
            //var endpoint0Pipe = openedDevice.OpenPipe(endpoint0);
            //endpoint0Pipe.TransferData(dataBytes, 0, dataBytes.Length);
            //Debug.Print("Received " + dataBytes[0] + ", " + dataBytes[1]);
        }

        static void DeviceDisconnectedEvent(USBH_Device device)
        {
            Debug.Print("Device disconnected...");
            Debug.Print("ID: " + device.ID + ", Interface: " + device.INTERFACE_INDEX + ", Type: " + device.TYPE);
            Debug.Print("Vendor ID: " + device.VENDOR_ID + ", Product ID:" + device.PRODUCT_ID);
        }
    }
}
