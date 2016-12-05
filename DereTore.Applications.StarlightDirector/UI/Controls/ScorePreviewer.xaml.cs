using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DereTore.Applications.StarlightDirector.Entities;
using System.Linq;
using DereTore.Applications.StarlightDirector.UI.Controls.Models;
using DereTore.Applications.StarlightDirector.UI.Windows;

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
                if (snote.Timing + snote.Duration < startTime - approachTime)
                {
                    continue;
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

            // fix start time

            _startTime -= (int)approachTime;
            if (_startTime < 0)
                _startTime = 0;

            // prepare canvas

            MainCanvas.Initialize(_notes, approachTime);

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
            var musicTotalTime = _window.GetMusicTotalTime();

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
            if (!_loaded && ActualWidth > 0 && ActualHeight > 0)
            {
                _loaded = true;
            }
        }
    }
}
