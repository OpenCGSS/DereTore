using System;
using System.IO;
using DereTore.ACB;
using DereTore.HCA;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using AudioOut = NAudio.Wave.WasapiOut;

namespace DereTore.Applications.ScoreEditor {
    public sealed class LiveMusicPlayer : DisposableBase {

        public static LiveMusicPlayer FromAcbStream(Stream stream, string acbFileName) {
            return new LiveMusicPlayer(stream, acbFileName, DecodeParams.Default);
        }

        public static LiveMusicPlayer FromAcbStream(Stream stream, string acbFileName, DecodeParams decodeParams) {
            return new LiveMusicPlayer(stream, acbFileName, decodeParams);
        }

        public static LiveMusicPlayer FromWaveStream(Stream stream) {
            return new LiveMusicPlayer(stream);
        }

        public static LiveMusicPlayer FromHcaStream(Stream stream) {
            return new LiveMusicPlayer(stream, DecodeParams.Default);
        }

        public static LiveMusicPlayer FromHcaStream(Stream stream, DecodeParams decodeParams) {
            return new LiveMusicPlayer(stream, decodeParams);
        }

        public event EventHandler<StoppedEventArgs> PlaybackStopped {
            add {
                if (_soundPlayer != null) {
                    _soundPlayer.PlaybackStopped += value;
                }
            }
            remove {
                if (_soundPlayer != null) {
                    _soundPlayer.PlaybackStopped -= value;
                }
            }
        }

        public void Play() {
            if (IsPlaying) {
                if (IsPaused) {
                    IsPaused = false;
                } else {
                    Stop();
                }
            }
            _soundPlayer?.Play();
            IsPlaying = true;
        }

        public void Stop() {
            _soundPlayer?.Stop();
            IsPlaying = false;
        }

        public void Pause() {
            if (!IsPlaying || IsPaused) {
                return;
            }
            _soundPlayer?.Pause();
            IsPaused = true;
        }

        public TimeSpan CurrentTime {
            get { return _waveStream.CurrentTime; }
            set {
                var waveStream = _waveStream;
                waveStream.CurrentTime = value;
                var position = waveStream.Position;
                var blockAlign = waveStream.BlockAlign;
                if (position % blockAlign != 0) {
                    position = (long)(Math.Round(position / (double)blockAlign) * blockAlign);
                    if (position < 0) {
                        position = 0;
                    }
                    waveStream.Position = position;
                }
            }
        }

        public Stream SourceStream => _sourceStream;

        public TimeSpan TotalLength => _totalLength;

        public string HcaName => _internalName;

        public bool IsPlaying {
            get {
                lock (_syncObject) {
                    return _isPlaying;
                }
            }
            private set {
                lock (_syncObject) {
                    _isPlaying = value;
                }
            }
        }

        public bool IsPaused {
            get {
                lock (_syncObject) {
                    return _isPaused;
                }
            }
            private set {
                lock (_syncObject) {
                    _isPaused = value;
                }
            }
        }

        public static bool Check(string acbFileName, string hcaName) {
            using (var fileStream = File.Open(acbFileName, FileMode.Open, FileAccess.Read)) {
                return Check(fileStream, acbFileName, hcaName);
            }
        }

        public static bool Check(Stream stream, string acbFileName, string hcaName) {
            try {
                using (var acb = AcbFile.FromStream(stream, acbFileName, false)) {
                    if (!acb.FileExists(hcaName)) {
                        return false;
                    }
                    try {
                        using (var acbStream = acb.OpenDataStream(hcaName)) {
                            return HcaDecoder.IsHcaStream(acbStream);
                        }
                    } catch (HcaException) {
                        return false;
                    }
                }
            } catch (Exception) {
                return false;
            }
        }

        protected override void Dispose(bool disposing) {
            _soundPlayer?.Stop();
            _soundPlayer?.Dispose();
            _waveStream?.Dispose();
            _hcaDataStream?.Dispose();
            _acb?.Dispose();
            _soundPlayer = null;
            _waveStream = null;
            _hcaDataStream = null;
            _acb = null;
            _soundPlayer = null;
        }

        private LiveMusicPlayer(Stream acbStream, string acbFileName, DecodeParams decodeParams) {
            _acb = AcbFile.FromStream(acbStream, acbFileName, false);
            var names = _acb.GetFileNames();
            _internalName = names[0];
            _hcaDataStream = _acb.OpenDataStream(_internalName);
            var hcaWaveProvider = new HcaWaveProvider(_hcaDataStream, decodeParams);
            _waveStream = new RawSourceWaveStream(hcaWaveProvider, hcaWaveProvider.WaveFormat);
            _soundPlayer = new AudioOut(AudioClientShareMode.Shared, 40);
            //_soundPlayer = new AudioOut();
            _soundPlayer.Init(_waveStream);
            _sourceStream = acbStream;
            _isPlaying = false;
            _syncObject = new object();
            var lengthInSeconds = (float)_waveStream.TotalTime.TotalSeconds;
            _totalLength = float.IsPositiveInfinity(lengthInSeconds) ? TimeSpan.MaxValue : TimeSpan.FromSeconds(lengthInSeconds);
        }

        private LiveMusicPlayer(Stream hcaStream, DecodeParams decodeParams) {
            var hcaWaveProvider = new HcaWaveProvider(_hcaDataStream, decodeParams);
            _waveStream = new RawSourceWaveStream(hcaWaveProvider, hcaWaveProvider.WaveFormat);
            _soundPlayer = new AudioOut(AudioClientShareMode.Shared, 40);
            //_soundPlayer = new AudioOut();
            _soundPlayer.Init(_waveStream);
            _sourceStream = hcaStream;
            _isPlaying = false;
            _syncObject = new object();
            var lengthInSeconds = (float)_waveStream.TotalTime.TotalSeconds;
            _totalLength = float.IsPositiveInfinity(lengthInSeconds) ? TimeSpan.MaxValue : TimeSpan.FromSeconds(lengthInSeconds);
        }

        private LiveMusicPlayer(Stream waveStream) {
            _waveStream = new WaveFileReader(waveStream);
            // See DirectSoundOut.cs in NAudio. Use 40 for WasapiOut to compsentate the latency in DirectSoundOut.
            _soundPlayer = new AudioOut(AudioClientShareMode.Shared, 40);
            //_soundPlayer = new AudioOut();
            _soundPlayer.Init(_waveStream);
            _sourceStream = waveStream;
            _isPlaying = false;
            _syncObject = new object();
            var lengthInSeconds = (float)_waveStream.TotalTime.TotalSeconds;
            _totalLength = float.IsPositiveInfinity(lengthInSeconds) ? TimeSpan.MaxValue : TimeSpan.FromSeconds(lengthInSeconds);
        }

        private readonly Stream _sourceStream;
        private AcbFile _acb;
        private Stream _hcaDataStream;
        private WaveStream _waveStream;
        private AudioOut _soundPlayer;
        private bool _isPlaying;
        private bool _isPaused;
        private readonly object _syncObject;
        private readonly TimeSpan _totalLength;
        private readonly string _internalName;

    }
}
