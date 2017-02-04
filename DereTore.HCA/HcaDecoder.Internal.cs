using System;

namespace DereTore.HCA {
    partial class HcaDecoder {

        internal uint DecodeBlock(byte[] waveDataBuffer, uint blockIndex) {
            if (waveDataBuffer == null) {
                throw new ArgumentNullException(nameof(waveDataBuffer));
            }
            var waveBlockSize = GetMinWaveDataBufferSize();
            if (waveDataBuffer.Length < waveBlockSize) {
                throw new HcaException(ErrorMessages.GetBufferTooSmall(waveBlockSize, waveDataBuffer.Length), ActionResult.BufferTooSmall);
            }
            TransformWaveDataBlocks(SourceStream, waveDataBuffer, blockIndex, 1, GetProperWaveWriter());
            return 1;
        }

        internal uint DecodeBlocks(byte[] waveDataBuffer, uint startBlockIndex) {
            if (waveDataBuffer == null) {
                throw new ArgumentNullException(nameof(waveDataBuffer));
            }
            var waveBlockSize = GetMinWaveDataBufferSize();
            if (waveDataBuffer.Length < waveBlockSize) {
                throw new HcaException(ErrorMessages.GetBufferTooSmall(waveBlockSize, waveDataBuffer.Length), ActionResult.BufferTooSmall);
            }
            var hcaInfo = HcaInfo;
            var numBlocksToDecode = (uint)(waveDataBuffer.Length / waveBlockSize);
            if (startBlockIndex + numBlocksToDecode >= hcaInfo.BlockCount) {
                numBlocksToDecode = hcaInfo.BlockCount - startBlockIndex;
            }
            if (numBlocksToDecode == 0) {
                return 0;
            }
            TransformWaveDataBlocks(SourceStream, waveDataBuffer, startBlockIndex, numBlocksToDecode, GetProperWaveWriter());
            return numBlocksToDecode;
        }

    }
}
