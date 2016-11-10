using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Entities.Extensions;
using DereTore.Applications.StarlightDirector.UI.Controls.Primitives;

namespace DereTore.Applications.StarlightDirector.UI.Controls
{
    /// <summary>
    /// Interaction logic for ScorePreviewer.xaml
    /// </summary>
    public partial class ScorePreviewer
    {
        private Score _score;
        private volatile bool _isPreviewing;
        private Task _task;
        private Dictionary<int, bool> _noteDone = new Dictionary<int, bool>();
        private Dictionary<int, int> _timings = new Dictionary<int, int>();
        private Dictionary<int, SimpleScoreNote> _scoreNotes = new Dictionary<int, SimpleScoreNote>();
        private Dictionary<int, int> _notePositions = new Dictionary<int, int>();
        private List<Note> _notes = new List<Note>();

        private const double NoteMarginTop = 20;
        private const double NoteMarginBottom = 60;
        private const double NoteXBetween = 80;
        private const double NoteSize = 30;

        private double _noteStartY;
        private double _noteEndY;
        private List<double> _noteX;

        public ScorePreviewer()
        {
            InitializeComponent();
        }

        public void BeginPreview(Score score)
        {
            _score = score;
            _isPreviewing = true;

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

                _notePositions[note.ID] = pos - 1;
                _noteDone[note.ID] = false;
                _scoreNotes[note.ID] = null;
                _timings[note.ID] = (int)(note.HitTiming * 1000);

                _notes.Add(note);
            }
            _notes.Sort((a, b) => _timings[a.ID] - _timings[b.ID]);

            _noteX = new List<double>();
            for (int i = 0; i < 5; ++i)
                _noteX.Add(0);
            ComputePositions();

            _task = new Task(DrawPreviewFrame);
            _task.Start();
        }

        public void EndPreview()
        {
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
        }

        #region Multithreading Invoke

        private readonly Func<Canvas, Note, SimpleScoreNote> _createScoreNoteFunc = (canvas, note) =>
        {
            var scoreNote = new SimpleScoreNote { Note = note };
            canvas.Children.Add(scoreNote);
            return scoreNote;
        };

        private readonly Action<Canvas, UIElement> _removeFromCanvasAction =
            (canvas, elem) => canvas.Children.Remove(elem);

        private readonly Action<UIElement, double, double> _setPositionInCanvasAction =
            (elem, x, y) =>
            {
                Canvas.SetLeft(elem, x);
                Canvas.SetTop(elem, y);
            };

        private SimpleScoreNote CreateScoreNote(Note note)
        {
            return Dispatcher.Invoke(_createScoreNoteFunc, PreviewCanvas, note) as SimpleScoreNote;
        }

        private void SetPositionInCanvas(UIElement elem, double x, double y)
        {
            Dispatcher.Invoke(_setPositionInCanvasAction, elem, x, y);
        }

        private void RemoveFromCanvas(UIElement elem)
        {
            Dispatcher.Invoke(_removeFromCanvasAction, PreviewCanvas, elem);
        }

        #endregion

        private void DrawPreviewFrame()
        {
            // computation parameters
            var targetFrameTime = 1000 / 30.0; // TODO: to constant or configurable
            var approachTime = 700.0; // TODO: use speed

            // drawing and timing
            var startTime = DateTime.UtcNow; // TODO: remove this, use music time
            var notesHead = 0;

            while (true)
            {
                if (!_isPreviewing)
                {
                    foreach (var sn in _scoreNotes.Values)
                    {
                        if (sn != null)
                            RemoveFromCanvas(sn);
                    }

                    _timings.Clear();
                    _scoreNotes.Clear();
                    _noteDone.Clear();
                    return;
                }

                var frameStartTime = DateTime.UtcNow;
                var songTime = (int)(frameStartTime - startTime).TotalMilliseconds;

                for (int i = notesHead; i < _notes.Count; ++i)
                {
                    var note = _notes[i];

                    /*
                     * diff <  0 --- not shown
                     * diff == 0 --- begin to show
                     * diff == approachTime --- arrive at end
                     * diff >  approachTime --- ended
                     */
                    var diff = songTime - _timings[note.ID] - approachTime;
                    if (diff < 0)
                        break;
                    if (_noteDone[note.ID])
                        continue;

                    var scoreNote = _scoreNotes[note.ID];
                    if (scoreNote == null)
                    {
                        scoreNote = _scoreNotes[note.ID] = CreateScoreNote(note);
                    }

                    var t = diff/approachTime;

                    SetPositionInCanvas(scoreNote, _noteX[_notePositions[note.ID]],
                        _noteStartY + t*(_noteEndY - _noteStartY));

                    // TODO: draw sync lines, hold note lines, group lines

                    if (diff > approachTime)
                    {

                        RemoveFromCanvas(scoreNote);
                        _scoreNotes[note.ID] = null;
                        _noteDone[note.ID] = true;
                        notesHead = i;
                    }
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
