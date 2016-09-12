using System.Collections.Generic;
using System.IO;
using DereTore.HCA;
using DereTore.StarlightStage;
using AudioOut = NAudio.Wave.DirectSoundOut;

namespace DereTore.Applications.ScoreEditor {
    public sealed class SoundManager : DisposableBase {

        static SoundManager() {
            SyncObject = new object();
        }

        public static SoundManager Instance {
            get {
                lock (SyncObject) {
                    if (_instance == null) {
                        _instance = new SoundManager();
                    }
                }
                return _instance;
            }
        }

        public void PreloadHca(string fileName) {
            int index;
            var @out = GetFreeOutput(fileName, out index) ?? CreateOutput(fileName, out index);
        }

        public void PlayHca(string fileName) {
            if (IsUserSeeking) {
                return;
            }
            int index;
            var @out = GetFreeOutput(fileName, out index) ?? CreateOutput(fileName, out index);
            _hcaWaveProviders[index].Seek(0, SeekOrigin.Begin);
            _playingList[index] = true;
            @out.Play();
        }

        public void ClearCache() {
            DisposeInternal();
            _soundStreams.Clear();
            _hcaWaveProviders.Clear();
            _fileNames.Clear();
            _audioOuts.Clear();
            _playingList.Clear();
        }

        public bool IsUserSeeking { get; set; }

        protected override void Dispose(bool disposing) {
            DisposeInternal();
        }

        private void DisposeInternal() {
            foreach (var audioOut in _audioOuts) {
                audioOut.Stop();
                audioOut.Dispose();
            }
            foreach (var hcaWaveProvider in _hcaWaveProviders) {
                hcaWaveProvider.Dispose();
            }
            foreach (var memoryStream in _soundStreams) {
                memoryStream.Dispose();
            }
        }

        private AudioOut GetFreeOutput(string fileName, out int index) {
            if (!_fileNames.Contains(fileName)) {
                index = -1;
                return null;
            }
            for (var i = 0; i < _audioOuts.Count; ++i) {
                if (_fileNames[i] == fileName && !_playingList[i]) {
                    index = i;
                    return _audioOuts[i];
                }
            }
            index = -1;
            return null;
        }

        private AudioOut CreateOutput(string fileName, out int index) {
            MemoryStream templateMemory = null;
            if (_fileNames.Contains(fileName)) {
                for (var i = 0; i < _soundStreams.Count; ++i) {
                    if (_fileNames[i] == fileName) {
                        templateMemory = _soundStreams[i];
                        break;
                    }
                }
            }

            _fileNames.Add(fileName);
            MemoryStream memory;
            if (templateMemory != null) {
                memory = new MemoryStream(templateMemory.Capacity);
                templateMemory.WriteTo(memory);
            } else {
                using (var fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    memory = new MemoryStream((int)fs.Length);
                    fs.CopyTo(memory);
                }
            }
            memory.Seek(0, SeekOrigin.Begin);
            memory.Capacity = (int)memory.Length;
            _soundStreams.Add(memory);

            var waveProvider = new HcaWaveProvider(memory, new DecodeParams {
                Key1 = CgssCipher.Key1,
                Key2 = CgssCipher.Key2
            }) {
                AllowDisposedOperations = true // In case of closing the form when sound effects are still playing
            };
            _hcaWaveProviders.Add(waveProvider);
            _playingList.Add(false);
            var @out = new AudioOut();
            _audioOuts.Add(@out);
            index = _audioOuts.Count - 1;
            var index2 = index;
            @out.PlaybackStopped += (s, e) => _playingList[index2] = false;
            @out.Init(waveProvider);
            return @out;
        }

        private SoundManager() {
            _soundStreams = new List<MemoryStream>();
            _fileNames = new List<string>();
            _hcaWaveProviders = new List<HcaWaveProvider>();
            _audioOuts = new List<AudioOut>();
            _playingList = new List<bool>();
        }

        private readonly List<MemoryStream> _soundStreams;
        private readonly List<HcaWaveProvider> _hcaWaveProviders;
        private readonly List<string> _fileNames;
        private readonly List<AudioOut> _audioOuts;
        private readonly List<bool> _playingList;

        private static SoundManager _instance;
        private static readonly object SyncObject;

    }
}
