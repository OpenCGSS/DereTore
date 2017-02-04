using System.Runtime.InteropServices;

namespace DereTore.HCA.Native {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CipherHeader {

        public uint CIPH {
            get { return _ciph; }
            set { _ciph = value; }
        }

        public ushort Type {
            get { return _type; }
            set { _type = value; }
        }

        private uint _ciph;
        private ushort _type;

    }
}
