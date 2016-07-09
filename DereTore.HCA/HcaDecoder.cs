using System;
using System.Diagnostics;
using System.IO;

namespace DereTore.HCA {
    public sealed partial class HcaDecoder : HcaReader {

        public HcaDecoder(Stream sourceStream)
            : this(sourceStream, new DecodeParams()) {
        }

        public HcaDecoder(Stream sourceStream, DecodeParams decodeParam)
            : base(sourceStream) {
            HcaHelper.TranslateTables();
            _hcaInfo = new HcaInfo() {
                CiphKey1 = decodeParam.Key1,
                CiphKey2 = decodeParam.Key2
            };
            _decodeParams = decodeParam.Clone();
            _status = new DecodeStatus();
            _ath = new Ath();
            _cipher = new Cipher();
        }

        internal bool HasMore() {
            return _status.BlockIndex < _hcaInfo.BlockCount;
        }

        private bool GenerateWaveData(Stream source, byte[] destination, uint count, byte[] buffer, DecodeToBufferFunc modeFunc, out int bytesDecoded) {
            source.Seek(_status.DataCursor, SeekOrigin.Begin);
            var cursor = 0;
            for (uint l = 0; l < count; ++l) {
                source.Read(buffer, 0, buffer.Length);
                if (!DecodeBlock(buffer)) {
                    bytesDecoded = 0;
                    return false;
                }
                for (int i = 0; i < 8; ++i) {
                    for (int j = 0; j < 0x80; ++j) {
                        for (uint k = 0; k < _hcaInfo.ChannelCount; ++k) {
                            var f = _channels[k].Wave[i * 0x80 + j] * _decodeParams.Volume * _hcaInfo.RvaVolume;
                            HcaHelper.Clamp(ref f, -1f, 1f);
                            cursor += modeFunc(f, destination, cursor);
                        }
                    }
                }
            }
            bytesDecoded = cursor;
            return true;
        }

        private bool DecodeBlock(byte[] blockData) {
            if (blockData.Length < _hcaInfo.BlockSize) {
                return false;
            }
            var checksum = HcaHelper.Checksum(blockData, 0);
            if (checksum != 0) {
                return false;
            }
            _cipher.Decrypt(blockData);
            var d = new DataBits(blockData, _hcaInfo.BlockSize);
            int magic = d.GetBit(16);
            if (magic == 0xffff) {
                int a = (d.GetBit(9) << 8) - d.GetBit(7);
                for (uint i = 0; i < _hcaInfo.ChannelCount; ++i) {
                    _channels[i].Decode1(d, _hcaInfo.CompR09, a, _ath.Table);
                }
                for (int i = 0; i < 8; ++i) {
                    for (uint j = 0; j < _hcaInfo.ChannelCount; ++j) {
                        _channels[j].Decode2(d);
                    }
                    for (uint j = 0; j < _hcaInfo.ChannelCount; ++j) {
                        _channels[j].Decode3(_hcaInfo.CompR09, _hcaInfo.CompR08, _hcaInfo.CompR07 + _hcaInfo.CompR06, _hcaInfo.CompR05);
                    }
                    for (uint j = 0; j < _hcaInfo.ChannelCount - 1; ++j) {
                        Channel.Decode4(ref _channels[j], ref _channels[j + 1], i, _hcaInfo.CompR05 - _hcaInfo.CompR06, _hcaInfo.CompR06, _hcaInfo.CompR07);
                    }
                    for (uint j = 0; j < _hcaInfo.ChannelCount; ++j) {
                        _channels[j].Decode5(i);
                    }
                }
            } else {
                Debug.Print("Warning: magic is {0}, expected 0xffff(65535).", magic);
                return false;
            }
            _status.DataCursor += (uint)blockData.Length;
            return true;
        }

        private byte[] GetHcaBlockBuffer() {
            return _hcaBlockBuffer ?? (_hcaBlockBuffer = new byte[_hcaInfo.BlockSize]);
        }

        private DecodeToBufferFunc GetDecodeToBufferFunc() {
            switch (_decodeParams.Mode) {
                case SamplingMode.S16:
                    return WaveHelper.DecodeToBufferInS16;
                case SamplingMode.Float:
                    return WaveHelper.DecodeToBufferInR32;
                case SamplingMode.S32:
                case SamplingMode.U8:
                case SamplingMode.S24:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException("_decodeParams.Mode");
            }
        }

        private int GetSampleBitsFromParams() {
            switch (_decodeParams.Mode) {
                case SamplingMode.Float:
                    return 32;
                case SamplingMode.S16:
                    return 16;
                case SamplingMode.S24:
                    return 24;
                case SamplingMode.S32:
                    return 32;
                case SamplingMode.U8:
                    return 8;
                default:
                    throw new ArgumentOutOfRangeException("_decodeParams.Mode");
            }
        }

        private byte[] _hcaBlockBuffer;
        private readonly Ath _ath;
        private readonly Cipher _cipher;
        private Channel[] _channels;
        private readonly DecodeParams _decodeParams;
        private DecodeStatus _status;

    }
}