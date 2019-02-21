using System;
using System.Runtime.InteropServices;

namespace DereTore.Exchange.Audio.HCA {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct Channel {

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 0x80)]
        public float[] Block;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 0x80)]
        public float[] Base;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 0x80)]
        public sbyte[] Value;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 0x80)]
        public sbyte[] Scale;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 8)]
        public sbyte[] Value2;
        [MarshalAs(UnmanagedType.I4)]
        public int Type;
        // Original type: public char *
        public IntPtr Value3;
        [MarshalAs(UnmanagedType.U4)]
        public uint Count;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 0x80)]
        public float[] Wav1;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 0x80)]
        public float[] Wav2;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 0x80)]
        public float[] Wav3;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 8 * 0x80)]
        public float[] Wave;

    }
}
