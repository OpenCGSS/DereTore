using System.Runtime.InteropServices;

namespace DereTore.HCA.Native {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AthHeader {

        public uint ATH {
            get { return _ath; }
            set { _ath = value; }
        }

        public ushort Type {
            get { return _type; }
            set { _type = value; }
        }

        private uint _ath;
        private ushort _type;

    }
}
