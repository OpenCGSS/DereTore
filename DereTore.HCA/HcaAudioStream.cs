using System;
using System.IO;

namespace DereTore.HCA {
    public sealed class HcaAudioStream : Stream {

        public HcaAudioStream(Stream sourceStream)
            : this(sourceStream, DecodeParams.Default) {
        }

        public HcaAudioStream(Stream sourceStream, DecodeParams decodeParams) {
            _decoder = new HcaDecoder(sourceStream, decodeParams);
            OutputWaveHeader = true;
            _state = DecodeState.Initialized;
        }

        public override void Flush() {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotSupportedException();
        }

        public override void SetLength(long value) {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            if (!CanRead) {
                return 0;
            }
            var loopControl = false;
            var writeLengthLimit = Math.Min(count, buffer.Length);
            do {
                int maxToCopy;
                bool hasMore;
                int decodedLength;
                var state = _state;
                switch (state) {
                    case DecodeState.Initialized:
                        if (!OutputWaveHeader) {
                            _state = DecodeState.WaveHeaderTransmitted;
                            loopControl = true;
                            break;
                        }
                        _waveHeaderSize = _decoder.GetWaveHeaderNeededLength();
                        _waveHeaderBuffer = new byte[_waveHeaderSize];
                        _waveHeaderSizeLeft = _waveHeaderSize;
                        _decoder.WriteWaveHeader(_waveHeaderBuffer);
                        maxToCopy = Math.Min(writeLengthLimit, _waveHeaderSizeLeft);
                        Array.Copy(_waveHeaderBuffer, _waveHeaderSize - _waveHeaderSizeLeft, buffer, offset, maxToCopy);
                        _waveHeaderSizeLeft -= maxToCopy;
                        _state = _waveHeaderSizeLeft <= 0 ? DecodeState.WaveHeaderTransmitted : DecodeState.WaveHeaderTransmitting;
                        return maxToCopy;
                    case DecodeState.WaveHeaderTransmitting:
                        if (!OutputWaveHeader) {
                            _state = DecodeState.WaveHeaderTransmitted;
                            loopControl = true;
                            break;
                        }
                        maxToCopy = Math.Min(writeLengthLimit, _waveHeaderSizeLeft);
                        Array.Copy(_waveHeaderBuffer, _waveHeaderSize - _waveHeaderSizeLeft, buffer, offset, maxToCopy);
                        _waveHeaderSizeLeft -= maxToCopy;
                        if (_waveHeaderSizeLeft <= 0) {
                            _state = DecodeState.WaveHeaderTransmitted;
                        }
                        return maxToCopy;
                    case DecodeState.WaveHeaderTransmitted:
                        _standardWaveDataSize = _decoder.GetWaveDataBlockNeededLength();
                        _waveDataSize = _standardWaveDataSize * BlockBatchSize;
                        _waveDataBuffer = new byte[_waveDataSize];
                        decodedLength = _decoder.DecodeData(_waveDataBuffer, out hasMore);
                        _waveDataSize = decodedLength;
                        _waveDataSizeLeft = _waveDataSize;
                        maxToCopy = Math.Min(writeLengthLimit, _waveDataSizeLeft);
                        Array.Copy(_waveDataBuffer, _waveDataSize - _waveDataSizeLeft, buffer, offset, maxToCopy);
                        _waveDataSizeLeft -= maxToCopy;
                        if (_waveDataSizeLeft <= 0) {
                            _state = hasMore ? DecodeState.DataTransmitting : DecodeState.WaveHeaderTransmitted;
                        } else {
                            _state = DecodeState.DataTransmitting;
                        }
                        return maxToCopy;
                    case DecodeState.DataTransmitting:
                        if (_waveDataSizeLeft <= 0) {
                            _waveDataSize = _standardWaveDataSize * BlockBatchSize;
                            _waveDataBuffer = new byte[_waveDataSize];
                            decodedLength = _decoder.DecodeData(_waveDataBuffer, out hasMore);
                            if (decodedLength > 0 || hasMore) {
                                _waveDataSize = decodedLength;
                                _waveDataSizeLeft = _waveDataSize;
                                _state = DecodeState.DataTransmitting;
                            } else {
                                _state = DecodeState.WaveHeaderTransmitted;
                                loopControl = true;
                                break;
                            }
                        }
                        maxToCopy = Math.Min(writeLengthLimit, _waveDataSizeLeft);
                        Array.Copy(_waveDataBuffer, _waveDataSize - _waveDataSizeLeft, buffer, offset, maxToCopy);
                        _waveDataSizeLeft -= maxToCopy;
                        return maxToCopy;
                    case DecodeState.DataTransmitted:
                        return 0;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(state));
                }
            } while (loopControl);
            return 0;
        }

        public override void Write(byte[] buffer, int offset, int count) {
            throw new NotSupportedException();
        }

        public override bool CanRead {
            get {
                if ((_state & DecodeState.CanDoHasMoreCheck) != 0) {
                    var hasMore = _decoder.HasMore();
                    return !(_waveDataSizeLeft <= 0 && _waveHeaderSizeLeft <= 0 && !hasMore);
                } else {
                    return _state == DecodeState.Initialized;
                }
            }
        }

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length {
            get { throw new NotSupportedException(); }
        }

        public override long Position {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public bool OutputWaveHeader { get; set; }

        public int BlockBatchSize => 10;

        public HcaInfo HcaInfo => _decoder.HcaInfo;

        public float LengthInSeconds => _decoder.LengthInSeconds;

        public int LengthInSamples => _decoder.LengthInSamples;

        protected override void Dispose(bool disposing) {
            if (!_isDisposed) {
                if (disposing) {
                    _isDisposed = true;
                }
            }
            base.Dispose(disposing);
        }

        private readonly HcaDecoder _decoder;
        private byte[] _waveHeaderBuffer;
        private int _waveHeaderSize;
        private int _waveHeaderSizeLeft;
        private byte[] _waveDataBuffer;
        private int _waveDataSize;
        private int _standardWaveDataSize;
        private int _waveDataSizeLeft;
        private bool _isDisposed;
        private DecodeState _state;

        [Flags]
        private enum DecodeState {
            Initialized = 0,
            WaveHeaderTransmitting = 1,
            WaveHeaderTransmitted = 2,
            DataTransmitting = 4,
            DataTransmitted = 8,
            CanDoHasMoreCheck = WaveHeaderTransmitting | WaveHeaderTransmitted | DataTransmitting,
        }

    }
}