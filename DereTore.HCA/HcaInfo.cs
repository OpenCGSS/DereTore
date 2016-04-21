using DereTore.HCA.Interop;

namespace DereTore.HCA {
    public struct HcaInfo {

        public uint Version;
        public uint DataOffset;
        public uint ChannelCount;
        public uint SamplingRate;
        public uint BlockCount;
        public uint FmtR01;
        public uint FmtR02;
        public uint BlockSize;
        public uint CompR01;
        public uint CompR02;
        public uint CompR03;
        public uint CompR04;
        public uint CompR05;
        public uint CompR06;
        public uint CompR07;
        public uint CompR08;
        public uint CompR09;
        public uint VbrR01;
        public uint VbrR02;
        public uint AthType;
        public uint LoopStart;
        public uint LoopEnd;
        public uint LoopR01;
        public uint LoopR02;
        public bool LoopFlag;
        public CipherType CiphType;
        public uint CiphKey1;
        public uint CiphKey2;
        public float RvaVolume;
        public uint CommentLength;
        public byte[] Comment;

    }
}
