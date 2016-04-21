using System;
using System.IO;

namespace DereTore.HCA.Native {
    public sealed class HcaAudioStream : Stream {

        public HcaAudioStream(string filename)
            : this(filename, 0, 0) {
        }

        public HcaAudioStream(string filename, uint key1, uint key2)
            : this() {
            _standardWaveDataSize = 0;
            NativeMethods.KsOpenFile(filename, out _hDecode);
            NativeMethods.KsSetParamI32(_hDecode, KsParamType.Key1, key1);
            NativeMethods.KsSetParamI32(_hDecode, KsParamType.Key2, key2);
            NativeMethods.KsBeginDecode(_hDecode);
            NativeMethods.KsGetHcaInfo(_hDecode, out _info);
        }

        private HcaAudioStream() {
            _hDecode = IntPtr.Zero;
            _isDisposed = false;
            _state = DecodeState.Initialized;
        }

        protected override void Dispose(bool disposing) {
            if (!_isDisposed) {
                if (!disposing) {
                    if (_hDecode != IntPtr.Zero) {
                        NativeMethods.KsEndDecode(_hDecode);
                        NativeMethods.KsCloseHandle(_hDecode);
                        _hDecode = IntPtr.Zero;
                        _isDisposed = true;
                    }
                }
            }
            base.Dispose(disposing);
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
            int bytesShouldCopy;
            switch (_state) {
                case DecodeState.Initialized:
                    NativeMethods.KsGetWaveHeader(_hDecode, null, ref _waveHeaderSize);
                    if (_waveHeaderSize <= 0) {
                        throw new IOException("Wave header size is less than or equal to 0");
                    }
                    _waveHeaderBuffer = new byte[_waveHeaderSize];
                    NativeMethods.KsGetWaveHeader(_hDecode, _waveHeaderBuffer, ref _waveHeaderSize);
                    _waveHeaderSizeLeft = _waveHeaderSize;
                    bytesShouldCopy = (int)Math.Min(_waveHeaderSizeLeft, count);
                    Array.Copy(_waveHeaderBuffer, _waveHeaderSize - _waveHeaderSizeLeft, buffer, offset, bytesShouldCopy);
                    if (count < _waveHeaderSizeLeft) {
                        _waveHeaderSizeLeft -= (uint)count;
                        _state = DecodeState.WaveHeaderTransmitting;
                    } else {
                        _waveHeaderSizeLeft = 0;
                        _state = DecodeState.WaveHeaderTransmitted;
                    }
                    return bytesShouldCopy;
                case DecodeState.WaveHeaderTransmitting:
                    bytesShouldCopy = (int)Math.Min(_waveHeaderSizeLeft, count);
                    Array.Copy(_waveHeaderBuffer, _waveHeaderSize - _waveHeaderSizeLeft, buffer, offset, bytesShouldCopy);
                    if (count < _waveHeaderSizeLeft) {
                        _waveHeaderSizeLeft -= (uint)count;
                    } else {
                        _waveHeaderSizeLeft = 0;
                        _waveHeaderBuffer = null;
                        _state = DecodeState.WaveHeaderTransmitted;
                    }
                    return bytesShouldCopy;
                case DecodeState.WaveHeaderTransmitted:
                    NativeMethods.KsDecodeData(_hDecode, null, ref _standardWaveDataSize);
                    if (_standardWaveDataSize <= 0) {
                        throw new IOException("Wave data size is less than or equal to 0.");
                    }

                    return DecodeBlocksData(buffer, offset, count);
                case DecodeState.DataTransmitting:
                    if (_waveDataSizeLeft <= 0) {
                        return DecodeBlocksData(buffer, offset, count);
                    }
                    bytesShouldCopy = (int)Math.Min(_waveDataSizeLeft, count);
                    Array.Copy(_waveDataBuffer, _waveDataSize - _waveDataSizeLeft, buffer, offset, bytesShouldCopy);
                    if (count < _waveDataSizeLeft) {
                        _waveDataSizeLeft -= (uint)count;
                    } else {
                        _waveDataSizeLeft = 0;
                    }
                    if (_waveDataSizeLeft <= 0) {
                        _waveDataBuffer = null;
                        _waveDataSize = 0;
                        bool hasMore;
                        NativeMethods.KsHasMoreData(_hDecode, out hasMore);
                        _state = hasMore ? DecodeState.DataTransmitting : DecodeState.DataTransmitted;
                    }
                    return bytesShouldCopy;
                case DecodeState.DataTransmitted:
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException("_state");
            }
        }

        public override void Write(byte[] buffer, int offset, int count) {
            throw new NotSupportedException();
        }

        public override bool CanRead {
            get {
                if ((_state & DecodeState.CanDoHasMoreCheck) != 0) {
                    bool hasMore;
                    var r = NativeMethods.KsHasMoreData(_hDecode, out hasMore);
                    return r >= 0 && hasMore;
                } else {
                    return _state == DecodeState.Initialized;
                }
            }
        }

        public override bool CanSeek {
            get { return false; }
        }

        public override bool CanWrite {
            get { return false; }
        }

        public override long Length {
            get {
                throw new NotSupportedException();
            }
        }

        public override long Position {
            get {
                throw new NotSupportedException();
            }
            set {
                throw new NotSupportedException();
            }
        }

        public HcaInfo HcaInfo {
            get { return _info; }
        }

        public uint BlockBatchSize {
            get { return 10; }
        }

        private int DecodeBlocksData(byte[] buffer, int offset, int count) {
            // Several blocks each time.
            _waveDataSize = _standardWaveDataSize * BlockBatchSize;
            _waveDataBuffer = new byte[_waveDataSize];
            var dataLength = (uint)_waveDataBuffer.Length;
            NativeMethods.KsDecodeData(_hDecode, _waveDataBuffer, ref dataLength);
            _waveDataSize = dataLength;
            _waveDataSizeLeft = _waveDataSize;
            var bytesShouldCopy = (int)Math.Min(_waveDataSizeLeft, count);
            Array.Copy(_waveDataBuffer, _waveDataSize - _waveDataSizeLeft, buffer, offset, bytesShouldCopy);
            if (count < _waveDataSizeLeft) {
                _waveDataSizeLeft -= (uint)count;
            } else {
                _waveDataSizeLeft = 0;
            }
            if (_waveDataSizeLeft <= 0) {
                _waveDataBuffer = null;
                _waveDataSize = 0;
                _state = DecodeState.DataTransmitted;
            } else {
                _state = DecodeState.DataTransmitting;
            }
            return bytesShouldCopy;
        }

        private readonly HcaInfo _info;
        private IntPtr _hDecode;
        private byte[] _waveHeaderBuffer;
        private uint _waveHeaderSize;
        private uint _waveHeaderSizeLeft;
        private byte[] _waveDataBuffer;
        private uint _waveDataSize;
        private uint _standardWaveDataSize;
        private uint _waveDataSizeLeft;
        private bool _isDisposed;
        private DecodeState _state;

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
