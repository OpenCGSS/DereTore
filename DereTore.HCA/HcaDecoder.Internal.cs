using System;
using System.IO;

namespace DereTore.HCA {
    partial class HcaDecoder {

        internal int DecodeData(byte[] buffer, ref DecodeStatus status, out bool hasMore) {
            if (buffer == null) {
                throw new ArgumentNullException(nameof(buffer));
            }
            var minimumSize = GetMinWaveDataBufferSize();
            if (buffer.Length < minimumSize) {
                throw new HcaException(ErrorMessages.GetBufferTooSmall(minimumSize, buffer.Length), ActionResult.BufferTooSmall);
            }
            if (status.DataCursor < _hcaInfo.DataOffset) {
                status.DataCursor = _hcaInfo.DataOffset;
            }
            uint waveBlockSize = 0x80 * (uint)GetSampleBitsFromParams() * _hcaInfo.ChannelCount;
            uint blockProcessableThisRound = (uint)buffer.Length / waveBlockSize;
            if (!_decodeParams.EnableLoop && !_hcaInfo.LoopFlag) {
                int bufferCursor;
                if (status.BlockIndex + blockProcessableThisRound >= _hcaInfo.BlockCount) {
                    blockProcessableThisRound = _hcaInfo.BlockCount - status.BlockIndex;
                    hasMore = false;
                } else {
                    hasMore = true;
                }
                var streamBuffer = GetHcaBlockBuffer();
                GenerateWaveDataBlocks(SourceStream, buffer, blockProcessableThisRound, streamBuffer, GetDecodeToBufferFunc(), ref status, out bufferCursor);
                status.BlockIndex += blockProcessableThisRound;
                return bufferCursor;
            } else {
                throw new NotImplementedException();
            }
        }

        internal bool HasMore(ref DecodeStatus status) {
            return status.BlockIndex < _hcaInfo.BlockCount;
        }

        internal void SeekToStart(ref DecodeStatus status) {
            SourceStream.Seek(_hcaInfo.DataOffset, SeekOrigin.Begin);
            status.BlockIndex = 0;
            status.DataCursor = _hcaInfo.DataOffset;
            status.LoopNumber = 0;
        }

        internal void GenerateWaveDataBlocks(Stream source, byte[] destination, uint count, byte[] buffer, DecodeToBufferFunc modeFunc, ref DecodeStatus status, out int bytesDecoded) {
            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            }
            if (destination == null) {
                throw new ArgumentNullException(nameof(destination));
            }
            if (buffer == null) {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (modeFunc == null) {
                throw new ArgumentNullException(nameof(modeFunc));
            }
            source.Seek(status.DataCursor, SeekOrigin.Begin);
            var cursor = 0;
            for (uint l = 0; l < count; ++l) {
                source.Read(buffer, 0, buffer.Length);
                DecodeBlock(buffer, ref status);
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

    }
}
