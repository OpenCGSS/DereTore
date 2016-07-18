using System.Collections.Generic;
using System.IO;
using DereTore.HCA.Interop;

namespace DereTore.HCA {
    public abstract class HcaReader {

        public Stream SourceStream {
            get { return _sourceStream; }
        }

        public HcaInfo HcaInfo {
            get { return _hcaInfo; }
        }

        public float LengthInSeconds {
            get { return _lengthInSeconds; }
        }

        public int LengthInSamples {
            get { return _lengthInSamples; }
        }

        public bool ParseHeaders() {
            var stream = SourceStream;
            uint v;
            // HCA
            v = stream.PeekUInt32();
            if (MagicValues.IsMagicMatch(v, MagicValues.HCA)) {
                HcaHeader header;
                stream.Read(out header);
                _hcaInfo.Version = HcaHelper.SwapEndian(header.Version);
                _hcaInfo.DataOffset = HcaHelper.SwapEndian(header.DataOffset);
            } else {
                return false;
            }
            // FMT
            v = stream.PeekUInt32();
            if (MagicValues.IsMagicMatch(v, MagicValues.FMT)) {
                FormatHeader header;
                stream.Read(out header);
                _hcaInfo.ChannelCount = header.Channels;
                _hcaInfo.SamplingRate = HcaHelper.SwapEndian(header.SamplingRate << 8);
                _hcaInfo.BlockCount = HcaHelper.SwapEndian(header.Blocks);
                _hcaInfo.FmtR01 = HcaHelper.SwapEndian(header.R01);
                _hcaInfo.FmtR02 = HcaHelper.SwapEndian(header.R02);
                if (_hcaInfo.ChannelCount < 1 || _hcaInfo.ChannelCount > 16) {
                    return false;
                }
                if (_hcaInfo.SamplingRate < 1 || _hcaInfo.SamplingRate > 0x7fffff) {
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
                _hcaInfo.BlockSize = HcaHelper.SwapEndian(header.BlockSize);
                _hcaInfo.CompR01 = header.R01;
                _hcaInfo.CompR02 = header.R02;
                _hcaInfo.CompR03 = header.R03;
                _hcaInfo.CompR04 = header.R04;
                _hcaInfo.CompR05 = header.R05;
                _hcaInfo.CompR06 = header.R06;
                _hcaInfo.CompR07 = header.R07;
                _hcaInfo.CompR08 = header.R08;
                if ((_hcaInfo.BlockSize < 8 || _hcaInfo.BlockSize > 0xffff) && _hcaInfo.BlockSize != 0) {
                    return false;
                }
                if (!(_hcaInfo.CompR01 <= _hcaInfo.CompR02 && _hcaInfo.CompR02 <= 0x1f)) {
                    return false;
                }
            } else if (MagicValues.IsMagicMatch(v, MagicValues.DEC)) {
                DecodeHeader header;
                stream.Read(out header);
                _hcaInfo.CompR01 = header.R01;
                _hcaInfo.CompR02 = header.R02;
                _hcaInfo.CompR03 = header.R04;
                _hcaInfo.CompR04 = header.R03;
                _hcaInfo.CompR05 = (uint)(header.Count1 + 1);
                _hcaInfo.CompR06 = (uint)(header.EnableCount2 ? header.Count2 : header.Count1) + 1;
                _hcaInfo.CompR07 = _hcaInfo.CompR05 - _hcaInfo.CompR06;
                _hcaInfo.CompR08 = 0;
                if ((_hcaInfo.BlockSize < 8 || _hcaInfo.BlockSize > 0xffff) && _hcaInfo.BlockSize != 0) {
                    return false;
                }
                if (!(_hcaInfo.CompR01 <= _hcaInfo.CompR02 && _hcaInfo.CompR02 <= 0x1f)) {
                    return false;
                }
                if (_hcaInfo.CompR03 == 0) {
                    _hcaInfo.CompR03 = 1;
                }
            } else {
                return false;
            }
            // VBR
            v = stream.PeekUInt32();
            if (MagicValues.IsMagicMatch(v, MagicValues.VBR)) {
                VbrHeader header;
                stream.Read(out header);
                _hcaInfo.VbrR01 = HcaHelper.SwapEndian(header.R01);
                _hcaInfo.VbrR02 = HcaHelper.SwapEndian(header.R02);
                if (!(_hcaInfo.BlockSize == 0 && _hcaInfo.VbrR01 < 0x01ff)) {
                    return false;
                }
            } else {
                _hcaInfo.VbrR01 = _hcaInfo.VbrR02 = 0;
            }
            // ATH
            v = stream.PeekUInt32();
            if (MagicValues.IsMagicMatch(v, MagicValues.ATH)) {
                AthHeader header;
                stream.Read(out header);
                _hcaInfo.AthType = header.Type;
            } else {
                _hcaInfo.AthType = (uint)(_hcaInfo.Version < 0x0200 ? 1 : 0);
            }
            // LOOP
            v = stream.PeekUInt32();
            if (MagicValues.IsMagicMatch(v, MagicValues.LOOP)) {
                LoopHeader header;
                stream.Read(out header);
                _hcaInfo.LoopStart = HcaHelper.SwapEndian(header.LoopStart);
                _hcaInfo.LoopEnd = HcaHelper.SwapEndian(header.LoopEnd);
                _hcaInfo.LoopR01 = HcaHelper.SwapEndian(header.R01);
                _hcaInfo.LoopR02 = HcaHelper.SwapEndian(header.R02);
                _hcaInfo.LoopFlag = true;
            } else {
                _hcaInfo.LoopStart = 0;
                _hcaInfo.LoopEnd = 0;
                _hcaInfo.LoopR01 = 0;
                _hcaInfo.LoopR02 = 0x400;
                _hcaInfo.LoopFlag = false;
            }
            // CIPH
            v = stream.PeekUInt32();
            if (MagicValues.IsMagicMatch(v, MagicValues.CIPH)) {
                CipherHeader header;
                stream.Read(out header);
                _hcaInfo.CiphType = (CipherType)HcaHelper.SwapEndian(header.Type);
            } else {
                _hcaInfo.CiphType = CipherType.NoChipher;
            }
            // RVA
            v = stream.PeekUInt32();
            if (MagicValues.IsMagicMatch(v, MagicValues.RVA)) {
                RvaHeader header;
                stream.Read(out header);
                _hcaInfo.RvaVolume = HcaHelper.SwapEndian(header.Volume);
            } else {
                _hcaInfo.RvaVolume = 1;
            }
            // COMM
            v = stream.PeekUInt32();
            if (MagicValues.IsMagicMatch(v, MagicValues.COMM)) {
                CommentHeader header;
                stream.Read(out header);
                _hcaInfo.CommentLength = header.Length;
                var tmpCommentCharList = new List<byte>();
                byte tmpByte;
                do {
                    tmpByte = (byte)stream.ReadByte();
                    tmpCommentCharList.Add(tmpByte);
                } while (tmpByte != 0);
                _hcaInfo.Comment = tmpCommentCharList.ToArray();
            } else {
                _hcaInfo.CommentLength = 0;
                _hcaInfo.Comment = null;
            }

            if (_hcaInfo.CompR03 == 0) {
                _hcaInfo.CompR03 = 1;
            }

            if (_hcaInfo.CompR01 != 1 || _hcaInfo.CompR02 != 0xf) {
                return false;
            }
            _hcaInfo.CompR09 = HcaHelper.Ceil2(_hcaInfo.CompR05 - (_hcaInfo.CompR06 + _hcaInfo.CompR07), _hcaInfo.CompR08);

            _lengthInSamples = (int)_hcaInfo.BlockCount * (int)_hcaInfo.ChannelCount * 0x80 * 8;
            _lengthInSeconds = _lengthInSamples / (float)_hcaInfo.SamplingRate;

            return true;
        }

        protected HcaReader(Stream sourceStream) {
            _sourceStream = sourceStream;
        }

        protected HcaInfo _hcaInfo;
        private readonly Stream _sourceStream;
        private float _lengthInSeconds;
        private int _lengthInSamples;

    }
}