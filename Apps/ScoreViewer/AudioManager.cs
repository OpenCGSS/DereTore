using DereTore.Common;
using NAudio.Wave;
using SharpAL;

namespace DereTore.Apps.ScoreViewer {
    public sealed class AudioManager : DisposableBase {

        public AudioManager() {
            _audioDevice = new AudioDevice();
            _audioContext = new AudioContext(_audioDevice);
        }

        public AudioDevice AudioDevice => _audioDevice;

        public AudioContext AudioContext => _audioContext;

        public static readonly WaveFormat StandardFormat = new WaveFormat();

        public static bool NeedsConversion(WaveFormat test, WaveFormat standard) {
            return test.SampleRate != standard.SampleRate ||
                   test.BitsPerSample != standard.BitsPerSample ||
                   test.Channels != standard.Channels ||
                   test.Encoding != standard.Encoding;
        }

        protected override void Dispose(bool disposing) {
            _audioContext?.Dispose();
            _audioDevice?.Dispose();

            _audioContext = null;
            _audioDevice = null;
        }

        private AudioDevice _audioDevice;
        private AudioContext _audioContext;

    }
}
