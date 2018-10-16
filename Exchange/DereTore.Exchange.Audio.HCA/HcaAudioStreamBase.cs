using System;
using System.IO;

namespace DereTore.Exchange.Audio.HCA {
    public abstract class HcaAudioStreamBase : Stream {

        protected HcaAudioStreamBase(Stream baseStream, DecodeParams decodeParams) {
            BaseStream = baseStream;
            _decodeParams = decodeParams;
        }

        public Stream BaseStream { get; }

        public sealed override void Flush() {
            throw new NotSupportedException();
        }

        public sealed override void SetLength(long value) {
            throw new NotSupportedException();
        }

        public sealed override void Write(byte[] buffer, int offset, int count) {
            throw new NotSupportedException();
        }

        public sealed override bool CanRead => true;

        public sealed override bool CanWrite => false;

        public bool AllowDisposedOperations { get; set; }

        public bool IsDisposed => _isDisposed;

        public HcaDecoder Decoder => _decoder;

        public abstract float LengthInSeconds { get; }

        public abstract uint LengthInSamples { get; }

        public HcaInfo HcaInfo => _decoder.HcaInfo;

        public DecodeParams DecodeParams => _decodeParams;

        protected bool EnsureNotDisposed() {
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

        protected override void Dispose(bool disposing) {
            _isDisposed = true;
            base.Dispose(disposing);
        }

        protected abstract bool HasMoreData { get; }

        protected HcaDecoder _decoder;
        private bool _isDisposed;
        private readonly DecodeParams _decodeParams;

    }
}
