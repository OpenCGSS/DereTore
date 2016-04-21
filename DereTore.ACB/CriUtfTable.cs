using System;
using System.Collections.Generic;
using System.IO;

namespace DereTore.ACB {

    // thanks to hcs for all of this!!!
    public class CriUtfTable {

        internal CriUtfTable() {
        }

        public void Initialize(string fileName, Stream stream, long offset) {
            _sourceFile = fileName;
            _baseOffset = offset;

            MagicBytes = stream.ParseSimpleOffset(BaseOffset, 4);

            // check if file is decrypted and get decryption keys if needed
            CheckEncryption(stream);

            // write (decrypted) utf header to file
            using (var utfMemoryStream = GetResultTable(stream, offset)) {
                _isEncrypted = false; // since we've decrypted to a temp file
                _utfReader.IsEncrypted = false;

                if (AcbHelper.CompareSegment(MagicBytes, 0, Signature)) {
                    // read header
                    InitializeUtfHeader(utfMemoryStream);

                    // initialize rows
                    _rows = new Dictionary<string, CriField>[NumberOfRows];

                    // read schema
                    if (TableSize > 0) {
                        InitializeUtfSchema(stream, utfMemoryStream, 0x20);
                    }
                } else {
                    //Dictionary<string, byte> foo = GetKeysForEncryptedUtfTable(this.MagicBytes);
                    throw new FormatException(string.Format("@UTF signature not found at offset <0x{0}>.", offset.ToString("X8")));
                }
            }
        }

        public string SourceFile {
            get { return _sourceFile; }
        }

        public long BaseOffset {
            get { return _baseOffset; }
        }

        public bool IsEncrypted {
            get { return _isEncrypted; }
        }

        public uint TableSize {
            get { return _tableSize; }
        }

        public uint RowOffset {
            get { return _rowOffset; }
        }

        public uint StringTableOffset {
            get { return _stringTableOffset; }
        }

        public uint DataOffset {
            get { return _dataOffset; }
        }

        public uint TableNameOffset {
            get { return _tableNameOffset; }
        }

        public string TableName {
            get { return _tableName; }
        }

        public ushort NumberOfFields {
            get { return _numberOfFields; }
        }

        public ushort RowSize {
            get { return _rowSize; }
        }

        public uint NumberOfRows {
            get { return _numberOfRows; }
        }

        public Dictionary<string, CriField>[] Rows {
            get { return _rows; }
        }

        protected static T GetUtfFieldForRow<T>(CriUtfTable utfTable, int rowIndex, string key) {
            return (T)GetUtfFieldForRow(utfTable, rowIndex, key);
        }

        protected static ulong GetOffsetForUtfFieldForRow(CriUtfTable utfTable, int rowIndex, string key) {
            ulong ret = 0;
            if (utfTable.Rows.GetLength(0) > rowIndex) {
                if (utfTable.Rows[rowIndex].ContainsKey(key)) {
                    ret = utfTable.Rows[rowIndex][key].Offset;
                }
            }
            return ret;
        }

        protected static ulong GetSizeForUtfFieldForRow(CriUtfTable utfTable, int rowIndex, string key) {
            ulong ret = 0;
            if (utfTable.Rows.GetLength(0) > rowIndex) {
                if (utfTable.Rows[rowIndex].ContainsKey(key)) {
                    ret = utfTable.Rows[rowIndex][key].Size;
                }
            }
            return ret;
        }

        protected static Dictionary<string, byte> GetKeysForEncryptedUtfTable(byte[] encryptedUtfSignature) {
            var keys = new Dictionary<string, byte>();
            var keysFound = false;
            for (var seed = 0; seed <= byte.MaxValue; seed++) {
                if (!keysFound) {
                    // match first char
                    if ((encryptedUtfSignature[0] ^ seed) == Signature[0]) {
                        for (var increment = 0; increment <= byte.MaxValue; increment++) {
                            if (!keysFound) {
                                var m = (byte)(seed * increment);
                                if ((encryptedUtfSignature[1] ^ m) == Signature[1]) {
                                    var t = (byte)increment;

                                    for (var j = 2; j < Signature.Length; j++) {
                                        m *= t;

                                        if ((encryptedUtfSignature[j] ^ m) != Signature[j]) {
                                            break;
                                        } else if (j == (Signature.Length - 1)) {
                                            keys.Add(LcgSeedKey, (byte)seed);
                                            keys.Add(LcgIncrementKey, (byte)increment);
                                            keysFound = true;
                                        }
                                    }
                                }
                            } else {
                                break;
                            }
                        }
                    }
                } else {
                    break;
                }
            }
            return keys;
        }

