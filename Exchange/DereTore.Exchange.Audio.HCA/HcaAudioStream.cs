using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DereTore.Common;

namespace DereTore.Exchange.Audio.HCA {
    public sealed class HcaAudioStream : HcaAudioStreamBase {

        public HcaAudioStream(Stream baseStream)
            : this(baseStream, DecodeParams.Default) {
        }

        public HcaAudioStream(Stream baseStream, AudioParams audioParams)
            : this(baseStream, DecodeParams.Default, audioParams) {
        }

        public HcaAudioStream(Stream baseStream, DecodeParams decodeParams)
            : this(baseStream, decodeParams, AudioParams.Default) {
        }

        public HcaAudioStream(Stream baseStream, DecodeParams decodeParams, AudioParams audioParams)
            : base(baseStream, decodeParams) {
            var decoder = new HcaDecoder(baseStream, decodeParams);
            _decoder = decoder;
            _audioParams = audioParams;
            var buffer = new byte[GetTotalAfterDecodingWaveDataSize()];
            _memoryCache = new MemoryStream(buffer, true);
            _decodedBlocks = new Dictionary<long, long>();
            _decodeBuffer = new byte[decoder.GetMinWaveDataBufferSize()];
            _hasLoop = decoder.HcaInfo.LoopFlag;

            PrepareDecoding();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            if (!EnsureNotDisposed()) {
                return 0;
            }
            switch (origin) {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position = Position + offset;
                    break;
                case SeekOrigin.End:
                    Position = Length - offset;
                    break;
                default:
                    break;
            }
            return Position;
        }

