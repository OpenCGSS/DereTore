using System;
using System.Diagnostics;
using System.IO;

namespace DereTore.HCA {
    public sealed partial class HcaDecoder {

        public HcaDecoder(Stream sourceStream)
            : this(sourceStream, new DecodeParam()) {
        }

        public HcaDecoder(Stream sourceStream, DecodeParam decodeParam) {
            HcaHelper.TranslateTables();
            _info = new HcaInfo() {
                CiphKey1 = decodeParam.Key1,
                CiphKey2 = decodeParam.Key2
            };
            _decodeParam = decodeParam.Clone();
            _sourceStream = sourceStream;
            _status = new DecodeStatus();
            _ath = new Ath();
            _cipher = new Cipher();
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
                        for (uint k = 0; k < _info.ChannelCount; ++k) {
                            var f = _channels[k].Wave[i * 0x80 + j] * _decodeParam.Volume * _info.RvaVolume;
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
            if (blockData.Length < _info.BlockSize) {
                return false;
            }
            if (HcaHelper.CheckSum(blockData, 0) != 0) {
                return false;
            }
            _cipher.Mask(blockData);
            var d = new Data(blockData, _info.BlockSize);
            int magic = d.GetBit(16);
            if (magic == 0xffff) {
                int a = (d.GetBit(9) << 8) - d.GetBit(7);
                for (uint i = 0; i < _info.ChannelCount; ++i) {
                    _channels[i].Decode1(d, _info.CompR09, a, _ath.Table);
                }
                for (int i = 0; i < 8; ++i) {
                    for (uint j = 0; j < _info.ChannelCount; ++j) {
                        _channels[j].Decode2(d);
                    }
                    for (uint j = 0; j < _info.ChannelCount; ++j) {
                        _channels[j].Decode3(_info.CompR09, _info.CompR08, _info.CompR07 + _info.CompR06, _info.CompR05);
                    }
                    for (uint j = 0; j < _info.ChannelCount - 1; ++j) {
                        Channel.Decode4(ref _channels[j], ref _channels[j + 1], i, _info.CompR05 - _info.CompR06, _info.CompR06, _info.CompR07);
                    }
                    for (uint j = 0; j < _info.ChannelCount; ++j) {
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
            return _hcaBlockBuffer ?? (_hcaBlockBuffer = new byte[_info.BlockSize]);
        }

        private DecodeToBufferFunc GetDecodeToBufferFunc() {
            switch (_decodeParam.Mode) {
                case SamplingMode.S16:
                    return DecodeToBufferInS16;
                case SamplingMode.Float:
                    return DecodeToBufferInR32;
                case SamplingMode.S32:
                case SamplingMode.U8:
                case SamplingMode.S24:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException("_decodeParam.Mode");
            }
        }

        private int GetSampleBitsFromParams() {
            switch (_decodeParam.Mode) {
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
                    throw new ArgumentOutOfRangeException("_decodeParam.Mode");
            }
        }

        private byte[] _hcaBlockBuffer;
        private readonly Ath _ath;
        private readonly Cipher _cipher;
        private HcaInfo _info;
        private float _lengthInSecs;
        private int _lengthInSamples;
        private Channel[] _channels;
        private readonly DecodeParam _decodeParam;
        private readonly Stream _sourceStream;
        private DecodeStatus _status;

    }
}
