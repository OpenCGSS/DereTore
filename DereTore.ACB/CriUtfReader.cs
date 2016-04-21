using System;
using System.IO;
using System.Text;

namespace DereTore.ACB {
    internal sealed class CriUtfReader {

        public CriUtfReader() {
            _isEncrypted = false;
        }

        public CriUtfReader(byte seed, byte increment, bool isEncrypted) {
            _seed = seed;
            _increment = increment;
            _isEncrypted = isEncrypted;
        }

        public byte[] GetBytes(Stream stream, long baseOffset, int size, long utfOffset) {
            var ret = stream.ParseSimpleOffset(baseOffset + utfOffset, size);

            if (IsEncrypted) {
                if (utfOffset < _currentUtfOffset) {
                    // reset, maybe add some sort of index later?
                    _currentUtfOffset = 0;
                }

                if (_currentUtfOffset == 0) {
                    // reset or initialize
                    _currentXor = _seed;
                }

                // catch up to this offset
                for (var j = _currentUtfOffset; j < utfOffset; j++) {
                    if (j > 0) {
                        _currentXor *= _increment;
                    }

                    _currentUtfOffset++;
                }

                // decrypt this offset
                for (long i = 0; i < size; i++) {
                    // first byte of UTF table must be XOR'd with the seed
                    if ((_currentUtfOffset != 0) || (i > 0)) {
                        _currentXor *= _increment;
                    }

                    ret[i] ^= _currentXor;
                    _currentUtfOffset++;
                }
            }

            return ret;
        }

        public string ReadAsciiString(Stream stream, long baseOffset, long utfOffset) {
            var asciiVal = new StringBuilder();
            var fileSize = stream.Length;

            if (IsEncrypted) {
                stream.Position = baseOffset + utfOffset;

                if (utfOffset < _currentUtfStringOffset) {
                    // reset, maybe add some sort of index later?
                    _currentUtfStringOffset = 0;
                }

                if (_currentUtfStringOffset == 0) {
                    // reset or initialize
                    _currentStringXor = _seed;
                }

                for (long j = _currentUtfStringOffset; j < utfOffset; j++) {
                    if (j > 0) {
                        _currentStringXor *= _increment;
                    }

                    _currentUtfStringOffset++;
                }

                for (long i = utfOffset; i < (fileSize - (baseOffset + utfOffset)); i++) {
                    _currentStringXor *= _increment;
                    _currentUtfStringOffset++;

                    var encryptedByte = (byte)stream.ReadByte();
                    var decryptedByte = (byte)(encryptedByte ^ _currentStringXor);

                    if (decryptedByte == 0) {
                        break;
                    } else {
                        asciiVal.Append(Convert.ToChar(decryptedByte));
                    }
                }
            } else {
                asciiVal.Append(stream.ReadAsciiString(baseOffset + utfOffset));
            }

            return asciiVal.ToString();
        }

        public byte ReadByte(Stream stream, long baseOffset, long utfOffset) {
            return GetBytes(stream, baseOffset, 1, utfOffset)[0];
        }

        public sbyte ReadSByte(Stream stream, long baseOffset, long utfOffset) {
            unchecked {
                return (sbyte)GetBytes(stream, baseOffset, 1, utfOffset)[0];
            }
        }

        public ushort ReadUInt16(Stream stream, long baseOffset, long utfOffset) {
            byte[] temp = GetBytes(stream, baseOffset, 2, utfOffset);

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(temp);
            }
            return BitConverter.ToUInt16(temp, 0);
        }

        public short ReadInt16(Stream stream, long baseOffset, long utfOffset) {
            var temp = GetBytes(stream, baseOffset, 2, utfOffset);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(temp);
            }
            return BitConverter.ToInt16(temp, 0);
        }

        public uint ReadUInt32(Stream stream, long baseOffset, long utfOffset) {
            var temp = GetBytes(stream, baseOffset, 4, utfOffset);

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(temp);
            }
            return BitConverter.ToUInt32(temp, 0);
        }

        public int ReadInt32(Stream stream, long baseOffset, long utfOffset) {
            var temp = GetBytes(stream, baseOffset, 4, utfOffset);

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(temp);
            }
            return BitConverter.ToInt32(temp, 0);
        }

        public ulong ReadUInt64(Stream stream, long baseOffset, long utfOffset) {
            var temp = GetBytes(stream, baseOffset, 8, utfOffset);

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(temp);
            }
            return BitConverter.ToUInt64(temp, 0);
        }

        public float ReadSingle(Stream stream, long baseOffset, long utfOffset) {
            var temp = GetBytes(stream, baseOffset, 4, utfOffset);

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(temp);
            }
            return BitConverter.ToSingle(temp, 0);
        }

        public bool IsEncrypted {
            get { return _isEncrypted; }
            set { _isEncrypted = value; }
        }

        private bool _isEncrypted;
        private readonly byte _increment;
        private readonly byte _seed;
        private byte _currentXor;
        private long _currentUtfOffset;
        private byte _currentStringXor;
        private long _currentUtfStringOffset;

    }
}