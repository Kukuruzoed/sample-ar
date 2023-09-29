using System;
using System.Runtime.InteropServices;

namespace UnityHcap.Scripts
{
    [StructLayout(LayoutKind.Explicit)]
    struct HcapBuffer
    {
        [FieldOffset(0)]
        public Byte[] Bytes;

        [FieldOffset(0)]
        public ushort[] UShorts;
    }
}