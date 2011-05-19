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
        enum FezUsbCommands : byte
        {
            None = 0,
            LedOn = 1,
            LedOff = 2,
            GetState = 3,
        }

        [Flags]
        enum FezUsbResponse : byte
        {
            None = 0,
            ButtonDown = 0x80,
            LedOn = 0x40,
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

            Debug.Print("Protocol version = " + AndroidAccessoryUtil.GetProtocol(openedDevice));

            // !!! THIS Thread.Sleep IS A HACK
            // It's not clear what the problem is, but if you don't pause here
            // then the PID on restart is the old PID. I don't know if this is
            // something stupid in the USB stack (someone kept old device info
            // around) or if this is something stupid on the Android side or
            // if it's something stupid I'm doing. But this makes it feel
            // better in my environment. I would love if it went away.

            Thread.Sleep(375);
            var strings = new AndroidAccessoryStrings
            {
                Description = "Prototype Engineering NETMF bridge",
                ManufacturerName = "Prototype Engineering, LLC",
                ModelName = "Model 1 baby",
                SerialNumber = "12345",
                Uri = "http://prototype-eng.com/",
                Version = "1.2.3"
            };
            AndroidAccessoryUtil.SetAllStrings(openedDevice, strings);
            Debug.Print("Strings set");

            AndroidAccessoryUtil.StartAccessoryMode(openedDevice);

            Debug.Print("Waiting for re-enumeration");
            deviceOpened.WaitOne();
            deviceOpened.Reset();
            Debug.Print("Got our new device");

            Debug.Print("Vendor ID = " + openedDevice.VENDOR_ID + ", Product ID = " + openedDevice.PRODUCT_ID);

            // Now do the scavenger hunt for the IN / OUT endpoints

            var configurationDescriptor = openedDevice.GetConfigurationDescriptors(0);
            USBH_RawDevice.Pipe outPipe = null;
            USBH_RawDevice.Pipe inPipe = null;

            // Set the configuration http://developer.android.com/guide/topics/usb/adk.html#establish
            openedDevice.SendSetupTransfer(0x00, 0x09, configurationDescriptor.bConfigurationValue, 0x00);

            var _interface = configurationDescriptor.interfaces[0];
            //foreach (var _interface in configurationDescriptor.interfaces)
            {
                Debug.Print("Interface class = " + _interface.bInterfaceClass + ", subclass = " + _interface.bInterfaceSubclass);

                foreach (var endpoint in _interface.endpoints)
                {
                    Debug.Print("Endpoint descriptor type = " + endpoint.bDescriptorType + ", address = " + endpoint.bEndpointAddress + ", attributes = " + endpoint.bmAttributes);
                    if ((endpoint.bEndpointAddress & 0x80) != 0)
                    {
                        Debug.Print("Opening inPipe");
                        inPipe = openedDevice.OpenPipe(endpoint);
                    }
                    else
                    {
                        Debug.Print("Opening outPipe");
                        outPipe = openedDevice.OpenPipe(endpoint);
                    }
                }
            }

            Debug.Print("Pipes opened");

            DoProtocol(inPipe, outPipe);
        }

        private static void DoProtocol(USBH_RawDevice.Pipe inPipe, USBH_RawDevice.Pipe outPipe)
        {
            var inBuffer = new byte[inPipe.PipeEndpoint.wMaxPacketSize];
            var outBuffer = new byte[outPipe.PipeEndpoint.wMaxPacketSize];
            int bytesTransferred;
            OutputPort LED = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.LED, false);
            InputPort button = new InputPort((Cpu.Pin)FEZ_Pin.Digital.LDR, false, Port.ResistorMode.PullUp);

            while (true)
            {
                bytesTransferred = inPipe.TransferData(inBuffer, 0, 1);
                if (bytesTransferred > 0)
                {
                    Debug.Print("Received " + inBuffer[0]);
                    switch ((FezUsbCommands) inBuffer[0])
                    {
                        case FezUsbCommands.LedOff:
                            LED.Write(false);
                            break;
                        case FezUsbCommands.LedOn:
                            LED.Write(true);
                            break;
                        default:
                            break;
                    }
                    outBuffer[0] = (byte)((LED.Read() ? FezUsbResponse.LedOn : 0) | ((!button.Read()) ? FezUsbResponse.ButtonDown : 0));
                    outPipe.TransferData(outBuffer, 0, 1);
                }
            }
        }

        // http://www.beyondlogic.org/usbnutshell/usb6.shtml
        // http://forum.pololu.com/viewtopic.php?f=16&t=3154
        // http://developer.android.com/guide/topics/usb/adk.html
    }
}
