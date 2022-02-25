using System;
using System.IO;
using System.Runtime.InteropServices;
using DereTore.Common;
using DereTore.Exchange.Audio.HCA.Native;

namespace DereTore.Exchange.Audio.HCA {
    public partial class HcaDecoder : HcaReader, IDisposable {

        public HcaDecoder(Stream sourceStream)
          : this(sourceStream, DecodeParams.Default) {
        }

        public HcaDecoder(Stream sourceStream, DecodeParams decodeParams)
            : base(sourceStream) {
            _decodeParams = decodeParams;
            HcaHelper.TranslateTables();
            _ath = new Ath();
            _cipher = new Cipher();
            Initialize();
        }

        public int GetMinWaveHeaderBufferSize() {
            if (_minWaveHeaderBufferSize != null) {
                return _minWaveHeaderBufferSize.Value;
            }
            var wavNoteSize = 0;
            var hcaInfo = HcaInfo;
            if (hcaInfo.Comment != null) {
                wavNoteSize = 4 + (int)hcaInfo.CommentLength + 1;
                if ((wavNoteSize & 3) != 0) {
                    wavNoteSize += 4 - (wavNoteSize & 3);
                }
            }
            var sizeNeeded = Marshal.SizeOf(typeof(WaveRiffSection));
            if (hcaInfo.LoopFlag) {
                // FIXME currently it's unknown what wavSmpl section means and how its values should be set.
                // Maybe it's just ignored by common media players.
                sizeNeeded += Marshal.SizeOf(typeof(WaveSampleSection));
            }
            if (hcaInfo.Comment != null && hcaInfo.Comment.Length > 0) {
                sizeNeeded += 8 * wavNoteSize;
            }
            sizeNeeded += Marshal.SizeOf(typeof(WaveDataSection));
            _minWaveHeaderBufferSize = sizeNeeded;
            return _minWaveHeaderBufferSize.Value;
        }

        public int GetMinWaveDataBufferSize() {
            if (_minWaveDataBufferSize != null) {
                return _minWaveDataBufferSize.Value;
            }
            _minWaveDataBufferSize = 0x80 * GetSampleBitsFromParams() * (int)HcaInfo.ChannelCount;
            return _minWaveDataBufferSize.Value;
        }

        public int WriteWaveHeader(byte[] stream) {
            return WriteWaveHeader(stream, AudioParams.Default);
        }

        public int WriteWaveHeader(byte[] stream, AudioParams audioParams) {
            if (stream == null) {
                throw new ArgumentNullException(nameof(stream));
            }
            var hcaInfo = HcaInfo;
            if (hcaInfo.LoopFlag && audioParams.InfiniteLoop) {
                // See remarks of AudioParams.InfiniteLoop.
                throw new HcaException(ErrorMessages.GetInvalidParameter(nameof(audioParams.InfiniteLoop)), ActionResult.InvalidParameter);
            }
            var minimumHeaderBufferSize = GetMinWaveHeaderBufferSize();
            if (stream.Length < minimumHeaderBufferSize) {
                throw new HcaException(ErrorMessages.GetBufferTooSmall(minimumHeaderBufferSize, stream.Length), ActionResult.BufferTooSmall);
            }
            var sampleBits = GetSampleBitsFromParams();
            var wavRiff = WaveRiffSection.CreateDefault();
            var wavNote = WaveNoteSection.CreateDefault();
            var wavSmpl = WaveSampleSection.CreateDefault();
            var wavData = WaveDataSection.CreateDefault();
            wavRiff.FmtType = (ushort)(_decodeParams.Mode != SamplingMode.R32 ? 1 : 3);
            wavRiff.FmtChannels = (ushort)hcaInfo.ChannelCount;
            wavRiff.FmtBitCount = (ushort)(sampleBits > 0 ? sampleBits : sizeof(float));
            wavRiff.FmtSamplingRate = hcaInfo.SamplingRate;
            wavRiff.FmtSamplingSize = (ushort)(wavRiff.FmtBitCount / 8 * wavRiff.FmtChannels);
            wavRiff.FmtSamplesPerSec = wavRiff.FmtSamplingRate * wavRiff.FmtSamplingSize;
            if (hcaInfo.Comment != null) {
                wavNote.NoteSize = 4 + hcaInfo.CommentLength + 1;
                if ((wavNote.NoteSize & 3) != 0) {
                    wavNote.NoteSize += 4 - (wavNote.NoteSize & 3);
                }
            }
            if (hcaInfo.LoopFlag)
            {
                // FIXME I see "※計算方法不明" here:
                // https://github.com/Nyagamon/HCADecoder/blob/e26b4d3a8bb450224ede3527d522c0330f7bf02b/clHCA.cpp#L556
                // so ... I don't know how to handle this either.
                // However this "smpl" header section seems to be unrecognized so it shouldn't matter.
                wavSmpl.SamplePeriod = (1 / hcaInfo.SamplingRate * 1000000000);
                wavSmpl.LoopStart = hcaInfo.LoopStart * 0x80 * 8 + hcaInfo.FmtR01;
                wavSmpl.LoopEnd = (hcaInfo.LoopEnd + 1) * 0x80 * 8 - 1;
                // LoopR01 should be "loop count", when it equals to 0x80 it means infinite loop
                wavSmpl.LoopPlayCount = (hcaInfo.LoopR01 == 0x80) ? (ushort) 0 : hcaInfo.LoopR01;
            }

            var totalBlockCount = hcaInfo.BlockCount;
            if (hcaInfo.LoopFlag) {
                totalBlockCount += (hcaInfo.LoopEnd - hcaInfo.LoopStart) * audioParams.SimulatedLoopCount;
            }
            wavData.DataSize = totalBlockCount * 0x80 * 8 * wavRiff.FmtSamplingSize;
            wavRiff.RiffSize = (uint)(
                0x1c
                + (hcaInfo.Comment != null ? wavNote.NoteSize : 0)
                + (hcaInfo.LoopFlag ? Marshal.SizeOf(typeof(WaveSampleSection)) : 0)
                + Marshal.SizeOf(wavData) + wavData.DataSize
            );

            var bytesWritten = stream.Write(wavRiff, 0);
            if (hcaInfo.Comment != null) {
                var address = bytesWritten;
                bytesWritten += stream.Write(wavNote, bytesWritten);
                stream.Write(hcaInfo.Comment, bytesWritten);
                bytesWritten = address + 8 + (int)wavNote.NoteSize;
                bytesWritten += 8 + (int)wavNote.NoteSize;
            }
            if (hcaInfo.LoopFlag)
            {
                bytesWritten += stream.Write(wavSmpl, bytesWritten);
            }
            bytesWritten += stream.Write(wavData, bytesWritten);
            return bytesWritten;
        }

        public void Dispose() {
            foreach (var c in snapshots) c.Value.Dispose();
            snapshots.Clear();
            _channels?.Dispose();
        }

        public void TakeSnapshot(string name) {
            if (snapshots.ContainsKey(name)) throw new ArgumentOutOfRangeException(nameof(name));
            var cloned = _channels.clone();
            snapshots.Add(name, cloned);
        }

        public void RewindToSnapshot(string name) {
            if (!snapshots.ContainsKey(name)) throw new ArgumentOutOfRangeException(nameof(name));
            if (_channels != null) _channels.Dispose();
            _channels = snapshots[name].clone();
        }

        public void DeleteSnapshot(string name) {
            if (!snapshots.ContainsKey(name)) throw new ArgumentOutOfRangeException(nameof(name));
            snapshots[name].Dispose();
            snapshots.Remove(name);
        }

    }
}
