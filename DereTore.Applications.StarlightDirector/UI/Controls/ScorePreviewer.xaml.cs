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
using LineTuple = System.Tuple<double, double, double, double>;

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
            public SimpleScoreNote ScoreNote { get; set; }
            public Line HoldLine { get; set; }
            public Line GroupLine { get; set; }
            public Line SyncLine { get; set; }
        }

        private const double NoteMarginTop = 20;
        private const double NoteMarginBottom = 60;
        private const double NoteXBetween = 80;
        private const double NoteSize = 30;
        private const double NoteRadius = NoteSize / 2;
        private const double HoldLineThickness = 16;
        private const double SyncLineThickness = 8;
        private const double GridLineThickness = 1;

        // used by frame update thread
        private Score _score;
        private volatile bool _isPreviewing;
        private Task _task;
        private readonly List<SimpleNote> _notes = new List<SimpleNote>();
        private double _targetFps;
        private int _startTime;
        private double _approachTime;

        // WPF new object performance is horrible, so we use pools to reuse them
        private readonly Queue<SimpleScoreNote> _scoreNotePool = new Queue<SimpleScoreNote>();
        private readonly Queue<Line> _linePool = new Queue<Line>();

        // screen positions
        private double _noteStartY;
        private double _noteEndY;
        private List<double> _noteX;
        private double _lineOffset;

        private readonly Brush _relationBrush = Application.Current.FindResource<Brush>(App.ResourceKeys.RelationBorderBrush);
        private readonly Brush _gridBrush = Brushes.DarkGray;

        // window-related
        private readonly MainWindow _window;
        private bool _shouldPlayMusic;

        // note hit effect!
        private int _noteHitCounter;
        private int _noteHitMax;
        private Line _effectedLine;

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
                    Y = _noteStartY
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

            // draw some grid lines
            DrawGrid();

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
            _noteEndY = PreviewCanvas.ActualHeight - NoteMarginBottom;
            _noteX[0] = (PreviewCanvas.ActualWidth - 4*NoteXBetween - 5*NoteSize)/2;
            for (int i = 1; i < 5; ++i)
            {
                _noteX[i] = _noteX[i - 1] + NoteXBetween + NoteSize;
            }

            _lineOffset = NoteRadius;
        }

        private void DrawGrid()
        {
            var lines = new List<LineTuple>();

            foreach (var x in _noteX)
            {
                lines.Add(new LineTuple(x, _noteStartY, x, _noteEndY));
            }
            lines.Add(new LineTuple(_noteX[0], _noteStartY, _noteX[4], _noteStartY));
            lines.Add(new LineTuple(_noteX[0], _noteEndY, _noteX[4], _noteEndY));
            lines.Add(new LineTuple(_noteX[0], _noteEndY, _noteX[4], _noteEndY));

            foreach (var line in lines)
            {
                // after the loop, _effectedLine will be the LAST line
                _effectedLine = new Line
                {
                    Stroke = _gridBrush,
                    StrokeThickness = GridLineThickness,
                    X1 = line.Item1 + _lineOffset,
                    Y1 = line.Item2 + _lineOffset,
                    X2 = line.Item3 + _lineOffset,
                    Y2 = line.Item4 + _lineOffset
                };
                LineCanvas.Children.Add(_effectedLine);
            }

            _effectedLine.Stroke = Brushes.Gold;
            _effectedLine.Opacity = 0;
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

        private readonly Action<UIElement, double, double> _setPositionInCanvasAction =
            (elem, x, y) =>
            {
                Canvas.SetLeft(elem, x);
                Canvas.SetTop(elem, y);
            };

        private SimpleScoreNote CreateScoreNote(SimpleNote note)
        {
            return Dispatcher.Invoke(new Func<SimpleScoreNote>(() =>
            {
                if (_scoreNotePool.Count == 0)
                {
                    var scoreNote = new SimpleScoreNote {Note = note.Note};
                    PreviewCanvas.Children.Add(scoreNote);
                    Canvas.SetTop(scoreNote, 0);
                    Canvas.SetLeft(scoreNote, 0);
                    return scoreNote;
                }
                else
                {
                    var scoreNote = _scoreNotePool.Dequeue();
                    scoreNote.Note = note.Note;
                    Canvas.SetTop(scoreNote, 0);
                    Canvas.SetLeft(scoreNote, 0);
                    scoreNote.Visibility = Visibility.Visible;
                    return scoreNote;
                }
            })) as SimpleScoreNote;
        }

        private void SetPositionInCanvas(UIElement elem, double x, double y)
        {
            Dispatcher.Invoke(_setPositionInCanvasAction, elem, x, y);
        }

        private void ReleaseScoreNote(SimpleScoreNote scoreNote)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                scoreNote.Visibility = Visibility.Collapsed;
                _scoreNotePool.Enqueue(scoreNote);
            }));
        }

        private void ReleaseLine(Line line)
        {
            if (line == null)
                return;

            Dispatcher.Invoke(new Action(() =>
            {
                line.Visibility = Visibility.Collapsed;
                _linePool.Enqueue(line);
            }));
        }

        private void ClearCanvases()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                PreviewCanvas.Children.Clear();
                LineCanvas.Children.Clear();
            }));
        }

        #endregion

        /// <summary>
        /// Helper used by DrawLines.
        /// </summary>
        private Line CreateLineOnCurrentThread(double thickness)
        {
            Line line;
            if (_linePool.Count == 0)
            {
                line = new Line
                {
                    Stroke = _relationBrush,
                    StrokeThickness = thickness
                };
                LineCanvas.Children.Add(line);
            }
            else
            {
                line = _linePool.Dequeue();
                line.Visibility = Visibility.Visible;
                line.StrokeThickness = thickness;
            }

            return line;
        }

        /// <summary>
        /// Draw lines. MUST BE CALLED ON MAIN THREAD
        /// </summary>
        private void DrawLines()
        {
            foreach (var note in _notes)
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (note.LastT == 0 || note.Done)
                    continue;

                // Hold line
                if (note.IsHoldStart)
                {
                    if (note.HoldLine == null)
                    {
                        note.HoldLine = CreateLineOnCurrentThread(HoldLineThickness);
                    }

                    note.HoldLine.X1 = note.HoldLine.X2 = note.X + _lineOffset;
                    note.HoldLine.Y1 = note.Y + _lineOffset;
                    note.HoldLine.Y2 = note.HoldTarget.Y + _lineOffset;
                }

                // Sync line
                // check LastT so that when HitNote arrives the line is gone
                if (note.SyncTarget != null && note.LastT < 1)
                {
                    if (note.SyncLine == null)
                    {
                        note.SyncLine = CreateLineOnCurrentThread(SyncLineThickness);
                    }

                    note.SyncLine.X1 = note.X + _lineOffset;
                    note.SyncLine.Y1 = note.SyncLine.Y2 = note.Y + _lineOffset;
                    note.SyncLine.X2 = note.SyncTarget.X + _lineOffset;
                }

                // Flicker line
                if (note.GroupTarget != null && note.LastT < 1)
                {
                    if (note.GroupLine == null)
                    {
                        note.GroupLine = CreateLineOnCurrentThread(SyncLineThickness);
                    }

                    note.GroupLine.X1 = note.X + _lineOffset;
                    note.GroupLine.Y1 = note.Y + _lineOffset;
                    note.GroupLine.X2 = note.GroupTarget.X + _lineOffset;
                    note.GroupLine.Y2 = note.GroupTarget.Y + _lineOffset;
                }
            }
        }

        /// <summary>
        /// Running in a background thread, refresh the locations of notes periodically. It tries to keep the target frame rate.
        /// </summary>
        private void DrawPreviewFrame()
        {
            // computation parameters
            var targetFrameTime = 1000 / _targetFps;
            _noteHitMax = (int) (_targetFps / 5); // effect lasts for 0.2 second

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
                    ClearCanvases();

                    _linePool.Clear();
                    _notes.Clear();
                    _scoreNotePool.Clear();
                    return;
                }

                var frameStartTime = DateTime.UtcNow;

                int songTime;
                if (_shouldPlayMusic)
                {
                    songTime = (int)_window.MusicTime().TotalMilliseconds;
                }
                else
                {
                    songTime = (int)(frameStartTime - startTime).TotalMilliseconds + _startTime;
                }

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

                    if (note.ScoreNote == null)
                    {
                        note.ScoreNote =  CreateScoreNote(note);
                    }

                    var t = diff/ _approachTime;
                    if (t > 1)
                        t = 1;

                    note.LastT = t;

                    // change this line if you want game-like approaching path
                    note.Y = _noteStartY + t*(_noteEndY - _noteStartY);
                    SetPositionInCanvas(note.ScoreNote, note.X, note.Y);

                    // note arrive at bottom
                    if (diff > _approachTime)
                    {
                        ReleaseLine(note.SyncLine);
                        ReleaseLine(note.GroupLine);
                        note.SyncLine = null;
                        note.GroupLine = null;

                        // Hit and flick notes end immediately
                        // Hold note heads end after its duration
                        if (!note.IsHoldStart || diff > _approachTime + note.Duration)
                        {
                            ReleaseScoreNote(note.ScoreNote);
                            ReleaseLine(note.HoldLine);
                            note.ScoreNote = null;
                            note.HoldLine = null;

                            note.Done = true;

                            // note hit effect
                            _noteHitCounter = _noteHitMax;
                        }

                        if (!headUpdated)
                        {
                            notesHead = i;
                            headUpdated = true;
                        }                        
                    }
                }

                // Draw sync, group, hold lines
                Dispatcher.Invoke(new Action(DrawLines));

                // Do note hit effect
                if (_noteHitCounter >= 0)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        double et = _noteHitCounter/(double) _noteHitMax; // from 1 to 0
                        _effectedLine.Opacity = et;
                    }));
                    --_noteHitCounter;
                }

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
