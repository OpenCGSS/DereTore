using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DereTore.HCA.Interop;

namespace DereTore.HCA {
    public partial class HcaDecoder {

        public int GetWaveHeaderNeededLength() {
            var wavNoteSize = 0;
            if (_info.Comment != null) {
                wavNoteSize = 4 + (int)_info.CommentLength + 1;
                if ((wavNoteSize & 3) != 0) {
                    wavNoteSize += 4 - (wavNoteSize & 3);
                }
            }
            var sizeNeeded = Marshal.SizeOf(typeof(WaveRiffSection));
            if (_info.LoopFlag) {
                sizeNeeded += Marshal.SizeOf(typeof(WaveSampleSection));
            }
            if (_info.Comment != null && _info.Comment.Length > 0) {
                sizeNeeded += 8 * wavNoteSize;
            }
            sizeNeeded += Marshal.SizeOf(typeof(WaveDataSection));
            return sizeNeeded;
        }

        public int GetWaveDataBlockNeededLength() {
            return 0x80 * GetSampleBitsFromParams() * (int)_info.ChannelCount;
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
            var loopCount = _decodeParam.Loop;
            var wavRiff = WaveRiffSection.CreateDefault();
            var wavSmpl = WaveSampleSection.CreateDefault();
            var wavNote = WaveNoteSection.CreateDefault();
            var wavData = WaveDataSection.CreateDefault();
            wavRiff.FmtType = (ushort)(_decodeParam.Mode != SamplingMode.Float ? 1 : 3);
            wavRiff.FmtChannels = (ushort)_info.ChannelCount;
            wavRiff.FmtBitCount = (ushort)(sampleBits > 0 ? sampleBits : sizeof(float));
            wavRiff.FmtSamplingRate = _info.SamplingRate;
            wavRiff.FmtSamplingSize = (ushort)(wavRiff.FmtBitCount / 8 * wavRiff.FmtChannels);
            wavRiff.FmtSamplesPerSec = wavRiff.FmtSamplingRate * wavRiff.FmtSamplingSize;
            if (_info.LoopFlag) {
                wavSmpl.SamplePeriod = (uint)(1000000000 / (double)wavRiff.FmtSamplingRate);
                wavSmpl.LoopStart = _info.LoopStart * 0x80 * 8 * wavRiff.FmtSamplingSize;
                wavSmpl.LoopEnd = _info.LoopR01 == 0x80 ? 0 : _info.LoopR01;
            } else if (_decodeParam.EnableLoop) {
                wavSmpl.LoopStart = 0;
                wavSmpl.LoopEnd = _info.BlockCount * 0x80 * 8 * wavRiff.FmtSamplingSize;
                _info.LoopStart = 0;
                _info.LoopEnd = _info.BlockCount;
            }
            if (_info.Comment != null) {
                wavNote.NoteSize = 4 + _info.CommentLength + 1;
                if ((wavNote.NoteSize & 3) != 0) {
                    wavNote.NoteSize += 4 - (wavNote.NoteSize & 3);
                }
            }
            wavData.DataSize = (uint)(_info.BlockCount * 0x80 * 8 * wavRiff.FmtSamplingSize + (wavSmpl.LoopEnd - wavSmpl.LoopStart) * loopCount);
            wavRiff.RiffSize = (uint)(0x1c + ((_info.LoopFlag && !_decodeParam.EnableLoop) ? Marshal.SizeOf(wavSmpl) : 0) + (_info.Comment != null ? wavNote.NoteSize : 0) + Marshal.SizeOf(wavData) + wavData.DataSize);

            var bytesWritten = stream.Write(wavRiff, 0);
            if (_info.LoopFlag && !_decodeParam.EnableLoop) {
                bytesWritten += stream.Write(wavSmpl, bytesWritten);
            }
            if (_info.Comment != null) {
                var address = bytesWritten;
                bytesWritten += stream.Write(wavNote, bytesWritten);
                stream.Write(_info.Comment, bytesWritten);
                bytesWritten = address + 8 + (int)wavNote.NoteSize;
                bytesWritten += 8 + (int)wavNote.NoteSize;
            }
            bytesWritten += stream.Write(wavData, bytesWritten);
            return bytesWritten;
        }

        public int DecodeData(byte[] buffer, out bool hasMore) {
            if (_status.DataCursor < _info.DataOffset) {
                _status.DataCursor = _info.DataOffset;
            }
            uint waveBlockSize = 0x80 * (uint)GetSampleBitsFromParams() * _info.ChannelCount;
            uint blockProcessableThisRound = (uint)buffer.Length / waveBlockSize;
            if (!_decodeParam.EnableLoop && !_info.LoopFlag) {
                int bufferCursor;
                if (_status.BlockIndex + blockProcessableThisRound >= _info.BlockCount) {
                    blockProcessableThisRound = _info.BlockCount - _status.BlockIndex;
                    hasMore = false;
                } else {
                    hasMore = true;
                }
                var streamBuffer = GetHcaBlockBuffer();
                if (!GenerateWaveData(_sourceStream, buffer, blockProcessableThisRound, streamBuffer, GetDecodeToBufferFunc(), out bufferCursor)) {
                    throw new HcaException();
                }
                _status.BlockIndex += blockProcessableThisRound;
                return bufferCursor;
            } else {
                throw new NotImplementedException();
            }
        }

        public HcaInfo HcaInfo {
            get {
                return _info;
            }
        }

        public float LengthInSecs {
            get { return _lengthInSecs; }
        }

        public int LengthInSamples {
            get { return _lengthInSamples; }
        }

        internal bool HasMore() {
            return _status.BlockIndex < _info.BlockCount;
        }

    }
}