        public override int Read(byte[] buffer, int offset, int count) {
            if (!EnsureNotDisposed()) {
                return 0;
            }
            if (!HasMoreData) {
                return 0;
            }
            if (offset < 0 || offset >= buffer.Length) {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if (count < 0) {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (count == 0) {
                return 0;
            }
            var hcaInfo = HcaInfo;
            var memoryCache = _memoryCache;
            var position = Position;
            var read = 0;
            if (position < hcaInfo.DataOffset) {
                var maxCopy = Math.Min(Math.Min((int)(hcaInfo.DataOffset - position), count), buffer.Length - offset);
                if (maxCopy == 0) {
                    return 0;
                }
                memoryCache.Seek(position, SeekOrigin.Begin);
                memoryCache.Read(buffer, offset, maxCopy);
                count -= maxCopy;
                offset += maxCopy;
                position += maxCopy;
                if (count <= 0 || offset >= buffer.Length) {
                    Position = position;
                    return maxCopy;
                }
                read += maxCopy;
            }

            var audioParams = _audioParams;
            var length = Length;
            count = HasLoop && audioParams.InfiniteLoop ? Math.Min(count, buffer.Length - offset) : Math.Min((int)(length - position), count);
            if (_decodedBlocks.Count < hcaInfo.BlockCount) {
                if (HasLoop && audioParams.SimulatedLoopCount > 0) {
                    EnsureSoundDataDecodedWithLoops(position, count);
                } else {
                    EnsureSoundDataDecoded(position, count);
                }
            }
            var headerSize = _headerSize;
            var waveBlockSize = _decoder.GetMinWaveDataBufferSize();
            while (count > 0) {
                var positionNonLooped = MapPosToNonLoopedWaveStreamPos(position);
                memoryCache.Seek(positionNonLooped, SeekOrigin.Begin);
                var shouldRead = Math.Min(count, waveBlockSize - ((int)(positionNonLooped - headerSize) % waveBlockSize));
                if (shouldRead == 0) {
                    break;
                }
                var realRead = memoryCache.Read(buffer, offset, shouldRead);
                Debug.Assert(realRead == shouldRead);
                count -= realRead;
                offset += realRead;
                read += realRead;
                position += realRead;
            }
            Position = position;
            return read;
        }

        public override bool CanSeek => true;

        public override long Length {
            get {
                if (!EnsureNotDisposed()) {
                    return 0;
                }
                if (_length != null) {
                    return _length.Value;
                }
                var audioParams = _audioParams;
                if (HasLoop && audioParams.InfiniteLoop) {
                    _length = long.MaxValue;
                    return _length.Value;
                }
                var hcaInfo = HcaInfo;
                long totalLength = 0;
                if (_audioParams.OutputWaveHeader) {
                    totalLength += _headerSize;
                }
                var waveDataBlockSize = _decoder.GetMinWaveDataBufferSize();
                totalLength += waveDataBlockSize * hcaInfo.BlockCount;
                if (HasLoop && audioParams.SimulatedLoopCount > 0) {
                    var loopedBlockCount = hcaInfo.LoopEnd - hcaInfo.LoopStart;
                    totalLength += waveDataBlockSize * loopedBlockCount * audioParams.SimulatedLoopCount;
                }
                _length = totalLength;
                return _length.Value;
            }
        }

        public override long Position {
            get {
                return EnsureNotDisposed() ? _position : 0;
            }
            set {
                EnsureNotDisposed();
                if (value < 0 || value > Length) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                _position = value;
            }
        }

        public override float LengthInSeconds => HcaHelper.CalculateLengthInSeconds(HcaInfo, _audioParams.SimulatedLoopCount, _audioParams.InfiniteLoop);

        public override uint LengthInSamples => HcaHelper.CalculateLengthInSamples(HcaInfo, _audioParams.SimulatedLoopCount, _audioParams.InfiniteLoop);

        protected override void Dispose(bool disposing) {
            if (!IsDisposed) {
                _memoryCache.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override bool HasMoreData => Position < Length;

        private void EnsureSoundDataDecoded(long waveDataOffset, int byteCount) {
            var decoder = _decoder;
            var headerSize = _headerSize;
            var blockSize = decoder.GetMinWaveDataBufferSize();
            var startBlockIndex = (uint)((waveDataOffset - headerSize) / blockSize);
            var hcaInfo = HcaInfo;
            var endBlockIndex = (uint)((waveDataOffset + byteCount - headerSize) / blockSize);
            endBlockIndex = Math.Min(endBlockIndex, hcaInfo.BlockCount - 1);
            for (var i = startBlockIndex; i <= endBlockIndex; ++i) {
                EnsureDecodeOneBlock(i);
            }
        }

        private void EnsureSoundDataDecodedWithLoops(long waveDataOffset, int byteCount) {
            waveDataOffset = MapPosToNonLoopedWaveStreamPos(waveDataOffset);
            var decoder = _decoder;
            var headerSize = _headerSize;
            var waveBlockSize = decoder.GetMinWaveDataBufferSize();
            var startBlockIndex = (uint)((waveDataOffset - headerSize) / waveBlockSize);
            var waveDataEnd = waveDataOffset + byteCount;
            var hcaInfo = HcaInfo;
            var audioParams = _audioParams;
            var decodedSize = (waveDataOffset - headerSize) / waveBlockSize * waveBlockSize;
            // For those before or in the loop range...
            if (audioParams.InfiniteLoop || startBlockIndex <= hcaInfo.LoopEnd - 1 + (hcaInfo.LoopEnd - hcaInfo.LoopStart) * audioParams.SimulatedLoopCount) {
                var blockIndex = startBlockIndex;
                var executed = false;
                while (true) {
                    if (executed && blockIndex == startBlockIndex) {
                        if (audioParams.InfiniteLoop) {
                            return;
                        } else {
                            break;
                        }
                    }
                    if (!audioParams.InfiniteLoop && decodedSize >= waveDataEnd) {
                        break;
                    }
                    EnsureDecodeOneBlock(blockIndex);
                    decodedSize += waveBlockSize;
                    ++blockIndex;
                    if (blockIndex > hcaInfo.LoopEnd - 1) {
                        blockIndex = hcaInfo.LoopStart;
                    }
                    executed = true;
                }
            }
            // And those following it.
            if (audioParams.InfiniteLoop) {
                return;
            }
            var endBlockIndex = hcaInfo.LoopEnd - 1 + (hcaInfo.LoopEnd - hcaInfo.LoopStart) * audioParams.SimulatedLoopCount;
            var startOfAfterLoopRegion = endBlockIndex * waveBlockSize;
            if (waveDataEnd >= startOfAfterLoopRegion) {
                for (var blockIndex = hcaInfo.LoopEnd; blockIndex < hcaInfo.BlockCount && decodedSize < waveDataEnd; ++blockIndex) {
                    EnsureDecodeOneBlock(blockIndex);
                    decodedSize += waveBlockSize;
                }
            }
        }

        private void EnsureDecodeOneBlock(uint blockIndex) {
            var decodedBlocks = _decodedBlocks;
            if (decodedBlocks.ContainsKey(blockIndex)) {
                return;
            }
            var decodeBuffer = _decodeBuffer;
            var decoder = _decoder;
            decoder.DecodeBlock(decodeBuffer, blockIndex);
            var positionInOutputStream = _headerSize + decoder.GetMinWaveDataBufferSize() * blockIndex;
            var memoryCache = _memoryCache;
            memoryCache.Seek(positionInOutputStream, SeekOrigin.Begin);
            memoryCache.WriteBytes(decodeBuffer);
            decodedBlocks.Add(blockIndex, positionInOutputStream);
        }

        private long MapPosToNonLoopedWaveStreamPos(long requestedPosition) {
            if (!HasLoop) {
                return requestedPosition;
            }
            var hcaInfo = HcaInfo;
            var waveBlockSize = _decoder.GetMinWaveDataBufferSize();
            var headerSize = _headerSize;
            var relativeDataPosition = requestedPosition - headerSize;
            var endLoopDataPosition = (hcaInfo.LoopEnd) * waveBlockSize;
            if (relativeDataPosition < endLoopDataPosition) {
                return requestedPosition;
            }
            var startLoopDataPosition = hcaInfo.LoopStart * waveBlockSize;
            var loopLength = endLoopDataPosition - startLoopDataPosition;
            var audioParams = _audioParams;
            if (audioParams.InfiniteLoop) {
                return headerSize + startLoopDataPosition + (relativeDataPosition - startLoopDataPosition) % loopLength;
            }
            var extendedLength = endLoopDataPosition + loopLength * audioParams.SimulatedLoopCount;
            if (relativeDataPosition < extendedLength) {
                return headerSize + startLoopDataPosition + (relativeDataPosition - startLoopDataPosition) % loopLength;
            } else {
                return headerSize + endLoopDataPosition + (relativeDataPosition - extendedLength);
            }
        }

        private bool HasLoop => _hasLoop;

        private void PrepareDecoding() {
            var decoder = _decoder;
            var memoryCache = _memoryCache;
            if (_audioParams.OutputWaveHeader) {
                var waveHeaderBufferSize = decoder.GetMinWaveHeaderBufferSize();
                var waveHeaderBuffer = new byte[waveHeaderBufferSize];
                decoder.WriteWaveHeader(waveHeaderBuffer, _audioParams);
                memoryCache.WriteBytes(waveHeaderBuffer);
                _headerSize = waveHeaderBufferSize;
            } else {
                _headerSize = 0;
            }
            memoryCache.Position = 0;
        }

        private int GetTotalAfterDecodingWaveDataSize() {
            var hcaInfo = HcaInfo;
            var totalLength = 0;
            if (_audioParams.OutputWaveHeader) {
                totalLength += _decoder.GetMinWaveHeaderBufferSize();
            }
            totalLength += _decoder.GetMinWaveDataBufferSize() * (int)hcaInfo.BlockCount;
            return totalLength;
        }

        private readonly AudioParams _audioParams;
        private int _headerSize;
        private readonly MemoryStream _memoryCache;
        private readonly Dictionary<long, long> _decodedBlocks;
        private long? _length;
        private long _position;
        private readonly byte[] _decodeBuffer;
        private readonly bool _hasLoop;

    }
}
