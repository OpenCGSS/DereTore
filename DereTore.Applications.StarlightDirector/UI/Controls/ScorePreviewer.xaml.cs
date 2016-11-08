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
    public partial class ScorePreviewer : UserControl
    {
        private Score _score;
        private volatile bool _isPreviewing;
        private Task _task;
        private Dictionary<int, bool> _noteDone = new Dictionary<int, bool>();
        private Dictionary<int, int> _timings = new Dictionary<int, int>();
        private Dictionary<int, SimpleScoreNote> _scoreNotes = new Dictionary<int, SimpleScoreNote>();
        private List<Note> _notes = new List<Note>();

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
                _noteDone[note.ID] = false;
                _scoreNotes[note.ID] = null;
                note.HitTime = (int)(note.GetHitTiming() * 1000);
                _timings[note.ID] = note.HitTime;
                _notes.Add(note);
            }
            _notes.Sort((a, b) => a.HitTime - b.HitTime);

            _task = new Task(DrawPreviewFrame);
            _task.Start();
        }

        public void EndPreview()
        {
            _isPreviewing = false;
        }

        private bool IsValidNote(Note note)
        {
            return (bool)Dispatcher.Invoke(new Func<bool>(() => note.Type == NoteType.TapOrFlick || note.Type == NoteType.Hold));
        }

        private SimpleScoreNote CreateScoreNote(Note note)
        {
            return Dispatcher.Invoke(new Func<SimpleScoreNote>(() =>
            {
                var scoreNote = new SimpleScoreNote { Note = note};
                PreviewCanvas.Children.Add(scoreNote);
                //scoreNote.X = (note.ID%100)*10;
                //scoreNote.Y = (note.ID%100)*10;
                return scoreNote;
            })) as SimpleScoreNote;
        }

        private void RemoveFromCanvas(UIElement elem)
        {
            Dispatcher.Invoke(new Action(() => PreviewCanvas.Children.Remove(elem)));
        }

        private void DrawPreviewFrame()
        {
            // computation parameters
            var targetFrameTime = 1000 / 30.0; // TODO: to constant or configurable
            var approachTime = 700; // TODO: use speed

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
                    if (!IsValidNote(note))
                        continue;

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
                        //Debug.WriteLine($"[{songTime}] Creating note {note.ID}, diff = {diff}");
                        scoreNote = _scoreNotes[note.ID] = CreateScoreNote(note);
                    }
                    
                    // TODO: set position of note
                    // TODO: draw other things

                    if (diff > approachTime)
                    {
                        //Debug.WriteLine($"[{songTime}] Removing note {note.ID}, noteHead becomes {i}");
                        RemoveFromCanvas(scoreNote);
                        _scoreNotes[note.ID] = null;
                        _noteDone[note.ID] = true;
                        notesHead = i;
                    }
                }

                Dispatcher.Invoke(new Action(() => Debug.WriteLine(PreviewCanvas.Children.Count)));
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