        private MemoryStream GetResultTable(Stream stream, long offsetToUtfTable) {
            var totalBytesRead = 0;
            var buffer = new byte[Constants.FileChunkSize];

            var tableSize = (int)_utfReader.ReadUInt32(stream, offsetToUtfTable, 4) + 8;

            if (IsEncrypted) {
                var memoryStream = new MemoryStream();
                while (totalBytesRead < tableSize) {
                    int maxRead;
                    if (tableSize - totalBytesRead > buffer.Length) {
                        maxRead = buffer.Length;
                    } else {
                        maxRead = tableSize - totalBytesRead;
                    }
                    buffer = _utfReader.GetBytes(stream, offsetToUtfTable, maxRead, totalBytesRead);
                    memoryStream.Write(buffer, 0, maxRead);
                    totalBytesRead += maxRead;
                }
                memoryStream.Seek(0, SeekOrigin.Begin);
                memoryStream.Capacity = (int)memoryStream.Length;
                return memoryStream;
            } else {
                return AcbHelper.ExtractChunkToStream(stream, (ulong)offsetToUtfTable, (ulong)tableSize);
            }
        }

        private void CheckEncryption(Stream stream) {
            if (AcbHelper.CompareSegment(MagicBytes, 0, Signature)) {
                _isEncrypted = false;
                _utfReader = new CriUtfReader();
            } else {
                _isEncrypted = true;
                var lcgKeys = GetKeysForEncryptedUtfTable(MagicBytes);

                if (lcgKeys.Count != 2) {
                    throw new FormatException(string.Format("Unable to decrypt UTF table at offset: 0x{0}", BaseOffset.ToString("X8")));
                } else {
                    _utfReader = new CriUtfReader(lcgKeys[LcgSeedKey], lcgKeys[LcgIncrementKey], IsEncrypted);
                }

                MagicBytes = _utfReader.GetBytes(stream, BaseOffset, 4, 0);
            }
        }

        private void InitializeUtfHeader(Stream utfTableStream) {
            _tableSize = utfTableStream.ReadUInt32BE(4);

            Unknown1 = utfTableStream.ReadUInt16BE(8);

            _rowOffset = (uint)utfTableStream.ReadUInt16BE(0xA) + 8;
            _stringTableOffset = utfTableStream.ReadUInt32BE(0xC) + 8;
            _dataOffset = utfTableStream.ReadUInt32BE(0x10) + 8;

            _tableNameOffset = utfTableStream.ReadUInt32BE(0x14);
            _tableName = utfTableStream.ReadAsciiString(StringTableOffset + TableNameOffset);

            _numberOfFields = utfTableStream.ReadUInt16BE(0x18);

            _rowSize = utfTableStream.ReadUInt16BE(0x1A);
            _numberOfRows = utfTableStream.ReadUInt32BE(0x1C);
        }

