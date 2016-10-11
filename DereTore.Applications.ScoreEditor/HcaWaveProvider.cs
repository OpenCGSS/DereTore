using System.IO;
using DereTore.HCA;
using NAudio.Wave;

namespace DereTore.Applications.ScoreEditor {
    public class HcaWaveProvider : HcaAudioStream, IWaveProvider {

        public HcaWaveProvider(Stream sourceStream)
            : base(sourceStream) {
            var hcaInfo = HcaInfo;
            WaveFormat = new WaveFormat((int)hcaInfo.SamplingRate, 16, (int)hcaInfo.ChannelCount);
        }

        public HcaWaveProvider(Stream sourceStream, DecodeParams decodeParams)
            : base(sourceStream, decodeParams) {
            var hcaInfo = HcaInfo;
            WaveFormat = new WaveFormat((int)hcaInfo.SamplingRate, 16, (int)hcaInfo.ChannelCount);
        }

        public WaveFormat WaveFormat { get; }

    }
}
