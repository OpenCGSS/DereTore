using System.IO;

namespace DereTore.HCA {
    public sealed class OneWayHcaDecoder : HcaDecoder {

        public OneWayHcaDecoder(Stream sourceStream)
            : base(sourceStream) {
            _status = new DecodeStatus();
        }

        public OneWayHcaDecoder(Stream sourceStream, DecodeParams decodeParams)
            : base(sourceStream, decodeParams) {
            _status = new DecodeStatus();
        }

        public int DecodeData(byte[] buffer, out bool hasMore) {
            return DecodeData(buffer, ref _status, out hasMore);
        }

        public bool HasMore() {
            return HasMore(ref _status);
        }

        private DecodeStatus _status;

    }
}
