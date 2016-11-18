using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.Extensions;
using DereTore.Applications.StarlightDirector.UI.Controls.Models;

namespace DereTore.Applications.StarlightDirector.UI.Controls
{
    public class PreviewCanvas : Canvas
    {
        // constants
        private const double NoteMarginTop = 20;
        private const double NoteMarginBottom = 60;
        private const double NoteXBetween = 80;
        private const double NoteSize = 30;
        private const double NoteRadius = NoteSize / 2;

        // dimensions
        private double _noteStartY;
        private double _noteEndY;
        private readonly List<double> _noteX = new List<double>();

        // computation
        private List<DrawingNote> _notes;
        private int _notesHead;
        private int _notesTail;
        private double _approachTime;

        // rendering
        private volatile bool _isPreviewing;
        private readonly List<int> _hitEffectCountdown;
        private readonly EventWaitHandle _renderCompleteHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        public int HitEffectFrames { get; set; }

        public PreviewCanvas()
        {
            _hitEffectCountdown = new List<int>();
            for (int i = 0; i < 5; ++i)
            {
                _hitEffectCountdown.Add(0);
            }
        }

        public void Initialize(List<DrawingNote> notes, double approachTime)
        {
            _notes = notes;
            _approachTime = approachTime;
            _notesHead = 0;

            // compute positions

            _noteStartY = NoteMarginTop;
            _noteEndY = ActualHeight - NoteMarginBottom;

            _noteX.Clear();
            _noteX.Add((ActualWidth - 4 * NoteXBetween - 5 * NoteSize) / 2);
            for (int i = 1; i < 5; ++i)
            {
                _noteX.Add(_noteX.Last() + NoteXBetween + NoteSize);
            }

            // initialize note positions

            foreach (var note in _notes)
            {
                note.X = _noteX[note.HitPosition];
                note.Y = _noteStartY;
            }

            _isPreviewing = true;
        }

        public void RenderFrameBlocked(int songTime)
        {
            ComputeFrame(songTime);

            Dispatcher.Invoke(new Action(InvalidateVisual));
            _renderCompleteHandle.WaitOne();
        }

        public void Stop()
        {
            _isPreviewing = false;
        }

        public void NoteHit(int position)
        {
            _hitEffectCountdown[position] = HitEffectFrames;
        }

        #region Brushes and Pens

