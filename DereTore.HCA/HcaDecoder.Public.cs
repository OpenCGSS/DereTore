using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DereTore.HCA.Interop;

namespace DereTore.HCA {
    public partial class HcaDecoder {

        public int GetWaveHeaderNeededLength() {
            var wavNoteSize = 0;
            if (_hcaInfo.Comment != null) {
                wavNoteSize = 4 + (int)_hcaInfo.CommentLength + 1;
                if ((wavNoteSize & 3) != 0) {
                    wavNoteSize += 4 - (wavNoteSize & 3);
                }
            }
            var sizeNeeded = Marshal.SizeOf(typeof(WaveRiffSection));
            if (_hcaInfo.LoopFlag) {
                sizeNeeded += Marshal.SizeOf(typeof(WaveSampleSection));
            }
            if (_hcaInfo.Comment != null && _hcaInfo.Comment.Length > 0) {
                sizeNeeded += 8 * wavNoteSize;
            }
            sizeNeeded += Marshal.SizeOf(typeof(WaveDataSection));
            return sizeNeeded;
        }

        public int GetWaveDataBlockNeededLength() {
            return 0x80 * GetSampleBitsFromParams() * (int)_hcaInfo.ChannelCount;
        }

        public int WriteWaveHeader(byte[] stream) {
            if (stream == null) {
                throw new ArgumentNullException("stream");
            }
            var minimumSize = GetWaveHeaderNeededLength();
            try {
                if (stream.Length < minimumSize) {
                    throw new ArgumentException(string.Format("Buffer length is not enough. Expected minum: {0}, received: {1}", minimumSize, stream.Length));
                }
            } catch (InvalidOperationException ex) {
                Debug.WriteLine(ex.Message);
            }
            var sampleBits = GetSampleBitsFromParams();
            var loopCount = _decodeParams.Loop;
            var wavRiff = WaveRiffSection.CreateDefault();
            var wavSmpl = WaveSampleSection.CreateDefault();
            var wavNote = WaveNoteSection.CreateDefault();
            var wavData = WaveDataSection.CreateDefault();
            wavRiff.FmtType = (ushort)(_decodeParams.Mode != SamplingMode.Float ? 1 : 3);
            wavRiff.FmtChannels = (ushort)_hcaInfo.ChannelCount;
            wavRiff.FmtBitCount = (ushort)(sampleBits > 0 ? sampleBits : sizeof(float));
            wavRiff.FmtSamplingRate = _hcaInfo.SamplingRate;
            wavRiff.FmtSamplingSize = (ushort)(wavRiff.FmtBitCount / 8 * wavRiff.FmtChannels);
            wavRiff.FmtSamplesPerSec = wavRiff.FmtSamplingRate * wavRiff.FmtSamplingSize;
            if (_hcaInfo.LoopFlag) {
                wavSmpl.SamplePeriod = (uint)(1000000000 / (double)wavRiff.FmtSamplingRate);
                wavSmpl.LoopStart = _hcaInfo.LoopStart * 0x80 * 8 * wavRiff.FmtSamplingSize;
                wavSmpl.LoopEnd = _hcaInfo.LoopR01 == 0x80 ? 0 : _hcaInfo.LoopR01;
            } else if (_decodeParams.EnableLoop) {
                wavSmpl.LoopStart = 0;
                wavSmpl.LoopEnd = _hcaInfo.BlockCount * 0x80 * 8 * wavRiff.FmtSamplingSize;
                _hcaInfo.LoopStart = 0;
                _hcaInfo.LoopEnd = _hcaInfo.BlockCount;
            }
            if (_hcaInfo.Comment != null) {
                wavNote.NoteSize = 4 + _hcaInfo.CommentLength + 1;
                if ((wavNote.NoteSize & 3) != 0) {
                    wavNote.NoteSize += 4 - (wavNote.NoteSize & 3);
                }
            }
            wavData.DataSize = (uint)(_hcaInfo.BlockCount * 0x80 * 8 * wavRiff.FmtSamplingSize + (wavSmpl.LoopEnd - wavSmpl.LoopStart) * loopCount);
            wavRiff.RiffSize = (uint)(0x1c + ((_hcaInfo.LoopFlag && !_decodeParams.EnableLoop) ? Marshal.SizeOf(wavSmpl) : 0) + (_hcaInfo.Comment != null ? wavNote.NoteSize : 0) + Marshal.SizeOf(wavData) + wavData.DataSize);

            var bytesWritten = stream.Write(wavRiff, 0);
            if (_hcaInfo.LoopFlag && !_decodeParams.EnableLoop) {
                bytesWritten += stream.Write(wavSmpl, bytesWritten);
            }
            if (_hcaInfo.Comment != null) {
                var address = bytesWritten;
                bytesWritten += stream.Write(wavNote, bytesWritten);
                stream.Write(_hcaInfo.Comment, bytesWritten);
                bytesWritten = address + 8 + (int)wavNote.NoteSize;
                bytesWritten += 8 + (int)wavNote.NoteSize;
            }
            bytesWritten += stream.Write(wavData, bytesWritten);
            return bytesWritten;
        }

        public int DecodeData(byte[] buffer, out bool hasMore) {
            if (_status.DataCursor < _hcaInfo.DataOffset) {
                _status.DataCursor = _hcaInfo.DataOffset;
            }
            uint waveBlockSize = 0x80 * (uint)GetSampleBitsFromParams() * _hcaInfo.ChannelCount;
            uint blockProcessableThisRound = (uint)buffer.Length / waveBlockSize;
            if (!_decodeParams.EnableLoop && !_hcaInfo.LoopFlag) {
                int bufferCursor;
                if (_status.BlockIndex + blockProcessableThisRound >= _hcaInfo.BlockCount) {
                    blockProcessableThisRound = _hcaInfo.BlockCount - _status.BlockIndex;
                    hasMore = false;
                } else {
                    hasMore = true;
                }
                var streamBuffer = GetHcaBlockBuffer();
                if (!GenerateWaveData(SourceStream, buffer, blockProcessableThisRound, streamBuffer, GetDecodeToBufferFunc(), out bufferCursor)) {
                    throw new HcaException();
                }
                _status.BlockIndex += blockProcessableThisRound;
                return bufferCursor;
            } else {
                throw new NotImplementedException();
            }
        }

        public bool InitializeDecodeComponents() {
            if (!_ath.Initialize(_hcaInfo.AthType, _hcaInfo.SamplingRate)) {
                return false;
            }
            if (!_cipher.Initialize(_hcaInfo.CiphType, _decodeParams.Key1, _decodeParams.Key2)) {
                return false;
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
            return true;
        }

    }
}