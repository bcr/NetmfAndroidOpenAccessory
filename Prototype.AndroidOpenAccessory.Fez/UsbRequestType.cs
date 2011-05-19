using System;

namespace Prototype.AndroidOpenAccessory.Fez
{
    [Flags]
    enum UsbRequestType : byte
    {
        None = 0x00,
        DeviceToHost = 0x80,
        Vendor = 0x40,
    }

}
