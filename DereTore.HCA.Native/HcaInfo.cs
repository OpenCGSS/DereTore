using System.Runtime.InteropServices;

namespace DereTore.HCA.Native {
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct HcaInfo {

        public readonly ushort VersionMajor;
        public readonly ushort VersionMinor;
        public readonly uint ChannelCount;
        public readonly uint SamplingRate;
        public readonly uint BlockCount;
        public readonly ushort BlockSize;
        public readonly ushort AthType;
        [MarshalAs(UnmanagedType.Bool)]
        public readonly bool LoopExists;
        public readonly uint LoopStart;
        public readonly uint LoopEnd;
        public readonly uint CipherType;
        public readonly float RvaVolume;
        public readonly byte CommentLength;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 0x100)]
        public readonly byte[] Comment;
        public readonly ushort FmtR01, FmtR02;
        public readonly ushort CompR01, CompR02, CompR03, CompR04, CompR05, CompR06, CompR07, CompR08;
        public readonly uint CompR09;
        public readonly ushort VbrR01, VbrR02;
        public readonly ushort LoopR01, LoopR02;

    }
}
