using System.IO;
using DereTore.HCA;
using NAudio.Wave;

namespace DereTore.Application.ScoreEditor {
    public class HcaWaveProvider : HcaAudioStream, IWaveProvider {

        public HcaWaveProvider(Stream sourceStream)
            : base(sourceStream) {
        }

        public HcaWaveProvider(Stream sourceStream, DecodeParams decodeParams)
            : base(sourceStream, decodeParams) {
        }

        public WaveFormat WaveFormat => DefaultWaveFormat;

        public static readonly WaveFormat DefaultWaveFormat = new WaveFormat();

    }
}
