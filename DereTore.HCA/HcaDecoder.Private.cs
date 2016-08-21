using System;
using System.IO;

namespace DereTore.HCA {
    partial class HcaDecoder {

        private void Initialize() {
            ParseHeaders();
            InitializeDecodeComponents();
        }

        private void InitializeDecodeComponents() {
            if (!_ath.Initialize(_hcaInfo.AthType, _hcaInfo.SamplingRate)) {
                throw new HcaException(ErrorMessages.GetAthInitializationFailed(), ActionResult.AthInitFailed);
            }
            if (!_cipher.Initialize(_hcaInfo.CiphType, _decodeParams.Key1, _decodeParams.Key2)) {
                throw new HcaException(ErrorMessages.GetCiphInitializationFailed(), ActionResult.CiphInitFailed);
            }
            _channels = new Channel[0x10];
            for (var i = 0; i < _channels.Length; ++i) {
                _channels[i] = Channel.CreateDefault();
            }
            var r = new byte[10];
            uint b = _hcaInfo.ChannelCount / _hcaInfo.CompR03;
            if (_hcaInfo.CompR07 != 0 && b > 1) {
                uint rIndex = 0;
                for (uint i = 0; i < _hcaInfo.CompR03; ++i, rIndex += b) {
                    switch (b) {
                        case 2:
                        case 3:
                            r[rIndex] = 1;
                            r[rIndex + 1] = 2;
                            break;
                        case 4:
                            r[rIndex] = 1;
                            r[rIndex + 1] = 2;
                            if (_hcaInfo.CompR04 == 0) {
                                r[rIndex + 2] = 1;
                                r[rIndex + 3] = 2;
                            }
                            break;
                        case 5:
                            r[rIndex] = 1;
                            r[rIndex + 1] = 2;
                            if (_hcaInfo.CompR04 <= 2) {
                                r[rIndex + 3] = 1;
                                r[rIndex + 4] = 2;
                            }
                            break;
                        case 6:
                        case 7:
                            r[rIndex] = 1;
                            r[rIndex + 1] = 2;
                            r[rIndex + 4] = 1;
                            r[rIndex + 5] = 2;
                            break;
                        case 8:
                            r[rIndex] = 1;
                            r[rIndex + 1] = 2;
                            r[rIndex + 4] = 1;
                            r[rIndex + 5] = 2;
                            r[rIndex + 6] = 1;
                            r[rIndex + 7] = 2;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("b");
                    }
                }
            }
            for (uint i = 0; i < _hcaInfo.ChannelCount; ++i) {
                _channels[i].Type = r[i];
                _channels[i].Value3 = _hcaInfo.CompR06 + _hcaInfo.CompR07;
                _channels[i].Count = _hcaInfo.CompR06 + (r[i] != 2 ? _hcaInfo.CompR07 : 0);
            }
        }

        private void DecodeBlock(byte[] blockData, ref DecodeStatus status) {
            if (blockData == null) {
                throw new ArgumentNullException(nameof(blockData));
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
            status.DataCursor += (uint)blockData.Length;
        }

        private byte[] GetHcaBlockBuffer() {
            return _hcaBlockBuffer ?? (_hcaBlockBuffer = new byte[_hcaInfo.BlockSize]);
        }

        private DecodeToBufferFunc GetDecodeToBufferFunc() {
            var mode = _decodeParams.Mode;
            switch (mode) {
                case SamplingMode.S16:
                    return WaveHelper.DecodeToBufferInS16;
                case SamplingMode.Float:
                    return WaveHelper.DecodeToBufferInR32;
                case SamplingMode.S32:
                case SamplingMode.U8:
                case SamplingMode.S24:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }

        private int GetSampleBitsFromParams() {
            var mode = _decodeParams.Mode;
            switch (mode) {
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
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }

        private byte[] _hcaBlockBuffer;
        private readonly Ath _ath;
        private readonly Cipher _cipher;
        private Channel[] _channels;
        private readonly DecodeParams _decodeParams;

    }
}