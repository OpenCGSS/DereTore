using System.Collections.Generic;

namespace DereTore.Exchange.Archive.ACB.Serialization {
    internal sealed partial class UtfTableImage {

        internal UtfTableImage(string name, uint alignment) {
            Rows = new List<List<UtfFieldImage>>();
            TableName = name;
            Alignment = alignment;
        }

        public string TableName { get; }
        public List<List<UtfFieldImage>> Rows { get; }
        public uint Alignment { get; }

        private byte[] TableNameBytesCache { get; set; }

        public override string ToString() {
            return $"UtfTableImage {{{TableName}}}";
        }
    }
}
