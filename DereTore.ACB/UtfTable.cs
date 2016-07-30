using System;
using System.Collections.Generic;
using System.IO;

namespace DereTore.ACB {
    public partial class UtfTable : DisposableBase {

        public virtual void Initialize() {
            var stream = _stream;
            var offset = _offset;

            var magic = stream.PeekBytes(offset, 4);
            if (!AcbHelper.AreDataIdentical(magic, UtfSignature)) {
                throw new FormatException($"'@UTF' signature is not found in '{_acbFileName}' at offset 0x{offset.ToString("x8")}.");
            }
            CheckEncryption(magic);
            using (var tableDataStream = GetTableDataStream(stream, offset)) {
                var header = GetUtfHeader(tableDataStream);
                _utfHeader = header;
                _rows = new Dictionary<string, UtfField>[header.RowCount];
                if (header.TableSize > 0) {
                    InitializeUtfSchema(stream, tableDataStream, 0x20);
                }
            }
        }

        public Stream Stream => _stream;

        public string AcbFileName => _acbFileName;

        public long Offset => _offset;

        public long Size => _size;

        public bool IsEncrypted => _isEncrypted;

        public UtfHeader Header => _utfHeader;

        public Dictionary<string, UtfField>[] Rows => _rows;

    }
}
