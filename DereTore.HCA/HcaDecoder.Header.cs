using System;
using System.Collections.Generic;
using DereTore.HCA.Interop;

namespace DereTore.HCA {
    public partial class HcaDecoder {

        public bool ParseHeaders() {
            var stream = _sourceStream;
            uint v;
            // HCA
            v = stream.PeekUInt32();
            if (MagicValues.IsMagicMatch(v, MagicValues.HCA)) {
                HcaHeader header;
                stream.Read(out header);
                _info.Version = HcaHelper.SwapEndian(header.Version);
                _info.DataOffset = HcaHelper.SwapEndian(header.DataOffset);
            } else {
                return false;
            }
            // FMT
            v = stream.PeekUInt32();
            if (MagicValues.IsMagicMatch(v, MagicValues.FMT)) {
                FormatHeader header;
                stream.Read(out header);
                _info.ChannelCount = header.Channels;
                _info.SamplingRate = HcaHelper.SwapEndian(header.SamplingRate << 8);
                _info.BlockCount = HcaHelper.SwapEndian(header.Blocks);
                _info.FmtR01 = HcaHelper.SwapEndian(header.R01);
                _info.FmtR02 = HcaHelper.SwapEndian(header.R02);
                if (_info.ChannelCount < 1 || _info.ChannelCount > 16) {
                    return false;
                }
                if (_info.SamplingRate < 1 || _info.SamplingRate > 0x7fffff) {
                    return false;
                }
            } else {
                return false;
            }
            // COMP / DEC
            v = stream.PeekUInt32();
            if (MagicValues.IsMagicMatch(v, MagicValues.COMP)) {
                CompressHeader header;
                stream.Read(out header);
                _info.BlockSize = HcaHelper.SwapEndian(header.BlockSize);
                _info.CompR01 = header.R01;
                _info.CompR02 = header.R02;
                _info.CompR03 = header.R03;
                _info.CompR04 = header.R04;
                _info.CompR05 = header.R05;
                _info.CompR06 = header.R06;
                _info.CompR07 = header.R07;
                _info.CompR08 = header.R08;
                if ((_info.BlockSize < 8 || _info.BlockSize > 0xffff) && _info.BlockSize != 0) {
                    return false;
                }
                if (!(_info.CompR01 <= _info.CompR02 && _info.CompR02 <= 0x1f)) {
                    return false;
                }
            } else if (MagicValues.IsMagicMatch(v, MagicValues.DEC)) {
                DecodeHeader header;
                stream.Read(out header);
                _info.CompR01 = header.R01;
                _info.CompR02 = header.R02;
                _info.CompR03 = header.R04;
                _info.CompR04 = header.R03;
                _info.CompR05 = (uint)(header.Count1 + 1);
                _info.CompR06 = (uint)(header.EnableCount2 ? header.Count2 : header.Count1) + 1;
                _info.CompR07 = _info.CompR05 - _info.CompR06;
                _info.CompR08 = 0;
                if ((_info.BlockSize < 8 || _info.BlockSize > 0xffff) && _info.BlockSize != 0) {
                    return false;
                }
                if (!(_info.CompR01 <= _info.CompR02 && _info.CompR02 <= 0x1f)) {
                    return false;
                }
                if (_info.CompR03 == 0) {
                    _info.CompR03 = 1;
                }
            } else {
                return false;
            }
            // VBR
            v = stream.PeekUInt32();
            if (MagicValues.IsMagicMatch(v, MagicValues.VBR)) {
                VbrHeader header;
                stream.Read(out header);
                _info.VbrR01 = HcaHelper.SwapEndian(header.R01);
                _info.VbrR02 = HcaHelper.SwapEndian(header.R02);
                if (!(_info.BlockSize == 0 && _info.VbrR01 < 0x01ff)) {
                    return false;
                }
            } else {
                _info.VbrR01 = _info.VbrR02 = 0;
            }
            // ATH
            v = stream.PeekUInt32();
            if (MagicValues.IsMagicMatch(v, MagicValues.ATH)) {
                AthHeader header;
                stream.Read(out header);
                _info.AthType = header.Type;
            } else {
                _info.AthType = (uint)(_info.Version < 0x0200 ? 1 : 0);
            }
            // LOOP
            v = stream.PeekUInt32();
            if (MagicValues.IsMagicMatch(v, MagicValues.LOOP)) {
                LoopHeader header;
                stream.Read(out header);
                _info.LoopStart = HcaHelper.SwapEndian(header.LoopStart);
                _info.LoopEnd = HcaHelper.SwapEndian(header.LoopEnd);
                _info.LoopR01 = HcaHelper.SwapEndian(header.R01);
                _info.LoopR02 = HcaHelper.SwapEndian(header.R02);
                _info.LoopFlag = true;
            } else {
                _info.LoopStart = 0;
                _info.LoopEnd = 0;
                _info.LoopR01 = 0;
                _info.LoopR02 = 0x400;
                _info.LoopFlag = false;
            }
            // CIPH
            v = stream.PeekUInt32();
            if (MagicValues.IsMagicMatch(v, MagicValues.CIPH)) {
                CipherHeader header;
                stream.Read(out header);
                _info.CiphType = (CipherType)HcaHelper.SwapEndian(header.Type);
            } else {
                _info.CiphType = CipherType.NoChipher;
            }
            // RVA
            v = stream.PeekUInt32();
            if (MagicValues.IsMagicMatch(v, MagicValues.RVA)) {
                RvaHeader header;
                stream.Read(out header);
                _info.RvaVolume = HcaHelper.SwapEndian(header.Volume);
            } else {
                _info.RvaVolume = 1;
            }
            // COMM
            v = stream.PeekUInt32();
            if (MagicValues.IsMagicMatch(v, MagicValues.COMM)) {
                CommentHeader header;
                stream.Read(out header);
                _info.CommentLength = header.Length;
                var tmpCommentCharList = new List<byte>();
                byte tmpByte;
                do {
                    tmpByte = (byte)stream.ReadByte();
                    tmpCommentCharList.Add(tmpByte);
                } while (tmpByte != 0);
                _info.Comment = tmpCommentCharList.ToArray();
            } else {
                _info.CommentLength = 0;
                _info.Comment = null;
            }
            if (!_ath.Initialize(_info.AthType, _info.SamplingRate)) {
                return false;
            }
            if (!_cipher.Initialize(_info.CiphType, _decodeParam.Key1, _decodeParam.Key2)) {
                return false;
            }
            if (_info.CompR03 == 0) {
                _info.CompR03 = 1;
            }

            _channels = new Channel[0x10];
            for (var i = 0; i < _channels.Length; ++i) {
                _channels[i] = Channel.CreateDefault();
            }
            if (_info.CompR01 != 1 || _info.CompR02 != 0xf) {
                return false;
            }
            _info.CompR09 = HcaHelper.Ceil2(_info.CompR05 - (_info.CompR06 + _info.CompR07), _info.CompR08);
            var r = new byte[10];
            uint b = _info.ChannelCount / _info.CompR03;
            if (_info.CompR07 != 0 && b > 1) {
                uint rIndex = 0;
                for (uint i = 0; i < _info.CompR03; ++i, rIndex += b) {
                    switch (b) {
                        case 2:
                        case 3:
                            r[rIndex] = 1;
                            r[rIndex + 1] = 2;
                            break;
                        case 4:
                            r[rIndex] = 1;
                            r[rIndex + 1] = 2;
                            if (_info.CompR04 == 0) {
                                r[rIndex + 2] = 1;
                                r[rIndex + 3] = 2;
                            }
                            break;
                        case 5:
                            r[rIndex] = 1;
                            r[rIndex + 1] = 2;
                            if (_info.CompR04 <= 2) {
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
            for (uint i = 0; i < _info.ChannelCount; ++i) {
                _channels[i].Type = r[i];
                _channels[i].Value3 = _info.CompR06 + _info.CompR07;
                _channels[i].Count = _info.CompR06 + (r[i] != 2 ? _info.CompR07 : 0);
            }

            _lengthInSamples = (int)_info.BlockCount * (int)_info.ChannelCount * 0x80 * 8;
            _lengthInSecs = _lengthInSamples / (float)_info.SamplingRate;

            return true;
        }

    }
}
