using System;
using System.Collections.Generic;
using System.IO;

namespace DereTore.ACB {
    public partial class UtfTable : DisposableBase {

        public Stream Stream => _stream;

        public string AcbFileName => _acbFileName;

        public long Offset => _offset;

        public long Size => _size;

        public bool IsEncrypted => _isEncrypted;

        public UtfHeader Header => _utfHeader;

        public Dictionary<string, UtfField>[] Rows => _rows;

    }
}
