using System;
using System.IO;

namespace DereTore.Exchange.Audio.HCA {
    partial class HcaDecoder {

        private void Initialize() {
            ParseHeaders();
            InitializeDecodeComponents();
        }

        private void InitializeDecodeComponents() {
            var hcaInfo = HcaInfo;
            if (!_ath.Initialize(hcaInfo.AthType, hcaInfo.SamplingRate)) {
                throw new HcaException(ErrorMessages.GetAthInitializationFailed(), ActionResult.AthInitFailed);
            }
            var decodeParams = _decodeParams;
            var cipherType = decodeParams.CipherTypeOverrideEnabled ? decodeParams.OverriddenCipherType : hcaInfo.CipherType;
            if (!_cipher.Initialize(cipherType, decodeParams.Key1, decodeParams.Key2, decodeParams.KeyModifier)) {
                throw new HcaException(ErrorMessages.GetCiphInitializationFailed(), ActionResult.CiphInitFailed);
            }

            var channels = _channels = new ChannelArray(0x10);
            var r = new byte[10];
            uint b = hcaInfo.ChannelCount / hcaInfo.CompR03;

            if (hcaInfo.CompR07 != 0 && b > 1) {
                uint rIndex = 0;
                for (uint i = 0; i < hcaInfo.CompR03; ++i, rIndex += b) {
                    switch (b) {
                        case 2:
                        case 3:
                            r[rIndex] = 1;
                            r[rIndex + 1] = 2;
                            break;
                        case 4:
                            r[rIndex] = 1;
                            r[rIndex + 1] = 2;
                            if (hcaInfo.CompR04 == 0) {
                                r[rIndex + 2] = 1;
                                r[rIndex + 3] = 2;
                            }
                            break;
                        case 5:
                            r[rIndex] = 1;
                            r[rIndex + 1] = 2;
                            if (hcaInfo.CompR04 <= 2) {
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

            unsafe {
                for (var i = 0; i < hcaInfo.ChannelCount; ++i) {
                    var pType = channels.GetPtrOfType(i);
                    var pValue = channels.GetPtrOfValue(i);
                    var ppValue3 = channels.GetPtrOfValue3(i);
                    var pCount = channels.GetPtrOfCount(i);

                    *pType = r[i];
                    *ppValue3 = &pValue[hcaInfo.CompR06 + hcaInfo.CompR07];
                    *pCount = (uint)(hcaInfo.CompR06 + (r[i] != 2 ? hcaInfo.CompR07 : 0));
                }
            }
        }

        private int DecodeToWaveR32(byte[] blockData, int blockIndex) {
            var hcaInfo = HcaInfo;
            if (blockData == null) {
                throw new ArgumentNullException(nameof(blockData));
            }
            if (blockData.Length < hcaInfo.BlockSize) {
                throw new HcaException(ErrorMessages.GetInvalidParameter(nameof(blockData) + "." + nameof(blockData.Length)), ActionResult.InvalidParameter);
            }
            var checksum = HcaHelper.Checksum(blockData, 0);
            if (checksum != 0) {
                throw new HcaException(ErrorMessages.GetChecksumNotMatch(0, checksum), ActionResult.ChecksumNotMatch);
            }
            _cipher.Decrypt(blockData);
            var d = new DataBits(blockData, hcaInfo.BlockSize);
            var magic = d.GetBit(16);
            if (magic != 0xffff) {
                throw new HcaException(ErrorMessages.GetMagicNotMatch(0xffff, magic), ActionResult.MagicNotMatch);
            }
            var a = (d.GetBit(9) << 8) - d.GetBit(7);
            var channels = _channels;
            var ath = _ath;
            string site = null;

            try {
                int i;

                for (i = 0; i < hcaInfo.ChannelCount; ++i) {
                    site = $"Decode1({i.ToString()})";
                    channels.Decode1(i, d, hcaInfo.CompR09, a, ath.Table);
                }

                for (i = 0; i < 8; ++i) {
                    for (var j = 0; j < hcaInfo.ChannelCount; ++j) {
                        site = $"Decode2({i.ToString()}/{j.ToString()})";
                        channels.Decode2(j, d);
                    }

                    for (var j = 0; j < hcaInfo.ChannelCount; ++j) {
                        site = $"Decode3({i.ToString()}/{j.ToString()})";
                        channels.Decode3(j, hcaInfo.CompR09, hcaInfo.CompR08, (uint)(hcaInfo.CompR07 + hcaInfo.CompR06), hcaInfo.CompR05);
                    }

                    for (var j = 0; j < hcaInfo.ChannelCount - 1; ++j) {
                        site = $"Decode4({i.ToString()}/{j.ToString()})";
                        channels.Decode4(j, j + 1, i, (uint)(hcaInfo.CompR05 - hcaInfo.CompR06), hcaInfo.CompR06, hcaInfo.CompR07);
                    }

                    for (var j = 0; j < hcaInfo.ChannelCount; ++j) {
                        site = $"Decode5({i.ToString()}/{j.ToString()})";
                        channels.Decode5(j, i);
                    }
                }

                return blockData.Length;
            } catch (IndexOutOfRangeException ex) {
                const string message = "Index access exception detected. It is probably because you are using an incorrect HCA key pair while decoding a type 56 HCA file.";
                var siteInfo = $"Site: {site} @ block {blockIndex.ToString()}";
                var err = message + Environment.NewLine + siteInfo;
                throw new HcaException(err, ActionResult.DecodeFailed, ex);
            }
        }

        private void TransformWaveDataBlocks(Stream source, byte[] destination, uint startBlockIndex, uint blockCount, IWaveWriter waveWriter) {
            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            }
            if (destination == null) {
                throw new ArgumentNullException(nameof(destination));
            }
            if (waveWriter == null) {
                throw new ArgumentNullException(nameof(waveWriter));
            }
            var hcaInfo = HcaInfo;
            var startOffset = hcaInfo.DataOffset + startBlockIndex * hcaInfo.BlockSize;
            source.Seek(startOffset, SeekOrigin.Begin);
            var channels = _channels;
            var decodeParams = _decodeParams;
            var hcaBlockBuffer = GetHcaBlockBuffer();

            var channelCount = hcaInfo.ChannelCount;
            var rvaVolume = hcaInfo.RvaVolume;
            var bytesPerSample = waveWriter.BytesPerSample;
            var volume = decodeParams.Volume;

            for (var l = 0; l < (int)blockCount; ++l) {
                source.Read(hcaBlockBuffer, 0, hcaBlockBuffer.Length);

                DecodeToWaveR32(hcaBlockBuffer, l + (int)startBlockIndex);

                for (var i = 0; i < 8; ++i) {
                    for (var j = 0; j < 0x80; ++j) {
                        for (var k = 0; k < channelCount; ++k) {
                            float f;

                            unsafe {
                                var pWave = channels.GetPtrOfWave(k);

                                f = pWave[i * 0x80 + j];
                                f = f * volume * rvaVolume;
                            }

                            HcaHelper.Clamp(ref f, -1f, 1f);

                            var offset = (((l * 8 + i) * 0x80 + j) * channelCount + k) * bytesPerSample;

                            waveWriter.DecodeToBuffer(f, destination, (uint)offset);
                        }
                    }
                }
            }
        }

        private byte[] GetHcaBlockBuffer() {
            return _hcaBlockBuffer ?? (_hcaBlockBuffer = new byte[HcaInfo.BlockSize]);
        }

        private int GetSampleBitsFromParams() {
            var mode = _decodeParams.Mode;
            switch (mode) {
                case SamplingMode.R32:
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

        private IWaveWriter GetProperWaveWriter() {
            var mode = _decodeParams.Mode;
            switch (mode) {
                case SamplingMode.S16:
                    return WaveHelper.S16;
                case SamplingMode.R32:
                    return WaveHelper.R32;
                case SamplingMode.S32:
                    return WaveHelper.S32;
                case SamplingMode.U8:
                    return WaveHelper.U8;
                case SamplingMode.S24:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }

        private byte[] _hcaBlockBuffer;
        private readonly Ath _ath;
        private readonly Cipher _cipher;
        private ChannelArray _channels;
        private readonly DecodeParams _decodeParams;
        private int? _minWaveHeaderBufferSize;
        private int? _minWaveDataBufferSize;

    }
}
