using System;
using System.IO;
using System.Runtime.InteropServices;
using DereTore.HCA.Interop;

namespace DereTore.HCA {
    public partial class HcaDecoder : HcaReader {

        public HcaDecoder(Stream sourceStream)
          : this(sourceStream, DecodeParams.Default) {
        }

        public HcaDecoder(Stream sourceStream, DecodeParams decodeParams)
            : base(sourceStream) {
            _decodeParams = decodeParams.Clone();
            HcaHelper.TranslateTables();
            _hcaInfo = new HcaInfo {
                CiphKey1 = decodeParams.Key1,
                CiphKey2 = decodeParams.Key2
            };
            _ath = new Ath();
            _cipher = new Cipher();
            Initialize();
        }

        public static bool IsHcaStream(Stream stream) {
            var position = stream.Position;
            bool result;
            try {
                // The cipher keys do nothing to validating HCA files.
                var decoder = new HcaDecoder(stream, DecodeParams.Default);
                result = true;
            } catch (HcaException) {
                result = false;
            }
            stream.Seek(position, SeekOrigin.Begin);
            return result;
        }

        public int GetMinWaveHeaderBufferSize() {
            if (_minWaveHeaderBufferSize != null) {
                return _minWaveHeaderBufferSize.Value;
            }
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
            _minWaveHeaderBufferSize = sizeNeeded;
            return _minWaveHeaderBufferSize.Value;
        }

        public int GetMinWaveDataBufferSize() {
            if (_minWaveDataBufferSize != null) {
                return _minWaveDataBufferSize.Value;
            }
            _minWaveDataBufferSize = 0x80 * GetSampleBitsFromParams() * (int)_hcaInfo.ChannelCount;
            return _minWaveDataBufferSize.Value;
        }

        public int WriteWaveHeader(byte[] stream) {
            if (stream == null) {
                throw new ArgumentNullException(nameof(stream));
            }
            var minimumHeaderBufferSize = GetMinWaveHeaderBufferSize();
            if (stream.Length < minimumHeaderBufferSize) {
                throw new HcaException(ErrorMessages.GetBufferTooSmall(minimumHeaderBufferSize, stream.Length), ActionResult.BufferTooSmall);
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

    }
}