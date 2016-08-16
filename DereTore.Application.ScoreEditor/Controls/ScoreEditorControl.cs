using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DereTore.Application.ScoreEditor.Model;

namespace DereTore.Application.ScoreEditor.Controls {
    public sealed class ScoreEditorControl : DoubleBufferedPictureBox {

        public ScoreEditorControl() {
            Notes = new List<Note>();
        }

        public List<Note> Notes { get; }

        public void SetScore(Score score) {
            Notes.Clear();
            Notes.AddRange(score.Items);
        }

        public bool EnableMouse { get; set; }

        public event EventHandler<NoteEnteringOrExitingStageEventArgs> NoteEnteringOrExitingStage;

        public void JudgeNotesEnteringOrExiting(TimeSpan timeSpan) {
            var now = (float)timeSpan.TotalSeconds;
            foreach (var note in Notes) {
                if (RenderHelper.IsNoteOnStage(note, now)) {
                    if (!note.Visible) {
                        NoteEnteringOrExitingStage?.Invoke(this, new NoteEnteringOrExitingStageEventArgs(note));
                        note.Visible = true;
                    }
                } else {
                    if (note.Visible) {
                        NoteEnteringOrExitingStage?.Invoke(this, new NoteEnteringOrExitingStageEventArgs(note));
                        note.Visible = false;
                    }
                }
            }
        }

        public void SetTime(TimeSpan elapsed) {
            _elapsed = elapsed;
        }

        public Note HitTest(float x, float y) {
            var now = (float)_elapsed.TotalSeconds;
            var clientSize = ClientSize;
            foreach (var note in Notes.Where(n => n.IsGamingNote)) {
                float nx = RenderHelper.GetNoteXPosition(note, clientSize, now), ny = RenderHelper.GetNoteYPosition(note, clientSize, now);
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

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            if (_isPainting || Renderer.Instance.IsRendering) {
                return;
            }
            _isPainting = true;
            e.Graphics.Clear(Color.Black);
            Renderer.Instance.RenderFrame(e.Graphics, ClientSize, _elapsed, Notes);
            _isPainting = false;
        }

        protected override void OnMouseClick(MouseEventArgs e) {
            base.OnMouseClick(e);
            var note = HitTest(e.X, e.Y);
            if (note != null) {
                note.Selected = true;
            }
        }

        private bool _isPainting;
        private TimeSpan _elapsed;

    }
}
