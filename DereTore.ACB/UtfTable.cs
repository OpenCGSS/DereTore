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
                throw new FormatException(string.Format("'@UTF' signature is not found in '{0}' at offset 0x{1}.", _acbFileName, offset.ToString("x8")));
            }
            CheckEncryption(stream, magic);
            using (var tableDataStream = GetTableDataStream(stream, offset)) {
                var header = GetUtfHeader(tableDataStream);
                _utfHeader = header;
                _rows = new Dictionary<string, UtfField>[header.RowCount];
                if (header.TableSize > 0) {
                    InitializeUtfSchema(stream, tableDataStream, 0x20);
                }
            }
        }

        public Stream Stream {
            get { return _stream; }
        }

        public string AcbFileName {
            get { return _acbFileName; }
        }

        public long Offset {
            get { return _offset; }
        }

        public long Size {
            get { return _size; }
        }

        public bool IsEncrypted {
            get { return _isEncrypted; }
        }

        public UtfHeader Header {
            get { return _utfHeader; }
        }

        public Dictionary<string, UtfField>[] Rows {
            get { return _rows; }
        }

    }
}
