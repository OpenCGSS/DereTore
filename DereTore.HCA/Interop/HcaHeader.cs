using System.Runtime.InteropServices;

namespace DereTore.HCA.Interop {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct HcaHeader {

        public uint HCA {
            get { return _hca; }
            set { _hca = value; }
        }

        public ushort Version {
            get { return _version; }
            set { _version = value; }
        }

        public ushort DataOffset {
            get { return _dataOffset; }
            set { _dataOffset = value; }
        }

        private uint _hca;
        private ushort _version;
        private ushort _dataOffset;
    }
}