        private void InitializeUtfSchema(Stream sourceFs, Stream utfTableFs, long schemaOffset) {
            for (uint i = 0; i < NumberOfRows; i++) {
                var currentOffset = schemaOffset;
                long currentRowBase = RowOffset + (RowSize * i);
                long currentRowOffset = 0;
                Rows[i] = new Dictionary<string, CriField>();

                // parse fields
                for (ushort j = 0; j < NumberOfFields; j++) {
                    var field = new CriField {
                        Type = utfTableFs.ReadByte(currentOffset)
                    };

                    long nameOffset = utfTableFs.ReadInt32BE(currentOffset + 1);
                    field.Name = utfTableFs.ReadAsciiString(StringTableOffset + nameOffset);

                    // each row will have a constant
                    switch ((field.Type & (byte)ColumnStorage.Mask)) {
                        case (byte)ColumnStorage.Constant:
                        case (byte)ColumnStorage.Constant2:
                            // capture offset of constant
                            var constantOffset = currentOffset + 5;

                            // read the constant depending on the type
                            long dataOffset;
                            switch ((ColumnType)(field.Type & (byte)ColumnType.Mask)) {
                                case ColumnType.String:
                                    dataOffset = utfTableFs.ReadInt32BE(constantOffset);
                                    field.Value = utfTableFs.ReadAsciiString(StringTableOffset + dataOffset);
                                    currentOffset += 4;
                                    break;
                                case ColumnType.UInt64:
                                    field.Value = utfTableFs.ReadUInt64BE(constantOffset);
                                    currentOffset += 8;
                                    break;
                                case ColumnType.Data:
                                    dataOffset = utfTableFs.ReadUInt32BE(constantOffset);
                                    long dataSize = utfTableFs.ReadUInt32BE(constantOffset + 4);
                                    field.Offset = (ulong)(BaseOffset + DataOffset + dataOffset);
                                    field.Size = (ulong)dataSize;

                                    // don't think this is encrypted, need to check
                                    field.Value = sourceFs.ParseSimpleOffset((long)field.Offset, (int)dataSize);
                                    currentOffset += 8;
                                    break;
                                case ColumnType.Single:
                                    field.Value = utfTableFs.ReadSingleBE(constantOffset);
                                    currentOffset += 4;
                                    break;
                                case ColumnType.Int32:
                                    field.Value = utfTableFs.ReadInt32BE(constantOffset);
                                    currentOffset += 4;
                                    break;
                                case ColumnType.UInt32:
                                    field.Value = utfTableFs.ReadUInt32BE(constantOffset);
                                    currentOffset += 4;
                                    break;
                                case ColumnType.Int16:
                                    field.Value = utfTableFs.ReadInt16BE(constantOffset);
                                    currentOffset += 2;
                                    break;
                                case ColumnType.UInt16:
                                    field.Value = utfTableFs.ReadUInt16BE(constantOffset);
                                    currentOffset += 2;
                                    break;
                                case ColumnType.SByte:
                                    field.Value = utfTableFs.ReadSByte(constantOffset);
                                    currentOffset += 1;
                                    break;
                                case ColumnType.Byte:
                                    field.Value = utfTableFs.ReadByte(constantOffset);
                                    currentOffset += 1;
                                    break;
                                default:
                                    throw new FormatException(string.Format("Unknown COLUMN TYPE at offset: 0x{0}", currentOffset.ToString("X8")));
                            } // switch (field.Type & COLUMN_TYPE_MASK)
                            break;
                        case (byte)ColumnStorage.PerRow:
                            // read the constant depending on the type
                            long rowDataOffset;
                            switch ((ColumnType)(field.Type & (byte)ColumnType.Mask)) {
                                case ColumnType.String:
                                    rowDataOffset = utfTableFs.ReadUInt32BE(currentRowBase + currentRowOffset);
                                    field.Value = utfTableFs.ReadAsciiString(StringTableOffset + rowDataOffset);
                                    currentRowOffset += 4;
                                    break;
                                case ColumnType.UInt64:
                                    field.Value = utfTableFs.ReadUInt64BE(currentRowBase + currentRowOffset);
                                    currentRowOffset += 8;
                                    break;
                                case ColumnType.Data:
                                    rowDataOffset = utfTableFs.ReadUInt32BE(currentRowBase + currentRowOffset);
                                    long rowDataSize = utfTableFs.ReadUInt32BE(currentRowBase + currentRowOffset + 4);
                                    field.Offset = (ulong)(BaseOffset + DataOffset + rowDataOffset);
                                    field.Size = (ulong)rowDataSize;

                                    // don't think this is encrypted
                                    field.Value = sourceFs.ParseSimpleOffset((long)field.Offset, (int)rowDataSize);
                                    currentRowOffset += 8;
                                    break;
                                case ColumnType.Single:
                                    field.Value = utfTableFs.ReadSingleBE(currentRowBase + currentRowOffset);
                                    currentRowOffset += 4;
                                    break;
                                case ColumnType.Int32:
                                    field.Value = utfTableFs.ReadInt32BE(currentRowBase + currentRowOffset);
                                    currentRowOffset += 4;
                                    break;
                                case ColumnType.UInt32:
                                    field.Value = utfTableFs.ReadUInt32BE(currentRowBase + currentRowOffset);
                                    currentRowOffset += 4;
                                    break;
                                case ColumnType.Int16:
                                    field.Value = utfTableFs.ReadInt16BE(currentRowBase + currentRowOffset);
                                    currentRowOffset += 2;
                                    break;
                                case ColumnType.UInt16:
                                    field.Value = utfTableFs.ReadUInt16BE(currentRowBase + currentRowOffset);
                                    currentRowOffset += 2;
                                    break;
                                case ColumnType.SByte:
                                    field.Value = utfTableFs.ReadSByte(currentRowBase + currentRowOffset);
                                    currentRowOffset += 1;
                                    break;
                                case ColumnType.Byte:
                                    field.Value = utfTableFs.ReadByte(currentRowBase + currentRowOffset);
                                    currentRowOffset += 1;
                                    break;
                                default:
                                    throw new FormatException(string.Format("Unknown COLUMN TYPE at offset: 0x{0}", currentOffset.ToString("X8")));
                            }
                            break;
                        default:
                            throw new FormatException(string.Format("Unknown COLUMN STORAGE at offset: 0x{0}", currentOffset.ToString("X8")));
                    }

                    // add field to dictionary
                    Rows[i].Add(field.Name, field);

                    // move to next field
                    currentOffset += 5; //  sizeof(CriField.Type + CriField.NameOffset)
                }
            }
        }

        private static object GetUtfFieldForRow(CriUtfTable utfTable, int rowIndex, string key) {
            object ret = null;
            if (utfTable.Rows.GetLength(0) > rowIndex) {
                if (utfTable.Rows[rowIndex].ContainsKey(key)) {
                    ret = utfTable.Rows[rowIndex][key].Value;
                }
            }
            return ret;
        }

        private ushort Unknown1 { set; get; }

        private byte[] MagicBytes { set; get; }

        private CriUtfReader _utfReader;
        private static readonly byte[] Signature = { 0x40, 0x55, 0x54, 0x46 };
        private static readonly string LcgSeedKey = "SEED";
        private static readonly string LcgIncrementKey = "INC";
        private string _sourceFile;
        private long _baseOffset;
        private bool _isEncrypted;
        private uint _tableSize;
        private Dictionary<string, CriField>[] _rows;
        private uint _rowOffset;
        private uint _stringTableOffset;
        private uint _dataOffset;
        private uint _tableNameOffset;
        private string _tableName;
        private ushort _numberOfFields;
        private ushort _rowSize;
        private uint _numberOfRows;
    }
}