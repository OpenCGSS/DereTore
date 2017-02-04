using System.Runtime.InteropServices;

namespace DereTore.HCA.Native {
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct CommentHeader {

        public uint COMM {
            get { return _comm; }
            set { _comm = value; }
        }

        public byte Length {
            get { return _length; }
            set { _length = value; }
        }

        private uint _comm;
        private byte _length;

    }
}
