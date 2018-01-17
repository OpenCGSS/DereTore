using System;
using System.Timers;
using DereTore.Apps.ScoreViewer.Extensions;
using DereTore.Common;
using NAudio.Wave;
using SharpAL;
using SharpAL.Extensions;
using SharpAL.OpenAL;

namespace DereTore.Apps.ScoreViewer {
    public sealed class ScorePlayer : DisposableBase {

        public ScorePlayer(AudioManager audioManager) {
            _audioManager = audioManager;
            _syncObject = new object();
            _audioSource = new AudioSource(audioManager.AudioContext);
            _audioBuffer = new AudioBuffer(audioManager.AudioContext);
            PlayerSettings.MusicVolumeChanged += OnMusicVolumeChanged;
            _timer = new Timer(15);
            _timer.Start();
            _timer.Elapsed += Timer_Tick;
        }

        public event EventHandler<EventArgs> PlaybackStopped;

        public event EventHandler<EventArgs> PositionChanged;

        public void Play() {
            if (IsPlaying) {
                if (IsPaused) {
                    IsPaused = false;
                } else {
                    Stop();
                }
            }
            _audioSource?.Play();
            IsPlaying = true;
        }

        public void Stop() {
            _audioSource?.Stop();
            IsPlaying = false;
        }

        public void Pause() {
            if (!IsPlaying || IsPaused) {
                return;
            }
            _audioSource?.Pause();
            IsPaused = true;
        }

        public TimeSpan CurrentTime {
            get { return _audioSource.CurrentTime; }
            set {
                var cur = _audioSource.CurrentTime;

                if (cur == value) {
                    return;
                }

                lock (_syncObject) {
                    _audioSource.CurrentTime = value;
                }

                PositionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

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

        public void LoadStream(WaveStream stream) {
            WaveStream transformedStream;

            if (AudioManager.NeedsConversion(stream.WaveFormat, AudioManager.StandardFormat)) {
                transformedStream = new WaveFormatConversionStream(AudioManager.StandardFormat, stream);
            } else {
                transformedStream = stream;
            }

            var allData = transformedStream.ReadToEnd();
            _audioBuffer.BufferData(allData, stream.WaveFormat.SampleRate);
            _audioSource.Bind(_audioBuffer);
        }

        protected override void Dispose(bool disposing) {
            PlayerSettings.MusicVolumeChanged -= OnMusicVolumeChanged;
            _timer.Stop();
            _timer.Dispose();

            Stop();

            _audioSource?.Bind(null);
            _audioSource?.Dispose();
            _audioBuffer?.Dispose();

            _audioSource = null;
            _audioBuffer = null;

            _timer.Elapsed -= Timer_Tick;
        }

        private void Timer_Tick(object sender, ElapsedEventArgs e) {
            var state = _audioSource.State;

            if ((_lastSourceState == ALSourceState.Paused || _lastSourceState == ALSourceState.Playing) && state == ALSourceState.Stopped) {
                PlaybackStopped?.Invoke(this, EventArgs.Empty);
            }

            _lastSourceState = state;
        }

        private void OnMusicVolumeChanged(object sender, EventArgs e) {
            _audioSource.Volume = PlayerSettings.MusicVolume;
        }

        private bool _isPlaying;
        private bool _isPaused;

        private AudioSource _audioSource;
        private AudioBuffer _audioBuffer;
        private readonly AudioManager _audioManager;
        private readonly object _syncObject;

        private ALSourceState _lastSourceState = ALSourceState.Initial;

        private Timer _timer;

    }
}
