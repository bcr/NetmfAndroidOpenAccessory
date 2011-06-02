using System;
using Microsoft.SPOT;
using GHIElectronics.NETMF.USBHost;

namespace Prototype.AndroidOpenAccessory.Fez
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
        ManufacturerName = 0,
        ModelName = 1,
        Description = 2,
        Version = 3,
        Uri = 4,
        SerialNumber = 5,
    }

    public class AndroidAccessoryUtil
    {
        static byte[] StringToUtf8NulTerminatedByteArray(string value)
        {
            var utf8Bytes = System.Text.Encoding.UTF8.GetBytes(value);
            var finalBytes = new byte[utf8Bytes.Length + 1];
            System.Array.Copy(utf8Bytes, finalBytes, utf8Bytes.Length);

            return finalBytes;
        }

        static void SetStringIfNotNull(USBH_RawDevice openedDevice, AndroidAccessoryStringTypes stringType, string value)
        {
            if (value != null)
            {
                var utf8Bytes = StringToUtf8NulTerminatedByteArray(value);
                openedDevice.SendSetupTransfer(
                    (byte)(UsbRequestType.Vendor),
                    (byte)AndroidAccessoryUsbCommands.SetString,
                    0,
                    (ushort)stringType,
                    utf8Bytes,
                    0,
                    utf8Bytes.Length
                    );
            }
        }

        public static void SetAllStrings(USBH_RawDevice openedDevice, AndroidAccessoryStrings strings)
        {
            SetStringIfNotNull(openedDevice, AndroidAccessoryStringTypes.Description, strings.Description);
            SetStringIfNotNull(openedDevice, AndroidAccessoryStringTypes.ManufacturerName, strings.ManufacturerName);
            SetStringIfNotNull(openedDevice, AndroidAccessoryStringTypes.ModelName, strings.ModelName);
            SetStringIfNotNull(openedDevice, AndroidAccessoryStringTypes.SerialNumber, strings.SerialNumber);
            SetStringIfNotNull(openedDevice, AndroidAccessoryStringTypes.Uri, strings.Uri);
            SetStringIfNotNull(openedDevice, AndroidAccessoryStringTypes.Version, strings.Version);
        }

        public static void StartAccessoryMode(USBH_RawDevice openedDevice)
        {
            openedDevice.SendSetupTransfer(
                (byte)(UsbRequestType.Vendor),
                (byte)AndroidAccessoryUsbCommands.StartAccessoryMode,
                0,
                0
                );
        }

        public static int GetProtocol(USBH_RawDevice openedDevice)
        {
            var dataBytes = new byte[2];
            try
            {
                openedDevice.SendSetupTransfer(
                    (byte)(UsbRequestType.DeviceToHost | UsbRequestType.Vendor),
                    (byte)AndroidAccessoryUsbCommands.GetProtocol,
                    0,
                    1,
                    dataBytes,
                    0,
                    dataBytes.Length
                    );
            }
            catch (Exception)
            {
                // SendSetupTransfer will just blow a generic Exception if it
                // fails. If this gets changed, catch the more specific
                // Exception instead. Right now the USB host code always
                // throws Exception when anything goes wrong, and then you use
                // USBHostController.GetLastError to see what happened.
                //
                // See http://www.tinyclr.com/forum/2/3367/ for details.

                if (USBHostController.GetLastError() == USBH_ERROR.NoError)
                {
                    // Some other kind of Exception -- let it fly
                    throw;
                }

                // Fake a nonsensical protocol version to signal to the caller
                // that it's not gonna happen. dataBytes should still be 0
                // but just to make sure I'll reset it.

                dataBytes[0] = 0;
                dataBytes[1] = 0;
            }
            return (dataBytes[1] << 8) | dataBytes[0];
        }
    }
}
