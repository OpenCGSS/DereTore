using System;
using System.Collections.Generic;
using DereTore.Apps.ScoreViewer.Extensions;
using DereTore.Common;
using NAudio.Wave;
using SharpAL;
using SharpAL.Extensions;
using SharpAL.OpenAL;

namespace DereTore.Apps.ScoreViewer {
    public sealed class SfxManager : DisposableBase {

        public SfxManager(AudioManager audioManager) {
            _syncObject = new object();
            _items = new List<SfxItem>(30);
            _fileNames = new List<string>(30);
            _audioManager = audioManager;
        }

        public void PreloadWave(string fileName) {
            if (_fileNames.Contains(fileName)) {
                return;
            }

            _fileNames.Add(fileName);
            var sfx = LoadSfx(fileName);
            _items.Add(sfx);
        }

        public void PlayWave(string fileName, float volume) {
            var sfx = GetFreeSfx(fileName);
            sfx.AudioSource.Volume = volume;
            sfx.AudioSource.Play();
        }

        public void StopAll() {
            foreach (var item in _items) {
                if (item.IsPlaying) {
                    item.AudioSource.Stop();
                }
            }
        }

        public TimeSpan BufferSize { get; set; } = new TimeSpan(0, 0, 0, 0, 80);

        public TimeSpan BufferOffset => BufferSize - PlayerSettings.SfxOffset;

        protected override void Dispose(bool disposing) {
            StopAll();
            foreach (var item in _items) {
                item.Dispose();
            }
        }

        private SfxItem GetFreeSfx(string fileName) {
            var requireFreshReload = !_fileNames.Contains(fileName);

            if (requireFreshReload) {
                _fileNames.Add(fileName);
            }

            SfxItem sfx;

            if (!requireFreshReload) {
                SfxItem template = null;

                foreach (var item in _items) {
                    if (item.FileName != fileName) {
                        continue;
                    }

                    template = item;

                    if (!item.IsPlaying) {
                        return item;
                    }
                }

                if (template != null) {
                    sfx = template.Extend();

                    lock (_syncObject) {
                        _items.Add(sfx);
                    }

                    return sfx;
                }
            }

            sfx = LoadSfx(fileName);
            lock (_syncObject) {
                _items.Add(sfx);
            }

            return sfx;
        }

        private SfxItem LoadSfx(string fileName) {
            if (!_fileNames.Contains(fileName)) {
                _fileNames.Add(fileName);
            }

            var sfx = new SfxItem();

            byte[] data;
            int sampleRate;
            TimeSpan totalTime;

            using (var waveReader = new WaveFileReader(fileName)) {
                sampleRate = waveReader.WaveFormat.SampleRate;
                totalTime = waveReader.TotalTime;

                WaveStream waveStream;

                if (AudioManager.NeedsConversion(waveReader.WaveFormat, AudioManager.StandardFormat)) {
                    waveStream = new WaveFormatConversionStream(AudioManager.StandardFormat, waveReader);
                } else {
                    waveStream = waveReader;
                }

                using (var offsetStream = new WaveOffsetStream(waveStream)) {
                    offsetStream.StartTime = PlayerSettings.SfxOffset;
                    data = offsetStream.ReadToEnd();
                }
            }

            sfx.AudioBuffer = new AudioBuffer(_audioManager.AudioContext);
            sfx.AudioSource = new AudioSource(_audioManager.AudioContext);
            sfx.AudioBuffer.BufferData(data, sampleRate);
            sfx.AudioSource.Bind(sfx.AudioBuffer);
            sfx.Data = data;
            sfx.FileName = fileName;
            sfx.SampleRate = sampleRate;
            sfx.TotalTime = totalTime;

            return sfx;
        }

        private sealed class SfxItem : IDisposable {

            public string FileName;

            /// <summary>
            /// Transformed data, 16-bit stereo 44.1 kHz.
            /// </summary>
            public byte[] Data;

            public int SampleRate;

            public AudioSource AudioSource;

            public AudioBuffer AudioBuffer;

            public TimeSpan TotalTime;

            public bool IsPlaying => AudioSource.State == ALSourceState.Playing;

            public SfxItem Extend() {
                var sfx = new SfxItem {
                    FileName = FileName,
                    Data = Data,
                    SampleRate = SampleRate,
                    TotalTime = TotalTime
                };

                sfx.AudioSource = new AudioSource(AudioSource.Context);
                sfx.AudioBuffer = new AudioBuffer(AudioBuffer.Context);
                sfx.AudioBuffer.BufferData(sfx.Data, sfx.SampleRate);
                sfx.AudioSource.Bind(sfx.AudioBuffer);

                return sfx;
            }

            public void Dispose() {
                AudioSource?.Bind(null);
                AudioSource?.Dispose();
                AudioBuffer?.Dispose();
                AudioSource = null;
                AudioBuffer = null;
            }

        }

        private readonly AudioManager _audioManager;
        private readonly List<SfxItem> _items;
        private readonly List<string> _fileNames;

        private readonly object _syncObject;

    }
}
