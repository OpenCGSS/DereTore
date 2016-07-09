using System;
using System.IO;
using System.Runtime.InteropServices;
using DereTore.HCA.Interop;

namespace DereTore.HCA {
    public sealed class HcaCipherConverter : HcaReader {

        public HcaCipherConverter(Stream sourceStream, Stream outputStream, CipherConfig ccFrom, CipherConfig ccTo)
            : base(sourceStream) {
            _outputStream = outputStream;
            _ccFrom = ccFrom;
            _ccTo = ccTo;
        }

        public bool SetNewCipherType() {
            var dataOffset = _hcaInfo.DataOffset;
            var buffer = new byte[dataOffset];
            var sourceStream = SourceStream;
            var outputStream = _outputStream;

            var cipherOffset = LocateCipherFieldOffset(sourceStream, 0);
            if (cipherOffset <= 0) {
                return false;
            }
            sourceStream.Seek(0, SeekOrigin.Begin);
            sourceStream.Read(buffer, 0, buffer.Length);
            var u = (ushort)_ccTo.CipherType;
            if (BitConverter.IsLittleEndian) {
                u = HcaHelper.SwapEndian(u);
            }
            var cipherTypeBytes = BitConverter.GetBytes(u);
            buffer[cipherOffset] = cipherTypeBytes[0];
            buffer[cipherOffset + 1] = cipherTypeBytes[1];
            FixDataBlock(buffer);
            outputStream.Write(buffer, 0, buffer.Length);
            return true;
        }

        public bool ConvertData() {
            var sourceStream = SourceStream;
            var outputStream = _outputStream;
            var dataOffset = _hcaInfo.DataOffset;
            var totalBlockCount = _hcaInfo.BlockCount;
            var buffer = new byte[_hcaInfo.BlockSize];

            sourceStream.Seek(dataOffset, SeekOrigin.Begin);
            for (var i = 0; i < totalBlockCount; ++i) {
                var read = sourceStream.Read(buffer, 0, buffer.Length);
                if (read < buffer.Length) {
                    return false;
                }
                var r = ConvertBlock(buffer, outputStream);
                if (!r) {
                    return false;
                }
            }
            return true;
        }

        public void InitializeCiphers() {
            _cipherFrom = new Cipher();
            _cipherFrom.Initialize(_hcaInfo.CiphType, _ccFrom.Key1, _ccFrom.Key2);
            _cipherTo = new Cipher();
            _cipherTo.Initialize(_ccTo.CipherType, _ccTo.Key1, _ccTo.Key2);
        }

        private bool ConvertBlock(byte[] blockData, Stream outputStream) {
            var checksum = HcaHelper.Checksum(blockData, 0);
            if (checksum != 0) {
                return false;
            }
            _cipherFrom.Decrypt(blockData);
            var dataClass = new DataBits(blockData, (uint)blockData.Length);
            var magic = dataClass.GetBit(16);
            if (magic != 0xffff) {
                return false;
            }
            _cipherTo.Encrypt(blockData);
            FixDataBlock(blockData);
            outputStream.Write(blockData, 0, blockData.Length);
            return true;
        }

        private static void FixDataBlock(byte[] blockData) {
            var length = blockData.Length;
            var sum = HcaHelper.Checksum(blockData, 0, length - 2);
            if (BitConverter.IsLittleEndian) {
                sum = HcaHelper.SwapEndian(sum);
            }
            var sumBytes = BitConverter.GetBytes(sum);
            blockData[length - 2] = sumBytes[0];
            blockData[length - 1] = sumBytes[1];
        }

        private int LocateCipherFieldOffset(Stream stream, int startOffset) {
            stream.Seek(startOffset, SeekOrigin.Begin);
            uint v;
            // HCA
            v = stream.PeekUInt32();
            if (MagicValues.IsMagicMatch(v, MagicValues.HCA)) {
                stream.Skip(Marshal.SizeOf(typeof(HcaHeader)));
            } else {
                return -1;
            }
            // FMT
            v = stream.PeekUInt32();
            if (MagicValues.IsMagicMatch(v, MagicValues.FMT)) {
                stream.Skip(Marshal.SizeOf(typeof(FormatHeader)));
            } else {
                return -1;
            }
            // COMP / DEC
            v = stream.PeekUInt32();
            if (MagicValues.IsMagicMatch(v, MagicValues.COMP)) {
                stream.Skip(Marshal.SizeOf(typeof(CompressHeader)));
            } else if (MagicValues.IsMagicMatch(v, MagicValues.DEC)) {
                stream.Skip(Marshal.SizeOf(typeof(DecodeHeader)));
            } else {
                return -1;
            }
            // VBR
            v = stream.PeekUInt32();
            if (MagicValues.IsMagicMatch(v, MagicValues.VBR)) {
                stream.Skip(Marshal.SizeOf(typeof(VbrHeader)));
            }
            // ATH
            v = stream.PeekUInt32();
            if (MagicValues.IsMagicMatch(v, MagicValues.ATH)) {
                stream.Skip(Marshal.SizeOf(typeof(AthHeader)));
            }
            // LOOP
            v = stream.PeekUInt32();
            if (MagicValues.IsMagicMatch(v, MagicValues.LOOP)) {
                stream.Skip(Marshal.SizeOf(typeof(LoopHeader)));
            }
            // CIPH
            v = stream.PeekUInt32();
            if (MagicValues.IsMagicMatch(v, MagicValues.CIPH)) {
                return (int)(stream.Position + 4);
            }
            return 0;
        }

        private readonly Stream _outputStream;
        private readonly CipherConfig _ccFrom;
        private readonly CipherConfig _ccTo;
        private Cipher _cipherFrom;
        private Cipher _cipherTo;

    }
}