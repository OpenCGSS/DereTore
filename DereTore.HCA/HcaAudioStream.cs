using System;
using System.Collections.Generic;
using System.IO;

namespace DereTore.HCA {
    public class HcaAudioStream : Stream {

        public HcaAudioStream(Stream sourceStream)
            : this(sourceStream, DecodeParams.Default, true) {
        }

        public HcaAudioStream(Stream sourceStream, bool outputWaveHeader)
            : this(sourceStream, DecodeParams.Default, outputWaveHeader) {
        }

        public HcaAudioStream(Stream sourceStream, DecodeParams decodeParams)
            : this(sourceStream, decodeParams, true) {
        }

        public HcaAudioStream(Stream sourceStream, DecodeParams decodeParams, bool outputWaveHeader) {
            var decoder = new HcaDecoder(sourceStream, decodeParams);
            _decoder = decoder;
            _hcaInfo = decoder.HcaInfo;
            _decodeStatus = new DecodeStatus();
            OutputWaveHeader = outputWaveHeader;
            var buffer = new byte[GetTotalAfterDecodingWaveDataSize()];
            _memoryCache = new MemoryStream(buffer, true);
            _decodedBlocks = new Dictionary<long, long>();
            _decodeBuffer = new byte[decoder.GetMinWaveDataBufferSize()];

            PrepareDecoding();
        }

        public override void Flush() {
            throw new NotSupportedException();
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
                    if (HasLoop) {
                        throw new NotSupportedException("Cannot seek to end in a looped HCA audio.");
                    }
                    Position = Length - offset;
                    break;
                default:
                    break;
            }
            return Position;
        }

