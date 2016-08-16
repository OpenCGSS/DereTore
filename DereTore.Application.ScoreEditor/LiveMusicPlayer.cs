using System;
using System.Diagnostics;
using System.IO;
using DereTore.ACB;
using DereTore.HCA;
using NAudio.CoreAudioApi;
using NAudio.Utils;
using AudioOut = NAudio.Wave.WasapiOut;

namespace DereTore.Application.ScoreEditor {
    public sealed class LiveMusicPlayer : DisposableBase {

        public static LiveMusicPlayer FromStream(Stream stream, string acbFileName) {
            return new LiveMusicPlayer(stream, acbFileName, DecodeParams.Default);
        }

        public static LiveMusicPlayer FromStream(Stream stream, string acbFileName, DecodeParams decodeParams) {
            return new LiveMusicPlayer(stream, acbFileName, decodeParams);
        }

        public void Play() {
            if (IsPlaying) {
                Stop();
            }
            _soundPlayer?.Play();
            _stopwatch.Start();
            IsPlaying = true;
        }

        public void Stop() {
            _soundPlayer?.Stop();
            _stopwatch.Reset();
            IsPlaying = false;
        }

        public TimeSpan Elapsed => _stopwatch.Elapsed;

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
            _hca?.Dispose();
            _hcaDataStream?.Dispose();
            _acb?.Dispose();
            _soundPlayer = null;
            _hca = null;
            _hcaDataStream = null;
            _acb = null;
            _soundPlayer = null;
            _stopwatch = null;
        }

        private LiveMusicPlayer(Stream stream, string acbFileName, DecodeParams decodeParams) {
            _acb = AcbFile.FromStream(stream, acbFileName, false);
            _acb.Initialize();
            var names = _acb.GetFileNames();
            _internalName = names[0];
            _hcaDataStream = _acb.OpenDataStream(_internalName);
            _hca = new HcaWaveProvider(_hcaDataStream, decodeParams);
            _soundPlayer = new AudioOut(AudioClientShareMode.Shared, 0);
            //_soundPlayer = new DirectSoundOut();
            _soundPlayer.Init(_hca);
            _sourceStream = stream;
            _stopwatch = new Stopwatch();
            _isPlaying = false;
            _syncObject = new object();
            _totalLength = TimeSpan.FromSeconds(_hca.LengthInSeconds);
        }

        private readonly Stream _sourceStream;
        private AcbFile _acb;
        private Stream _hcaDataStream;
        private HcaWaveProvider _hca;
        private AudioOut _soundPlayer;
        private bool _isPlaying;
        private Stopwatch _stopwatch;
        private readonly object _syncObject;
        private readonly TimeSpan _totalLength;
        private readonly string _internalName;

    }
}
