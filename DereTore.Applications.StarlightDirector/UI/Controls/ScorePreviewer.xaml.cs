using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DereTore.Applications.StarlightDirector.Entities;
using System.Linq;
using DereTore.Applications.StarlightDirector.UI.Controls.Models;
using DereTore.Applications.StarlightDirector.UI.Windows;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    /// <summary>
    /// Interaction logic for ScorePreviewer.xaml
    /// </summary>
    public partial class ScorePreviewer {

        // first-time loading
        private bool _loaded;
        private EventWaitHandle _loadHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        // used by frame update thread
        private Score _score;
        private Task _task;
        private readonly List<DrawingNote> _notes = new List<DrawingNote>();
        private double _targetFps;
        private int _startTime;
        private volatile bool _isPreviewing;

        // hit sound
        private const string HitSoundFile = "hitsound.wav";
        private const string FlickSoundFile = "flicksound.wav";
        private WasapiOut[] _hitAudios = {null, null};
        private DateTime[] _hitModifiedTimes = {new DateTime(), new DateTime()};
        private WaveFileReader[] _hitWaves = {null, null};
        private MemoryStream[] _hitMem = {null, null};
        private EventWaitHandle _hitsoundHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        // music time fixing
        private int _lastMusicTime;
        private int _lastComputedSongTime;
        private DateTime _lastFrameEndtime;

        // window-related
        private readonly MainWindow _window;
        private bool _shouldPlayMusic;

        public ScorePreviewer() {
            InitializeComponent();
            _window = Application.Current.MainWindow as MainWindow;
        }

        public void BeginPreview(Score score, double targetFps, int startTime, double approachTime) {
            // wait if not loaded yet
            if (!_loaded)
            {
                new Task(() =>
                {
                    _loadHandle.WaitOne();
                    Dispatcher.BeginInvoke(
                        new Action<Score, double, int, double>(BeginPreview), 
                        score, targetFps, startTime, approachTime);
                }).Start();
                return;
            }

            // setup parameters

            _score = score;
            _targetFps = targetFps;
            _startTime = startTime;

            // prepare notes

            foreach (var note in _score.Notes) {
                // can I draw it?
                if (note.Type != NoteType.TapOrFlick && note.Type != NoteType.Hold) {
                    continue;
                }

                var pos = (int)note.FinishPosition;
                if (pos == 0)
                    pos = (int)note.StartPosition;

                if (pos == 0)
                    continue;

                var snote = new DrawingNote {
                    Note = note,
                    Done = false,
                    Duration = 0,
                    IsHoldStart = note.IsHoldStart,
                    Timing = (int)(note.HitTiming * 1000),
                    LastT = 0,
                    HitPosition = pos - 1,
                    DrawType = (note.IsTap && !note.IsHoldEnd) || note.IsFlick ? (int)note.FlickType : 3
                };

                if (note.IsHoldStart) {
                    snote.Duration = (int)(note.HoldTarget.HitTiming * 1000) - (int)(note.HitTiming * 1000);
                }

                // skip notes that are done before start time
                if (snote.Timing + snote.Duration < startTime)
                {
                    continue;
                }

                // skip hit effect of hold note start if started before
                if (snote.IsHoldStart && snote.Timing < startTime)
                {
                    snote.EffectShown = true;
                }

                _notes.Add(snote);
            }

            // end if no notes
            if (_notes.Count == 0)
            {
                // TODO: alert user?
                IsPreviewing = false;
                return;
            }

            _notes.Sort((a, b) => a.Timing - b.Timing);

            // prepare note relationships

            foreach (var snote in _notes) {
                if (snote.IsHoldStart) {
                    snote.HoldTarget = _notes.FirstOrDefault(note => note.Note.ID == snote.Note.HoldTargetID);
                }

                if (snote.Note.HasNextSync) {
                    snote.SyncTarget = _notes.FirstOrDefault(note => note.Note.ID == snote.Note.NextSyncTarget.ID);
                }

                if (snote.Note.HasNextFlick) {
                    snote.GroupTarget = _notes.FirstOrDefault(note => note.Note.ID == snote.Note.NextFlickNoteID);
                }
            }

            // music

            _shouldPlayMusic = _window != null && _window.MusicLoaded;

            // prepare canvas

            approachTime *= ActualHeight / 484.04;
            MainCanvas.Initialize(_notes, approachTime);

            // hit sound

            LoadHitSounds();
            new Task(HitSoundTask).Start();

            // go

            if (_shouldPlayMusic) {
                StartMusic(_startTime);
            }

            _task = new Task(DrawPreviewFrames);
            _task.Start();
        }

        public void EndPreview() {
            if (_shouldPlayMusic) {
                StopMusic();
            }

            IsPreviewing = false;
            _hitsoundHandle.Set();

            // ensure responding to dimension change
            _loaded = false;

            // clear canvas
            MainCanvas.InvalidateVisual();
        }

        // These methods invokes the main thread and perform the tasks
        #region Multithreading Invoke

        private void StartMusic(double milliseconds) {
            Dispatcher.Invoke(new Action(() => _window.PlayMusic(milliseconds)));
        }

        private void StopMusic() {
            Dispatcher.Invoke(new Action(() => _window.StopMusic()));
        }

        #endregion

        /// <summary>
        /// Running in a background thread, refresh the locations of notes periodically. It tries to keep the target frame rate.
        /// </summary>
        private void DrawPreviewFrames() {
            // frame rate
            double targetFrameTime = 0;
            if (!double.IsInfinity(_targetFps)) {
                targetFrameTime = 1000 / _targetFps;
            }

            MainCanvas.HitEffectMilliseconds = 200;

            // drawing and timing
            var startTime = DateTime.UtcNow;
            TimeSpan? musicCurrentTime = null;
            TimeSpan? musicTotalTime = null;
            if (_shouldPlayMusic)
                musicTotalTime = _window.GetMusicTotalTime();

            while (_isPreviewing) {
                if (_shouldPlayMusic) {
                    musicCurrentTime = _window.GetCurrentMusicTime();
                    if (musicCurrentTime.HasValue && musicTotalTime.HasValue && musicCurrentTime.Value >= musicTotalTime.Value) {
                        EndPreview();
                        break;
                    }
                }

                var frameStartTime = DateTime.UtcNow;

                // compute time

                int musicTimeInMillis;
                if (_shouldPlayMusic) {
                    if (musicCurrentTime == null) {
                        EndPreview();
                        break;
                    }

                    musicTimeInMillis = (int)musicCurrentTime.Value.TotalMilliseconds;
                    if (musicTimeInMillis > 0 && musicTimeInMillis == _lastMusicTime) {
                        // music time not updated, add frame time
                        _lastComputedSongTime += (int)(frameStartTime - _lastFrameEndtime).TotalMilliseconds;
                        musicTimeInMillis = _lastComputedSongTime;
                    } else {
                        // music time updated
                        _lastComputedSongTime = musicTimeInMillis;
                        _lastMusicTime = musicTimeInMillis;
                    }
                } else {
                    musicTimeInMillis = (int)(frameStartTime - startTime).TotalMilliseconds + _startTime;
                }

                // wait for rendering

                MainCanvas.RenderFrameBlocked(musicTimeInMillis);

                // play hitsound

                _hitsoundHandle.Set();

                // wait for next frame

                _lastFrameEndtime = DateTime.UtcNow;
                if (targetFrameTime > 0) {
                    var frameEllapsedTime = (_lastFrameEndtime - frameStartTime).TotalMilliseconds;
                    if (frameEllapsedTime < targetFrameTime) {
                        Thread.Sleep((int)(targetFrameTime - frameEllapsedTime));
                    } else {
                        Debug.WriteLine($"[Warning] Frame ellapsed time {frameEllapsedTime:N2} exceeds target.");
                    }
                }
            }

            MainCanvas.Stop();
            _notes.Clear();
        }

        private void ScorePreviewer_OnLayoutUpdated(object sender, EventArgs e)
        {
            if (_isPreviewing && !_loaded && ActualWidth > 0 && ActualHeight > 0)
            {
                _loaded = true;
                _loadHandle.Set();
            }
        }

        #region Hit Sound

        private MemoryStream MsFromFile(string file)
        {
            var ms = new MemoryStream();
            using (var fs = new FileStream(file, FileMode.Open))
            {
                fs.CopyTo(ms);
            }
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        private void SetHitsoundAudio(int index, MemoryStream ms)
        {
            _hitAudios[index]?.Stop();
            _hitAudios[index]?.Dispose();
            _hitWaves[index]?.Close();

            _hitMem[index] = ms;
            _hitWaves[index] = new WaveFileReader(ms);
            _hitAudios[index] = new WasapiOut(AudioClientShareMode.Shared, 0);
            _hitAudios[index].Init(_hitWaves[index]);
        }

        private void LoadHitSounds()
        {
            try
            {
                if (File.Exists(HitSoundFile) && _hitModifiedTimes[0] != File.GetLastWriteTime(HitSoundFile))
                {
                    SetHitsoundAudio(0, MsFromFile(HitSoundFile));
                }

                if (File.Exists(FlickSoundFile) && _hitModifiedTimes[1] != File.GetLastWriteTime(FlickSoundFile))
                {
                    SetHitsoundAudio(1, MsFromFile(FlickSoundFile));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load hitsounds:{Environment.NewLine}{ex.Message}","Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Runs in a separate thread to play hitsound
        /// </summary>
        private void HitSoundTask()
        {
            _hitsoundHandle.WaitOne();

            while (_isPreviewing)
            {
                for (int i = 0; i < 2; ++i)
                {
                    if (!MainCanvas.ShallPlaySoundEffect[i])
                        continue;

                    // note: no need to stop audio

                    if (_hitWaves[i] != null)
                    {
                        _hitWaves[i].Seek(0, SeekOrigin.Begin);
                        _hitAudios[i].Play();
                    }
                }

                _hitsoundHandle.WaitOne();
            }
        }

        #endregion
    }
}
