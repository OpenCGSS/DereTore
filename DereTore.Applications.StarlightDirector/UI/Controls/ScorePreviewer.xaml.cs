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
using DereTore.Applications.StarlightDirector.UI.Windows;
using LineTuple = System.Tuple<int, double, double, double, double>;

namespace DereTore.Applications.StarlightDirector.UI.Controls
{
    /// <summary>
    /// Interaction logic for ScorePreviewer.xaml
    /// </summary>
    public partial class ScorePreviewer
    {
        /// <summary>
        /// Internal representation of notes for drawing
        /// </summary>
        private class SimpleNote
        {
            public Note Note { get; set; }
            public SimpleNote HoldTarget { get; set; }
            public SimpleNote SyncTarget { get; set; }
            public SimpleNote GroupTarget { get; set; }
            public int NoteId { get; set; }
            public int Timing { get; set; }
            public bool Done { get; set; }
            public int Duration { get; set; }
            public bool IsHoldStart { get; set; }
            public double LastT { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public int DrawType { get; set; }
            public int HitPosition { get; set; }
        }

        private const double NoteMarginTop = 20;
        private const double NoteMarginBottom = 60;
        private const double NoteXBetween = 80;
        private const double NoteSize = 30;
        private const double NoteRadius = NoteSize / 2;

        // used by frame update thread
        private Score _score;
        private volatile bool _isPreviewing;
        private Task _task;
        private readonly List<SimpleNote> _notes = new List<SimpleNote>();
        private double _targetFps;
        private int _startTime;
        private double _approachTime;

        // screen positions
        private double _noteStartY;
        private double _noteEndY;
        private List<double> _noteX;
        
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
            _approachTime = approachTime;

            _noteX = new List<double>();
            for (int i = 0; i < 5; ++i)
                _noteX.Add(0);
            ComputePositions();

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

                var snote = new SimpleNote
                {
                    Note = note,
                    NoteId = note.ID,
                    Done = false,
                    Duration = 0,
                    IsHoldStart = note.IsHoldStart,
                    Timing = (int) (note.HitTiming*1000),
                    LastT = 0,
                    X = _noteX[pos - 1],
                    Y = _noteStartY,
                    HitPosition = pos - 1
                };

                snote.DrawType = note.Type == NoteType.Hold ? 3 : (int) note.FlickType;

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
                    snote.HoldTarget = _notes.FirstOrDefault(note => note.NoteId == snote.Note.HoldTargetID);
                }

                if (snote.Note.HasNextSync)
                {
                    snote.SyncTarget = _notes.FirstOrDefault(note => note.NoteId == snote.Note.NextSyncTarget.ID);
                }

                if (snote.Note.HasNextFlick)
                {
                    snote.GroupTarget = _notes.FirstOrDefault(note => note.NoteId == snote.Note.NextFlickNoteID);
                }
            }

            // music
            _shouldPlayMusic = ShouldPlayMusic();

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

        /// <summary>
        /// Compute where the notes should be according to current window size
        /// </summary>
        private void ComputePositions()
        {
            _noteStartY = NoteMarginTop;
            _noteEndY = MainCanvas.ActualHeight - NoteMarginBottom;
            _noteX[0] = (MainCanvas.ActualWidth - 4*NoteXBetween - 5*NoteSize)/2;
            for (int i = 1; i < 5; ++i)
            {
                _noteX[i] = _noteX[i - 1] + NoteXBetween + NoteSize;
            }
        }

        // These methods invokes the main thread and perform the tasks
        #region Multithreading Invoke

        private bool ShouldPlayMusic()
        {
            return (bool)Dispatcher.Invoke(new Func<bool>(() => _window != null && _window.MusicLoaded));
        }

        private void StartMusic(double milliseconds)
        {
            Dispatcher.Invoke(new Action(() => _window.PlayMusic(milliseconds)));
        }

        private void StopMusic()
        {
            Dispatcher.Invoke(new Action(() => _window.StopMusic()));
        }

        #endregion

        private void ComputeLine(SimpleNote note)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (note.LastT == 0 || note.Done)
                return;

            // Hold line
            if (note.IsHoldStart)
            {
                MainCanvas.Lines.Add(new LineTuple(0, note.X, note.Y, note.HoldTarget.X, note.HoldTarget.Y));
            }

            // Sync line
            // check LastT so that when HitNote arrives the line is gone
            if (note.SyncTarget != null && note.LastT < 1)
            {
                MainCanvas.Lines.Add(new LineTuple(1, note.X, note.Y, note.SyncTarget.X, note.SyncTarget.Y));
            }

            // Flicker line
            if (note.GroupTarget != null && note.LastT < 1)
            {
                MainCanvas.Lines.Add(new LineTuple(2, note.X, note.Y, note.GroupTarget.X, note.GroupTarget.Y));
            }
        }

        /// <summary>
        /// Running in a background thread, refresh the locations of notes periodically. It tries to keep the target frame rate.
        /// </summary>
        private void DrawPreviewFrame()
        {
            // computation parameters
            var targetFrameTime = 1000 / _targetFps;
            MainCanvas.HitEffectFrames = (int) (_targetFps / 5); // effect lasts for 0.2 second

            // fix start time
            _startTime -= (int)_approachTime;
            if (_startTime < 0)
                _startTime = 0;

            // drawing and timing
            var startTime = DateTime.UtcNow;
            var notesHead = 0;

            if (_shouldPlayMusic)
            {
                StartMusic(_startTime);
            }

            while (true)
            {
                if (!_isPreviewing)
                {
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

                // reset canvas

                MainCanvas.Notes.Clear();
                MainCanvas.Lines.Clear();

                // compute notes

                int computeStart = notesHead;
                int computeEnd = notesHead;
                bool headUpdated = false;
                for (int i = notesHead; i < _notes.Count; ++i)
                {
                    var note = _notes[i];

                    /*
                     * diff <  0 --- not shown
                     * diff == 0 --- begin to show
                     * diff == approachTime --- arrive at end
                     * diff >  approachTime --- ended
                     */
                    var diff = songTime - note.Timing + _approachTime;
                    if (diff < 0)
                        break;
                    if (note.Done)
                        continue;

                    computeEnd = i;

                    var t = diff/ _approachTime;
                    if (t > 1)
                        t = 1;

                    note.LastT = t;

                    // change this line if you want game-like approaching path
                    note.Y = _noteStartY + t*(_noteEndY - _noteStartY);

                    MainCanvas.Notes.Add(new Tuple<int, double, double>(note.DrawType, note.X, note.Y));

                    // note arrive at bottom
                    if (diff > _approachTime)
                    {
                        MainCanvas.NoteHit(note.HitPosition);

                        // Hit and flick notes end immediately
                        // Hold note heads end after its duration
                        if (!note.IsHoldStart || diff > _approachTime + note.Duration)
                        {
                            note.Done = true;
                        }                 
                    }

                    // update head to be the first note that is not done
                    if (!note.Done && !headUpdated)
                    {
                        notesHead = i;
                        headUpdated = true;
                    }
                }

                // compute lines

                for (int i = computeStart; i <= computeEnd; ++i)
                {
                    ComputeLine(_notes[i]);
                }

                // wait for rendering

                Dispatcher.Invoke(new Action(() => MainCanvas.InvalidateVisual()));
                MainCanvas.RenderCompleteHandle.WaitOne();

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
