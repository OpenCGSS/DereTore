using System;
using System.IO;
using DereTore.ACB;
using DereTore.HCA;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace DereTore.Applications.ScoreEditor {
    public sealed class LiveMusicWaveStream : WaveStream {

        public static LiveMusicWaveStream FromAcbStream(Stream stream, string acbFileName) {
            return new LiveMusicWaveStream(stream, acbFileName, DecodeParams.Default);
        }

        public static LiveMusicWaveStream FromAcbStream(Stream stream, string acbFileName, DecodeParams decodeParams) {
            return new LiveMusicWaveStream(stream, acbFileName, decodeParams);
        }

        public static LiveMusicWaveStream FromWaveStream(Stream stream) {
            return new LiveMusicWaveStream(stream);
        }

        public static LiveMusicWaveStream FromHcaStream(Stream stream) {
            return new LiveMusicWaveStream(stream, DecodeParams.Default);
        }

        public static LiveMusicWaveStream FromHcaStream(Stream stream, DecodeParams decodeParams) {
            return new LiveMusicWaveStream(stream, decodeParams);
        }

        public override TimeSpan CurrentTime {
            get { return _waveStream.CurrentTime; }
            set {
                lock (_syncObject) {
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
        }

        public Stream SourceStream => _sourceStream;

        public override TimeSpan TotalTime => _waveStream.TotalTime;

        public override long Length => _waveStream.Length;

        public override long Position {
            get { return _waveStream.Position; }
            set {
                lock (_syncObject) {
                    var waveStream = _waveStream;
                    var position = value;
                    var blockAlign = waveStream.BlockAlign;
                    if (position % blockAlign != 0) {
                        position = (long)(Math.Round(position / (double)blockAlign) * blockAlign);
                        if (position < 0) {
                            position = 0;
                        }
                    }
                    waveStream.Position = position;
                }
            }
        }

        public override WaveFormat WaveFormat => _waveStream.WaveFormat;

        public override int Read(byte[] buffer, int offset, int count) {
            lock (_syncObject) {
                return _waveStream.Read(buffer, offset, count);
            }
        }

        public string HcaName => _internalName;

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
            if (disposing) {
                _waveStream?.Dispose();
                _hcaDataStream?.Dispose();
                _acb?.Dispose();
                _waveStream = null;
                _hcaDataStream = null;
                _acb = null;
            }
            base.Dispose(disposing);
            _disposed = true;
        }

        private LiveMusicWaveStream(Stream acbStream, string acbFileName, DecodeParams decodeParams) {
            _acb = AcbFile.FromStream(acbStream, acbFileName, false);
            var names = _acb.GetFileNames();
            _internalName = names[0];
            _hcaDataStream = _acb.OpenDataStream(_internalName);
            var hcaWaveProvider = new HcaWaveProvider(_hcaDataStream, decodeParams);
            _waveStream = new RawSourceWaveStream(hcaWaveProvider, hcaWaveProvider.WaveFormat);
            _sourceStream = acbStream;
            _syncObject = new object();
        }

        private LiveMusicWaveStream(Stream hcaStream, DecodeParams decodeParams) {
            var hcaWaveProvider = new HcaWaveProvider(hcaStream, decodeParams);
            _waveStream = new RawSourceWaveStream(hcaWaveProvider, hcaWaveProvider.WaveFormat);
            _sourceStream = hcaStream;
            _syncObject = new object();
        }

        private LiveMusicWaveStream(Stream waveStream) {
            _waveStream = new WaveFileReader(waveStream);
            _sourceStream = waveStream;
            _syncObject = new object();
        }

        private bool _disposed = false;

        private readonly Stream _sourceStream;
        private AcbFile _acb;
        private Stream _hcaDataStream;
        private WaveStream _waveStream;
        private readonly object _syncObject;
        private readonly string _internalName;

    }
}
