using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DereTore.Applications.StarlightDirector.Extensions;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.UI.Controls.Primitives;
using System.Linq;
using DereTore.Applications.StarlightDirector.UI.Controls.Models;
using DereTore.Applications.StarlightDirector.UI.Windows;
using LineTuple = System.Tuple<int, double, double, double, double>;

namespace DereTore.Applications.StarlightDirector.UI.Controls
{
    /// <summary>
    /// Interaction logic for ScorePreviewer.xaml
    /// </summary>
    public partial class ScorePreviewer
    {

        // used by frame update thread
        private Score _score;
        private volatile bool _isPreviewing;
        private Task _task;
        private readonly List<DrawingNote> _notes = new List<DrawingNote>();
        private double _targetFps;
        private int _startTime;

        // window-related
        private readonly MainWindow _window;
        private bool _shouldPlayMusic;

        public ScorePreviewer()
        {
            InitializeComponent();
            _window = Application.Current.MainWindow as MainWindow;
        }

        public void BeginPreview(Score score, double targetFps, int startTime, double approachTime)
        {
            // setup parameters

            _score = score;
            _isPreviewing = true;
            _targetFps = targetFps;
            _startTime = startTime;

            // prepare notes

            foreach (var note in _score.Notes)
            {
                // can I draw it?
                if (note.Type != NoteType.TapOrFlick && note.Type != NoteType.Hold)
                {
                    continue;
                }
                var pos = (int)note.FinishPosition;
                if (pos == 0)
                    pos = (int)note.StartPosition;

                if (pos == 0)
                    continue;

                var snote = new DrawingNote
                {
                    Note = note,
                    Done = false,
                    Duration = 0,
                    IsHoldStart = note.IsHoldStart,
                    Timing = (int) (note.HitTiming*1000),
                    LastT = 0,
                    HitPosition = pos - 1,
                    DrawType = (note.IsTap && !note.IsHoldEnd) || note.IsFlick ? (int)note.FlickType : 3
                };

                if (note.IsHoldStart)
                {
                    snote.Duration = (int) (note.HoldTarget.HitTiming*1000) - (int) (note.HitTiming*1000);
                }

                _notes.Add(snote);
            }

            _notes.Sort((a, b) => a.Timing - b.Timing);

            // prepare note relationships

            foreach (var snote in _notes)
            {
                if (snote.IsHoldStart)
                {
                    snote.HoldTarget = _notes.FirstOrDefault(note => note.Note.ID == snote.Note.HoldTargetID);
                }

                if (snote.Note.HasNextSync)
                {
                    snote.SyncTarget = _notes.FirstOrDefault(note => note.Note.ID == snote.Note.NextSyncTarget.ID);
                }

                if (snote.Note.HasNextFlick)
                {
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

            if (_shouldPlayMusic)
            {
                StartMusic(_startTime);
            }

            _task = new Task(DrawPreviewFrame);
            _task.Start();
        }

        public void EndPreview()
        {
            if (_shouldPlayMusic)
            {
                StopMusic();
            }

            _isPreviewing = false;
        }

        // These methods invokes the main thread and perform the tasks
        #region Multithreading Invoke

        private void StartMusic(double milliseconds)
        {
            Dispatcher.Invoke(new Action(() => _window.PlayMusic(milliseconds)));
        }

        private void StopMusic()
        {
            Dispatcher.Invoke(new Action(() => _window.StopMusic()));
        }

        #endregion

        /// <summary>
        /// Running in a background thread, refresh the locations of notes periodically. It tries to keep the target frame rate.
        /// </summary>
        private void DrawPreviewFrame()
        {
            // frame rate
            var targetFrameTime = 1000 / _targetFps;
            MainCanvas.HitEffectFrames = (int) (_targetFps / 5); // effect lasts for 0.2 second

            // drawing and timing
            var startTime = DateTime.UtcNow;

            while (true)
            {
                if (!_isPreviewing)
                {
                    MainCanvas.Stop();
                    _notes.Clear();
                    return;
                }

                var frameStartTime = DateTime.UtcNow;

                // compute time

                int songTime;
                if (_shouldPlayMusic)
                {
                    songTime = (int)_window.MusicTime().TotalMilliseconds;
                }
                else
                {
                    songTime = (int)(frameStartTime - startTime).TotalMilliseconds + _startTime;
                }

                // wait for rendering

                MainCanvas.RenderFrameBlocked(songTime);

                // wait for next frame

                var frameEllapsedTime = (DateTime.UtcNow - frameStartTime).TotalMilliseconds;
                if (frameEllapsedTime < targetFrameTime)
                {
                    Thread.Sleep((int) (targetFrameTime - frameEllapsedTime));
                }
                else
                {
                    Debug.WriteLine($"[Warning] Frame ellapsed time {frameEllapsedTime:N2} exceeds target.");
                }
            }
        }
    }
}