        private static readonly SolidColorBrush NoteStroke =
            new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x22));

        private static readonly SolidColorBrush NoteShapeOutlineFill = Brushes.White;

        private static readonly SolidColorBrush NormalNoteShapeStroke =
            new SolidColorBrush(Color.FromRgb(0xFF, 0x33, 0x66));

        private static readonly LinearGradientBrush NormalNoteShapeFill =
            new LinearGradientBrush(Color.FromRgb(0xFF, 0x33, 0x66), Color.FromRgb(0xFF, 0x99, 0xBB), 90.0);

        private static readonly SolidColorBrush HoldNoteShapeStroke =
            new SolidColorBrush(Color.FromRgb(0xFF, 0xBB, 0x22));

        private static readonly LinearGradientBrush HoldNoteShapeFillOuter =
            new LinearGradientBrush(Color.FromRgb(0xFF, 0xBB, 0x22), Color.FromRgb(0xFF, 0xDD, 0x66), 90.0);

        private static readonly SolidColorBrush HoldNoteShapeFillInner = Brushes.White;

        private static readonly SolidColorBrush FlickNoteShapeStroke =
            new SolidColorBrush(Color.FromRgb(0x22, 0x55, 0xBB));

        private static readonly LinearGradientBrush FlickNoteShapeFillOuter =
            new LinearGradientBrush(Color.FromRgb(0x22, 0x55, 0xBB), Color.FromRgb(0x88, 0xBB, 0xFF), 90.0);

        private static readonly SolidColorBrush FlickNoteShapeFillInner = Brushes.White;

        private static readonly Pen NoteStrokePen = new Pen(NoteStroke, 1.5);

        private static readonly Pen NormalNoteShapeStrokePen = new Pen(NormalNoteShapeStroke, 1);

        private static readonly Pen HoldNoteShapeStrokePen = new Pen(HoldNoteShapeStroke, 1);

        private static readonly Pen FlickNoteShapeStrokePen = new Pen(FlickNoteShapeStroke, 1);

        private static readonly Brush RelationBrush =  Application.Current.FindResource<Brush>(App.ResourceKeys.RelationBorderBrush);

        private static readonly Pen[] LinePens =
        {
            new Pen(RelationBrush, 16), // hold line
            new Pen(RelationBrush, 2),  // sync line
            new Pen(RelationBrush, 8)   // group line
        };

        #endregion

        #region Computation and Positions

        private void SetNotePosition(DrawingNote note, double t)
        {
            note.Y = _noteStartY + t * (_noteEndY - _noteStartY);
        }

        private void ComputeFrame(int songTime)
        {
            var headUpdated = false;
            for (int i = _notesHead; i < _notes.Count; ++i)
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

                _notesTail = i;

                var t = diff / _approachTime;
                if (t > 1)
                    t = 1;

                note.LastT = t;
                SetNotePosition(note, t);

                // note arrive at bottom
                if (diff > _approachTime)
                {
                    NoteHit(note.HitPosition);

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
                    _notesHead = i;
                    headUpdated = true;
                }
            }
        }

        #endregion

        #region Rendering

        private void DrawLines(DrawingContext dc)
        {
            for (int i = _notesHead; i <= _notesTail; ++i)
            {
                var note = _notes[i];

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (note.LastT == 0 || note.Done)
                    return;

                // Hold line
                if (note.IsHoldStart)
                {
                    dc.DrawLine(LinePens[0], new Point(note.X, note.Y), new Point(note.HoldTarget.X, note.HoldTarget.Y));
                }

                // Sync line
                // check LastT so that when HitNote arrives the line is gone
                if (note.SyncTarget != null && note.LastT < 1)
                {
                    dc.DrawLine(LinePens[1], new Point(note.X, note.Y), new Point(note.SyncTarget.X, note.SyncTarget.Y));
                }

                // Flicker line
                if (note.GroupTarget != null && note.LastT < 1)
                {
                    dc.DrawLine(LinePens[2], new Point(note.X, note.Y), new Point(note.GroupTarget.X, note.GroupTarget.Y));
                }
            }
        }

        private void DrawNotes(DrawingContext dc)
        {
            for (int i = _notesHead; i <= _notesTail; ++i)
            {
                var note = _notes[i];
                var center = new Point(note.X, note.Y);

                switch (note.DrawType)
                {
                    case 0:
                    case 1:
                    case 2:
                        dc.DrawEllipse(NoteShapeOutlineFill, NoteStrokePen, center, NoteRadius, NoteRadius);
                        dc.DrawEllipse(NormalNoteShapeFill, NormalNoteShapeStrokePen, center, NoteRadius - 4, NoteRadius - 4);
                        break;
                    case 3:
                        dc.DrawEllipse(NoteShapeOutlineFill, NoteStrokePen, center, NoteRadius, NoteRadius);
                        dc.DrawEllipse(HoldNoteShapeFillOuter, HoldNoteShapeStrokePen, center, NoteRadius - 4, NoteRadius - 4);
                        dc.DrawEllipse(HoldNoteShapeFillInner, null, center, NoteRadius - 10, NoteRadius - 10);
                        break;
                }
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            _renderCompleteHandle.Reset();

            base.OnRender(dc);

            if (!_isPreviewing)
            {
                _renderCompleteHandle.Set();
                return;
            }

            DrawLines(dc);
            DrawNotes(dc);

            _renderCompleteHandle.Set();
        }

        #endregion
    }
}
