using System;
using System.IO;

namespace DereTore.ACB.Serialization {
    public sealed partial class AcbSerializer {

        public AcbSerializer() {
            Alignment = 32;
        }

        public void Serialize<T>(T[] tableRows, Stream serializationStream) where T : UtfRowBase {
            if (tableRows == null) {
                throw new ArgumentNullException(nameof(tableRows));
            }
            var tableData = GetTableBytes(tableRows).RoundUpTo(Alignment);
            serializationStream.WriteBytes(tableData);
        }

        public uint Alignment {
            get {
                return _alignment;
            }
            set {
                if (value <= 0 || value % 16 != 0) {
                    throw new ArgumentException("Alignment should be a positive integer, also a multiple of 16.", nameof(value));
                }
                _alignment = value;
            }
        }

        private uint _alignment;

    }
}
