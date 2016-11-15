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

        private Score _score;
        private volatile bool _isPreviewing;
        private Task _task;
        private List<SimpleNote> _notes = new List<SimpleNote>();
        private int _offset;
        private double _targetFps;

        // WPF new object performance is horrible, so we use pools to reuse them
        private Queue<SimpleScoreNote> _scoreNotePool = new Queue<SimpleScoreNote>();
        private Queue<Line> _linePool = new Queue<Line>();

        private double _noteStartY;
        private double _noteEndY;
        private List<double> _noteX;
        private double _lineOffset;

        private Brush _relationBrush = Application.Current.FindResource<Brush>(App.ResourceKeys.RelationBorderBrush);
        private Brush _gridBrush = Brushes.DarkGray;

        private MainWindow _window;
        private bool _shouldPlayMusic;
        private int _startTime;

        public ScorePreviewer()
        {
            InitializeComponent();
            _window = Application.Current.MainWindow as MainWindow;
        }

        public void BeginPreview(Score score, double offset, double targetFps, int startTime)
        {
            _score = score;
            _isPreviewing = true;
            _offset = (int)(offset * 1000);
            _targetFps = targetFps;
            _startTime = startTime;

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

            foreach (var line in lines)
            {
                LineCanvas.Children.Add(new Line
                {
                    Stroke = _gridBrush,
                    StrokeThickness = GridLineThickness,
                    X1 = line.Item1 + _lineOffset,
                    Y1 = line.Item2 + _lineOffset,
                    X2 = line.Item3 + _lineOffset,
                    Y2 = line.Item4 + _lineOffset
                });
            }
        }

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
                    return scoreNote;
                }
                else
                {
                    var scoreNote = _scoreNotePool.Dequeue();
                    scoreNote.Note = note.Note;
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

        /// <summary>
        /// Helper used by DrawLines. DO NOT CALL THIS METHOD
        /// </summary>
        private Line CreateLineOnThread(double thickness)
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
                        note.HoldLine = CreateLineOnThread(HoldLineThickness);
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
                        note.SyncLine = CreateLineOnThread(SyncLineThickness);
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
                        note.GroupLine = CreateLineOnThread(SyncLineThickness);
                    }

                    note.GroupLine.X1 = note.X + _lineOffset;
                    note.GroupLine.Y1 = note.Y + _lineOffset;
                    note.GroupLine.X2 = note.GroupTarget.X + _lineOffset;
                    note.GroupLine.Y2 = note.GroupTarget.Y + _lineOffset;
                }
            }
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

        private void DrawPreviewFrame()
        {
            // computation parameters
            var targetFrameTime = 1000 / _targetFps;
            var approachTime = 700.0; // TODO: use speed

            // fix start time
            _startTime -= (int)approachTime;
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
                    songTime = (int)_window.MusicTime().TotalMilliseconds + _offset;
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
                    var diff = songTime - note.Timing - approachTime;
                    if (diff < 0)
                        break;
                    if (note.Done)
                        continue;

                    if (note.ScoreNote == null)
                    {
                        note.ScoreNote =  CreateScoreNote(note);
                    }

                    var t = diff/approachTime;
                    if (t > 1)
                        t = 1;

                    note.LastT = t;

                    note.Y = _noteStartY + t*(_noteEndY - _noteStartY);
                    SetPositionInCanvas(note.ScoreNote, note.X, note.Y);

                    if (diff > approachTime)
                    {
                        ReleaseLine(note.SyncLine);
                        ReleaseLine(note.GroupLine);
                        note.SyncLine = null;
                        note.GroupLine = null;

                        if (!note.IsHoldStart || diff > approachTime + note.Duration)
                        {
                            ReleaseScoreNote(note.ScoreNote);
                            ReleaseLine(note.HoldLine);
                            note.ScoreNote = null;
                            note.HoldLine = null;

                            note.Done = true;
                        }

                        if (!headUpdated)
                        {
                            notesHead = i;
                            headUpdated = true;
                        }                        
                    }
                }

                Dispatcher.Invoke(new Action(DrawLines));

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