        public override void SetLength(long value) {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            if (!EnsureNotDisposed()) {
                return 0;
            }
            if (!HasMoreData()) {
                return 0;
            }
            if (offset < 0) {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if (count < 0) {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (count == 0) {
                return 0;
            }
            var memoryCache = _memoryCache;
            var hcaInfo = _hcaInfo;
            var positionInWaveFileStream = GetPositionInWaveFileStream();
            int read;
            var headerCrossData = false;
            long origPosInStream = 0;
            var headerSize = _headerSize;
            if (positionInWaveFileStream + count < headerSize) {
                read = memoryCache.Read(buffer, offset, count);
                Position += read;
                return read;
            } else {
                if (positionInWaveFileStream < headerSize) {
                    origPosInStream = positionInWaveFileStream;
                    positionInWaveFileStream = headerSize;
                    headerCrossData = true;
                }
            }
            EnsureDecoded(positionInWaveFileStream, count);
            if (headerCrossData) {
                positionInWaveFileStream = origPosInStream;
            }
            try {
                memoryCache.Seek(positionInWaveFileStream, SeekOrigin.Begin);
                if (HasLoop) {
                    var decoder = _decoder;
                    var endLoopDataPositionIncludingHeader = hcaInfo.LoopEnd * decoder.GetMinWaveDataBufferSize() + headerSize;
                    var shouldRead = (int)Math.Min(count, endLoopDataPositionIncludingHeader - positionInWaveFileStream);
                    read = memoryCache.Read(buffer, offset, shouldRead);
                } else {
                    read = memoryCache.Read(buffer, offset, count);
                }
                Position += read;
            } catch (ObjectDisposedException) {
                return 0;
            }
            return read;
        }

        public override void Write(byte[] buffer, int offset, int count) {
            throw new NotSupportedException();
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;

        public override long Length {
            get {
                if (!EnsureNotDisposed()) {
                    return 0;
                }
                if (_length != null) {
                    return _length.Value;
                }
                var hcaInfo = _hcaInfo;
                if (hcaInfo.LoopFlag) {
                    _length = long.MaxValue;
                } else {
                    long totalLength = 0;
                    if (OutputWaveHeader) {
                        totalLength += _headerSize;
                    }
                    totalLength += _decoder.GetMinWaveDataBufferSize() * hcaInfo.BlockCount;
                    _length = totalLength;
                }
                return _length.Value;
            }
        }

        public override long Position {
            get {
                if (!EnsureNotDisposed()) {
                    return 0;
                }
                return _position;
            }
            set {
                EnsureNotDisposed();
                if (value < 0 || value > Length) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                _position = value;
            }
        }

        public bool OutputWaveHeader { get; }

        public HcaInfo HcaInfo => _decoder.HcaInfo;

        public float LengthInSeconds => HasLoop ? float.PositiveInfinity : _decoder.LengthInSeconds;

        public int LengthInSamples => _decoder.LengthInSamples;

        public bool IsDisposed => _isDisposed;

        public bool AllowDisposedOperations { get; set; }

        protected override void Dispose(bool disposing) {
            _memoryCache.Dispose();
            _isDisposed = true;
            base.Dispose(disposing);
        }

        private bool EnsureNotDisposed() {
            if (IsDisposed) {
                if (AllowDisposedOperations) {
                    return false;
                } else {
                    throw new ObjectDisposedException(typeof(HcaAudioStream).Name);
                }
            } else {
                return true;
            }
        }

        private long GetPositionInWaveFileStream() {
            var position = Position;
            if (!HasLoop) {
                return position;
            }
            var hcaInfo = _hcaInfo;
            var decoder = _decoder;
            var headerSize = _headerSize;
            var relativeDataPosition = position - headerSize;
            var endLoopDataPosition = hcaInfo.LoopEnd * decoder.GetMinWaveDataBufferSize();
            if (relativeDataPosition < endLoopDataPosition) {
                return position;
            }
            var startLoopDataPosition = hcaInfo.LoopStart * decoder.GetMinWaveDataBufferSize();
            var waveDataLength = endLoopDataPosition - startLoopDataPosition;
            var positionInsideLoop = (relativeDataPosition - startLoopDataPosition) % waveDataLength;
            return headerSize + startLoopDataPosition + positionInsideLoop;
        }

        private void EnsureDecoded(long dataOffset, int count) {
            var decodeStatus = _decodeStatus;
            var decoder = _decoder;
            var decodedBlocks = _decodedBlocks;
            var headerSize = _headerSize;
            var blockSize = decoder.GetMinWaveDataBufferSize();
            var startBlockIndex = (dataOffset - headerSize) / blockSize;
            var endBlockIndex = (dataOffset + count - headerSize) / blockSize;
            var hcaInfo = _hcaInfo;
            endBlockIndex = Math.Min(endBlockIndex, (int)hcaInfo.BlockCount - 1);
            var decodeBuffer = _decodeBuffer;
            var memoryCache = _memoryCache;
            for (var i = startBlockIndex; i <= endBlockIndex; ++i) {
                if (decodedBlocks.ContainsKey(i)) {
                    continue;
                }
                decodeStatus.BlockIndex = (uint)i;
                decodeStatus.DataCursor = (uint)i * hcaInfo.BlockSize + hcaInfo.DataOffset;
                decodeStatus.LoopNumber = 0;
                bool hasMore;
                decoder.DecodeData(decodeBuffer, ref decodeStatus, out hasMore);
                var positionInOutputStream = headerSize + blockSize * i;
                memoryCache.Seek(positionInOutputStream, SeekOrigin.Begin);
                memoryCache.WriteBytes(decodeBuffer);
                decodedBlocks.Add(i, positionInOutputStream);
            }
        }

        private void PrepareDecoding() {
            var decoder = _decoder;
            var memoryCache = _memoryCache;
            if (OutputWaveHeader) {
                var waveHeaderBufferSize = decoder.GetMinWaveHeaderBufferSize();
                var waveHeaderBuffer = new byte[waveHeaderBufferSize];
                decoder.WriteWaveHeader(waveHeaderBuffer);
                memoryCache.WriteBytes(waveHeaderBuffer);
                _headerSize = waveHeaderBufferSize;
            } else {
                _headerSize = 0;
            }
            memoryCache.Position = 0;
        }

        private bool HasMoreData() {
            return HasLoop || Position < Length;
        }

        private bool HasLoop => _hcaInfo.LoopFlag;

        private int GetTotalAfterDecodingWaveDataSize() {
            var hcaInfo = _hcaInfo;
            var totalLength = 0;
            if (OutputWaveHeader) {
                totalLength += _decoder.GetMinWaveHeaderBufferSize();
            }
            totalLength += _decoder.GetMinWaveDataBufferSize() * (int)hcaInfo.BlockCount;
            return totalLength;
        }

        private readonly HcaDecoder _decoder;
        private int _headerSize;
        private HcaInfo _hcaInfo;
        private readonly DecodeStatus _decodeStatus;
        private readonly MemoryStream _memoryCache;
        private readonly Dictionary<long, long> _decodedBlocks;
        private long? _length;
        private long _position;
        private bool _isDisposed;

        private readonly byte[] _decodeBuffer;

    }
}
