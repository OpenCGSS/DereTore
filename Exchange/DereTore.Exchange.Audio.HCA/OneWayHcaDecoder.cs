using System.IO;

namespace DereTore.Exchange.Audio.HCA {
    public sealed class OneWayHcaDecoder : HcaDecoder {

        public OneWayHcaDecoder(Stream sourceStream)
            : base(sourceStream) {
        }

        public OneWayHcaDecoder(Stream sourceStream, DecodeParams decodeParams)
            : base(sourceStream, decodeParams) {
        }

        public uint DecodeBlocks(byte[] buffer) {
            if (!HasMore) {
                return 0;
            }
            var decodedBlockCount = DecodeBlocks(buffer, _blockIndex);
            _blockIndex += decodedBlockCount;
            return decodedBlockCount;
        }

        public bool HasMore => _blockIndex < HcaInfo.BlockCount;

        public uint LengthInSamples => HcaHelper.CalculateLengthInSamples(HcaInfo);

        public float LengthInSeconds => HcaHelper.CalculateLengthInSeconds(HcaInfo);

        private uint _blockIndex = 0;

    }
}
