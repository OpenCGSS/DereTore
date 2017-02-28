using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DereTore.Applications.ScoreViewer.Model;
using Timer = System.Threading.Timer;

namespace DereTore.Applications.ScoreViewer.Controls {
    public sealed class ScoreEditorControl : DoubleBufferedPictureBox {

        public ScoreEditorControl() {
            MouseEventsEnabled = true;
            _timer = new Timer(OnTimer, null, 1000, 1000);
        }

        ~ScoreEditorControl() {
            _timer.Dispose();
            _timer = null;
        }

        [Browsable(false)]
        public bool MouseEventsEnabled { get; set; }

        public event EventHandler<NoteEnteringOrExitingStageEventArgs> NoteEnteringOrExitingStage;
        public event EventHandler<EventArgs> SelectedNoteChanged;

        public void JudgeNotesEnteringOrExiting(TimeSpan timeSpan) {
            var now = timeSpan.TotalSeconds;
            foreach (var note in Score.Notes) {
                if (RenderHelper.IsNoteOnStage(note, now)) {
                    if (!note.EditorVisible) {
                        NoteEnteringOrExitingStage?.Invoke(this, new NoteEnteringOrExitingStageEventArgs(note, true));
                        note.EditorVisible = true;
                    }
                } else {
                    if (note.EditorVisible) {
                        NoteEnteringOrExitingStage?.Invoke(this, new NoteEnteringOrExitingStageEventArgs(note, false));
                        note.EditorVisible = false;
                    }
                }
            }
        }

        public void SetTime(TimeSpan elapsed) {
            _elapsed = elapsed;
        }

        public Score Score {
            get {
                return _score;
            }
            set {
                if (_score != null) {
                    _score.ScoreChanged -= Score_ScoreChanged;
                }
                if (value == null) {
                    SelectedNote = null;
                }
                _score = value;
                if (_score != null) {
                    _score.ScoreChanged += Score_ScoreChanged;
                }
            }
        }

        public Note HitTest(float x, float y) {
            if (Score == null) {
                return null;
            }
            var now = (float)_elapsed.TotalSeconds;
            var renderParams = new RenderParams(null, ClientSize, now, IsPreview);
            foreach (var note in Score.Notes.Where(n => n.IsGamingNote)) {
                float nx = RenderHelper.GetNoteXPosition(renderParams, note), ny = RenderHelper.GetNoteYPosition(renderParams, note);
                var rect = new RectangleF(nx - RenderHelper.AvatarCircleRadius, ny - RenderHelper.AvatarCircleRadius, RenderHelper.AvatarCircleDiameter, RenderHelper.AvatarCircleDiameter);
                if (rect.Contains(x, y)) {
                    return note;
                }
            }
            return null;
        }

        public Note HitTest(PointF pt) {
            return HitTest(pt.X, pt.Y);
        }

        public Note SelectedNote {
            get {
                return _selectedNote;
            }
            private set {
                var b = value != _selectedNote;
                _selectedNote = value;
                if (b) {
                    SelectedNoteChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// True: preview mode (dynamic trail)
        /// False: score editing mode (no trail)
        /// </summary>
        public bool IsPreview { get; set; }

        public uint FPS => _fps;

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            if (_isPainting || Renderer.Instance.IsRendering) {
                return;
            }
            _isPainting = true;
            e.Graphics.Clear(MouseEventsEnabled ? MouseEventAcceptedColor : MouseEventIgnoredColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var renderParams = new RenderParams(e.Graphics, ClientSize, _elapsed.TotalSeconds, IsPreview);
            Renderer.Instance.RenderFrame(renderParams, Score?.Notes);
            ++_frameCount;
            _isPainting = false;
        }

        protected override void OnMouseClick(MouseEventArgs e) {
            if (!MouseEventsEnabled) {
                return;
            }
            if (SelectedNote != null) {
                SelectedNote.EditorSelected = false;
                SelectedNote = null;
            }
            var note = HitTest(e.X, e.Y);
            if (note != null) {
                note.EditorSelected = !note.EditorSelected;
                SelectedNote = note.EditorSelected ? note : null;
            } else {
                SelectedNote = null;
            }
            base.OnMouseClick(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e) {
            if (!MouseEventsEnabled) {
                return;
            }
            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            if (!MouseEventsEnabled) {
                return;
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            if (!MouseEventsEnabled) {
                return;
            }
            base.OnMouseUp(e);
        }

        protected override void OnMouseEnter(EventArgs e) {
            if (!MouseEventsEnabled) {
                return;
            }
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e) {
            if (!MouseEventsEnabled) {
                return;
            }
            base.OnMouseLeave(e);
        }

        protected override void OnMouseHover(EventArgs e) {
            if (!MouseEventsEnabled) {
                return;
            }
            base.OnMouseHover(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            if (!MouseEventsEnabled) {
                return;
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e) {
            if (!MouseEventsEnabled) {
                return;
            }
            base.OnMouseWheel(e);
        }

        private void Score_ScoreChanged(object sender, ScoreChangedEventArgs e) {
            switch (e.Reason) {
                case ScoreChangeReason.Removing:
                    if (e.Note == SelectedNote) {
                        SelectedNote = null;
                    }
                    break;
                default:
                    break;
            }
        }

        private void OnTimer(object state) {
            _fps = _frameCount;
            _frameCount = 0;
        }

        private bool _isPainting;
        private TimeSpan _elapsed;
        private Note _selectedNote;
        private Score _score;
        private uint _fps;
        private uint _frameCount;
        private Timer _timer;

        private static readonly Color MouseEventAcceptedColor = Color.FromArgb(0x20, 0x20, 0x20);
        private static readonly Color MouseEventIgnoredColor = Color.Black;

    }
}
