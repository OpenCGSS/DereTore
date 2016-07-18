using System;
using System.IO;

namespace DereTore.HCA {
    public sealed partial class HcaDecoder : HcaReader {

        internal bool HasMore() {
            return _status.BlockIndex < _hcaInfo.BlockCount;
        }

        private void GenerateWaveDataBlocks(Stream source, byte[] destination, uint count, byte[] buffer, DecodeToBufferFunc modeFunc, out int bytesDecoded) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }
            if (destination == null) {
                throw new ArgumentNullException("destination");
            }
            if (buffer == null) {
                throw new ArgumentNullException("buffer");
            }
            if (modeFunc == null) {
                throw new ArgumentNullException("modeFunc");
            }
            source.Seek(_status.DataCursor, SeekOrigin.Begin);
            var cursor = 0;
            for (uint l = 0; l < count; ++l) {
                source.Read(buffer, 0, buffer.Length);
                DecodeBlock(buffer);
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
        }

        private void DecodeBlock(byte[] blockData) {
            if (blockData == null) {
                throw new ArgumentNullException("blockData");
            }
            if (blockData.Length < _hcaInfo.BlockSize) {
                throw new HcaException(ErrorMessages.GetInvalidParameter("blockData.Length"), ActionResult.InvalidParameter);
            }
            var checksum = HcaHelper.Checksum(blockData, 0);
            if (checksum != 0) {
                throw new HcaException(ErrorMessages.GetChecksumNotMatch(0, checksum), ActionResult.ChecksumNotMatch);
            }
            _cipher.Decrypt(blockData);
            var d = new DataBits(blockData, _hcaInfo.BlockSize);
            int magic = d.GetBit(16);
            if (magic != 0xffff) {
                throw new HcaException(ErrorMessages.GetMagicNotMatch(0xffff, magic), ActionResult.MagicNotMatch);
            }
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
            _status.DataCursor += (uint)blockData.Length;
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