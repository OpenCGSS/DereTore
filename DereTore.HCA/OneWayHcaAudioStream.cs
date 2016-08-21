using System;
using System.IO;

namespace DereTore.HCA {
    public class OneWayHcaAudioStream : Stream {

        public OneWayHcaAudioStream(Stream sourceStream)
            : this(sourceStream, DecodeParams.Default, true) {
        }

        public OneWayHcaAudioStream(Stream sourceStream, bool outputWaveHeader)
            : this(sourceStream, DecodeParams.Default, outputWaveHeader) {
        }

        public OneWayHcaAudioStream(Stream sourceStream, DecodeParams decodeParams)
            : this(sourceStream, decodeParams, true) {
        }

        public OneWayHcaAudioStream(Stream sourceStream, DecodeParams decodeParams, bool outputWaveHeader) {
            _decoder = new OneWayHcaDecoder(sourceStream, decodeParams);
            OutputWaveHeader = outputWaveHeader;
            _state = HcaAudioStreamDecodeState.Initialized;
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
                    case HcaAudioStreamDecodeState.Initialized:
                        if (!OutputWaveHeader) {
                            _state = HcaAudioStreamDecodeState.WaveHeaderTransmitted;
                            loopControl = true;
                            break;
                        }
                        _waveHeaderSize = _decoder.GetMinWaveHeaderBufferSize();
                        _waveHeaderBuffer = new byte[_waveHeaderSize];
                        _waveHeaderSizeLeft = _waveHeaderSize;
                        _decoder.WriteWaveHeader(_waveHeaderBuffer);
                        maxToCopy = Math.Min(writeLengthLimit, _waveHeaderSizeLeft);
                        Array.Copy(_waveHeaderBuffer, _waveHeaderSize - _waveHeaderSizeLeft, buffer, offset, maxToCopy);
                        _waveHeaderSizeLeft -= maxToCopy;
                        _state = _waveHeaderSizeLeft <= 0 ? HcaAudioStreamDecodeState.WaveHeaderTransmitted : HcaAudioStreamDecodeState.WaveHeaderTransmitting;
                        return maxToCopy;
                    case HcaAudioStreamDecodeState.WaveHeaderTransmitting:
                        if (!OutputWaveHeader) {
                            _state = HcaAudioStreamDecodeState.WaveHeaderTransmitted;
                            loopControl = true;
                            break;
                        }
                        maxToCopy = Math.Min(writeLengthLimit, _waveHeaderSizeLeft);
                        Array.Copy(_waveHeaderBuffer, _waveHeaderSize - _waveHeaderSizeLeft, buffer, offset, maxToCopy);
                        _waveHeaderSizeLeft -= maxToCopy;
                        if (_waveHeaderSizeLeft <= 0) {
                            _state = HcaAudioStreamDecodeState.WaveHeaderTransmitted;
                        }
                        return maxToCopy;
                    case HcaAudioStreamDecodeState.WaveHeaderTransmitted:
                        _standardWaveDataSize = _decoder.GetMinWaveDataBufferSize();
                        _waveDataSize = _standardWaveDataSize * BlockBatchSize;
                        _waveDataBuffer = new byte[_waveDataSize];
                        decodedLength = _decoder.DecodeData(_waveDataBuffer, out hasMore);
                        _waveDataSize = decodedLength;
                        _waveDataSizeLeft = _waveDataSize;
                        maxToCopy = Math.Min(writeLengthLimit, _waveDataSizeLeft);
                        Array.Copy(_waveDataBuffer, _waveDataSize - _waveDataSizeLeft, buffer, offset, maxToCopy);
                        _waveDataSizeLeft -= maxToCopy;
                        if (_waveDataSizeLeft <= 0) {
                            _state = hasMore ? HcaAudioStreamDecodeState.DataTransmitting : HcaAudioStreamDecodeState.WaveHeaderTransmitted;
                        } else {
                            _state = HcaAudioStreamDecodeState.DataTransmitting;
                        }
                        return maxToCopy;
                    case HcaAudioStreamDecodeState.DataTransmitting:
                        if (_waveDataSizeLeft <= 0) {
                            _waveDataSize = _standardWaveDataSize * BlockBatchSize;
                            _waveDataBuffer = new byte[_waveDataSize];
                            decodedLength = _decoder.DecodeData(_waveDataBuffer, out hasMore);
                            if (decodedLength > 0 || hasMore) {
                                _waveDataSize = decodedLength;
                                _waveDataSizeLeft = _waveDataSize;
                                _state = HcaAudioStreamDecodeState.DataTransmitting;
                            } else {
                                _state = HcaAudioStreamDecodeState.WaveHeaderTransmitted;
                                loopControl = true;
                                break;
                            }
                        }
                        maxToCopy = Math.Min(writeLengthLimit, _waveDataSizeLeft);
                        Array.Copy(_waveDataBuffer, _waveDataSize - _waveDataSizeLeft, buffer, offset, maxToCopy);
                        _waveDataSizeLeft -= maxToCopy;
                        return maxToCopy;
                    case HcaAudioStreamDecodeState.DataTransmitted:
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
                if ((_state & HcaAudioStreamDecodeState.CanDoHasMoreCheck) != 0) {
                    var hasMore = _decoder.HasMore();
                    return !(_waveDataSizeLeft <= 0 && _waveHeaderSizeLeft <= 0 && !hasMore);
                } else {
                    return _state == HcaAudioStreamDecodeState.Initialized;
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

        public bool OutputWaveHeader { get; }

        public int BlockBatchSize => 10;

        public HcaInfo HcaInfo => _decoder.HcaInfo;

        public float LengthInSeconds => _decoder.LengthInSeconds;

        public int LengthInSamples => _decoder.LengthInSamples;

        private readonly OneWayHcaDecoder _decoder;
        private HcaAudioStreamDecodeState _state;
        private byte[] _waveHeaderBuffer;
        private int _waveHeaderSize;
        private int _waveHeaderSizeLeft;
        private byte[] _waveDataBuffer;
        private int _waveDataSize;
        private int _standardWaveDataSize;
        private int _waveDataSizeLeft;

    }
}