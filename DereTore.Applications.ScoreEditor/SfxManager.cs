using System;
using System.Collections.Generic;
using System.IO;
using DereTore.Applications.ScoreEditor.Controls;
using NAudio.Wave;
using Timer = System.Timers.Timer;

namespace DereTore.Applications.ScoreEditor {
    public sealed class SfxManager : DisposableBase {

        public WaveOffsetStream PreloadWave(string fileName) {
            return PreloadWave(null, fileName);
        }

        public WaveOffsetStream PreloadWave(Stream dataStream, string fileName) {
            int index;
            var @out = GetFreeStream(fileName, TimeSpan.Zero, out index)
                ?? (dataStream != null ? CreateStream(dataStream, fileName, TimeSpan.Zero, out index) : CreateStream(fileName, TimeSpan.Zero, out index));
            return @out;
        }

        public void PlayWave(string fileName, TimeSpan startTime, float volume = 1f) {
            PlayWave(null, fileName, startTime, volume);
        }

        public void PlayWave(Stream dataStream, string fileName, TimeSpan startTime, float volume) {
            int index;
            TimeSpan correctedStartTime = startTime + PlayerSettings.SfxOffset;
            var @out = GetFreeStream(fileName, correctedStartTime, out index)
                ?? (dataStream != null ? CreateStream(dataStream, fileName, correctedStartTime, out index) : CreateStreamForceUsingCache(fileName, correctedStartTime, out index));
            @out.Seek(0, SeekOrigin.Begin);
            _playingList[index] = true;
            _mixerInputWaveStreams[index] = _scorePlayer?.AddInputStream(@out, volume);
        }

        public void StopAll() {
            for (int i = 0; i < _waveOffsetStreams.Count; ++i) {
                if (_playingList[i]) {
                    _scorePlayer.RemoveInputStream(_mixerInputWaveStreams[i]);
                    _mixerInputWaveStreams[i] = null;
                    _playingList[i] = false;
                }
            }
        }

        public void ClearCache() {
            DisposeInternal();
            _soundStreams.Clear();
            _waveStreams.Clear();
            _fileNames.Clear();
            _waveOffsetStreams.Clear();
            _mixerInputWaveStreams.Clear();
            _playingList.Clear();
        }

        public TimeSpan BufferSize { get; set; } = new TimeSpan(0, 0, 0, 0, 80);


        public TimeSpan BufferOffset => BufferSize - PlayerSettings.SfxOffset;

        protected override void Dispose(bool disposing) {
            if (disposing) {
                _timer.Elapsed -= Timer_Tick;
                _timer.Stop();
                _timer.Dispose();
                DisposeInternal();
            }
        }

        private void DisposeInternal() {
            StopAll();
            foreach (var stream in _waveOffsetStreams) {
                stream.Dispose();
            }
            foreach (var hcaWaveProvider in _waveStreams) {
                hcaWaveProvider.Dispose();
            }
            foreach (var memoryStream in _soundStreams) {
                memoryStream.Dispose();
            }
        }

        private WaveOffsetStream GetFreeStream(string fileName, TimeSpan startTime, out int index) {
            if (!_fileNames.Contains(fileName)) {
                index = -1;
                return null;
            }
            for (var i = 0; i < _waveOffsetStreams.Count; ++i) {
                if (_fileNames[i] == fileName && !_playingList[i]) {
                    index = i;
                    var waveOffsetStream = _waveOffsetStreams[i];
                    waveOffsetStream.StartTime = startTime;
                    waveOffsetStream.CurrentTime = startTime;
                    return waveOffsetStream;
                }
            }
            index = -1;
            return null;
        }

        private WaveOffsetStream CreateStream(Stream dataStream, string fileName, TimeSpan startTime, out int index) {
            var fileNames = _fileNames;
            var soundStreams = _soundStreams;
            MemoryStream templateMemory = null;
            if (fileNames.Contains(fileName)) {
                for (var i = 0; i < fileNames.Count; ++i) {
                    if (fileNames[i] == fileName) {
                        templateMemory = soundStreams[i];
                        break;
                    }
                }
            }

            fileNames.Add(fileName);
            MemoryStream memory;
            if (templateMemory != null) {
                memory = new MemoryStream(templateMemory.Capacity);
                templateMemory.WriteTo(memory);
            } else {
                if (dataStream == null) {
                    throw new ArgumentNullException(nameof(dataStream), "When not using a cache, the data stream must not be null.");
                }
                memory = new MemoryStream((int)dataStream.Length);
                dataStream.CopyTo(memory);
            }
            memory.Seek(0, SeekOrigin.Begin);
            memory.Capacity = (int)memory.Length;
            soundStreams.Add(memory);

            // The SFX files were provided so just keep it the 44.1kHz/16bits/stereo.
            var waveProvider = new RawSourceWaveStream(memory, DefaultWaveFormat);
            _waveStreams.Add(waveProvider);
            _playingList.Add(false);
            var waveOffsetStream = new WaveOffsetStream(waveProvider, startTime, TimeSpan.Zero, waveProvider.TotalTime);
            _waveOffsetStreams.Add(waveOffsetStream);
            _mixerInputWaveStreams.Add(null);
            index = _waveOffsetStreams.Count - 1;
            return waveOffsetStream;
        }

        private WaveOffsetStream CreateStream(string fileName, TimeSpan startTime, out int index) {
            using (var fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                return CreateStream(fs, fileName, startTime, out index);
            }
        }

        private WaveOffsetStream CreateStreamForceUsingCache(string fileName, TimeSpan startTime, out int index) {
            return CreateStream(null, fileName, startTime, out index);
        }

        private void Timer_Tick(object sender, EventArgs e) {
            lock (_syncObject) {
                for (var i = 0; i < _waveOffsetStreams.Count; i++) {
                    if (_playingList[i] && _waveOffsetStreams[i].Position >= _waveOffsetStreams[i].Length) {
                        _scorePlayer.RemoveInputStream(_mixerInputWaveStreams[i]);
                        _mixerInputWaveStreams[i] = null;
                        _playingList[i] = false;
                    }
                }
            }
        }

        public SfxManager(ScorePlayer scorePlayer) {
            _syncObject = new object();
            _soundStreams = new List<MemoryStream>();
            _fileNames = new List<string>();
            _waveStreams = new List<WaveStream>();
            _waveOffsetStreams = new List<WaveOffsetStream>();
            _mixerInputWaveStreams = new List<WaveStream>();
            _playingList = new List<bool>();
            _timer = new Timer(15);
            _scorePlayer = scorePlayer;
            _timer.Elapsed += Timer_Tick;
            _timer.Start();
        }

        private readonly ScorePlayer _scorePlayer;
        private readonly List<MemoryStream> _soundStreams;
        private readonly List<WaveStream> _waveStreams;
        private readonly List<string> _fileNames;
        private readonly List<WaveOffsetStream> _waveOffsetStreams;
        private readonly List<WaveStream> _mixerInputWaveStreams;
        private readonly List<bool> _playingList;

        private readonly object _syncObject;
        private static readonly WaveFormat DefaultWaveFormat = new WaveFormat();
        private readonly Timer _timer;

    }
}
