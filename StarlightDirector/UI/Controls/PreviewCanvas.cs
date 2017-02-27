using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StarlightDirector.Extensions;
using StarlightDirector.UI.Controls.Models;

namespace StarlightDirector.UI.Controls {
    public class PreviewCanvas : Canvas {

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
        private readonly List<Point> _startPoints = new List<Point>();
        private readonly List<Point> _endPoints = new List<Point>();

        // computation
        private List<DrawingNote> _notes;
        private int _notesHead;
        private int _notesTail;
        private double _approachTime;
        private List<DrawingBar> _bars;
        private int _barsHead;

        // rendering
        private volatile bool _isPreviewing;
        private List<int> _hitEffectStartTime;
        private List<double> _hitEffectT;
        private readonly EventWaitHandle _renderCompleteHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        public double HitEffectMilliseconds { get; set; }
        public bool[] ShallPlaySoundEffect { get; } = {false, false};

        public void Initialize(List<DrawingNote> notes, List<DrawingBar> bars,  double approachTime) {
            _notes = notes;
            _bars = bars;
            _approachTime = approachTime;
            _notesHead = 0;
            _barsHead = 0;

            // compute positions

            _noteStartY = NoteMarginTop;
            _noteEndY = ActualHeight - NoteMarginBottom;

            _noteX.Clear();
            _noteX.Add((ActualWidth - 4 * NoteXBetween - 5 * NoteSize) / 2);
            for (int i = 1; i < 5; ++i) {
                _noteX.Add(_noteX.Last() + NoteXBetween + NoteSize);
            }

            _startPoints.Clear();
            _endPoints.Clear();
            for (int i = 0; i < 5; ++i) {
                _startPoints.Add(new Point(_noteX[i], _noteStartY));
                _endPoints.Add(new Point(_noteX[i], _noteEndY));
            }

            // initialize note positions

            foreach (var note in _notes) {
                note.X = _noteX[note.HitPosition];
                note.Y = _noteStartY;
            }

            // hit effect timing

            _hitEffectStartTime = new List<int>();
            _hitEffectT = new List<double>();
            for (int i = 0; i < 5; ++i) {
                _hitEffectStartTime.Add(0);
                _hitEffectT.Add(0);
            }

            _isPreviewing = true;
        }

        public void RenderFrameBlocked(int musicTimeInMillis) {
            ComputeFrame(musicTimeInMillis);

            Dispatcher.Invoke(new Action(InvalidateVisual));
            _renderCompleteHandle.WaitOne();
        }

        public void Stop() {
            _isPreviewing = false;
        }

        private void NoteHit(DrawingNote note, int songTime) {
            _hitEffectStartTime[note.HitPosition] = songTime;
            if (note.DrawType == 1 || note.DrawType == 2)
            {
                ShallPlaySoundEffect[1] = true;
            }
            else
            {
                ShallPlaySoundEffect[0] = true;
            }
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

        private static readonly Pen GridPen = new Pen(Brushes.DarkGray, 1);

        private static readonly Brush RelationBrush = Application.Current.FindResource<Brush>(App.ResourceKeys.RelationBorderBrush);

        private static readonly Pen[] LinePens =
        {
            new Pen(RelationBrush, 16), // hold line
            new Pen(RelationBrush, 2),  // sync line
            new Pen(RelationBrush, 8)   // group line
        };

        private static readonly Geometry LeftNoteOuterGeometry =
    Geometry.Parse("M -6,15 A 24,24 0 0 1 15,0 A 15,15 0 0 1 15,30 A 24,24 0 0 1 -6,15 Z");
        private static readonly Geometry LeftNoteInnerGeometry =
            Geometry.Parse("M -6,15 C 6,-2 20,-2 15,15 C 21,32 6,32 -6,15 Z");

        private static readonly Geometry RightNoteOuterGeometry =
    Geometry.Parse("M 36,15 A 24,24 0 0 0 15,0 A 15,15 0 0 0 15,30 A 24,24 0 0 0 36,15 Z");
        private static readonly Geometry RightNoteInnerGeometry =
            Geometry.Parse("M 36,15 C 24,-2 16,-2 15,15 C 16,32 24,32 36,15 Z");

        private static readonly ScaleTransform FlickNoteOuterScale = new ScaleTransform(0.722, 0.722, 15, 15);
        private static readonly ScaleTransform FlickNoteInnerScale = new ScaleTransform(0.5714, 0.5714, 15, 15);

        private static readonly SolidColorBrush HitEffectBrush = Brushes.Gold;

        private static readonly Pen[] BarPens =
        {
            new Pen(new SolidColorBrush(Color.FromArgb(0xff, 0xf6, 0xf6, 0x0a)), 2),
            new Pen(new SolidColorBrush(Color.FromArgb(0xff, 0xfd, 0xaf, 0xc9)), 2),
            new Pen(new SolidColorBrush(Color.FromArgb(0xff, 0xc6, 0xc0, 0xe2)), 2),
            new Pen(new SolidColorBrush(Color.FromArgb(0xff, 0xd5, 0xd5, 0xd5)), 1)
        };

        #endregion

        #region Computation and Positions

        private void SetNotePosition(DrawingNote note, double t) {
            note.Y = _noteStartY + t * (_noteEndY - _noteStartY);
        }

        private void ComputeFrame(int musicTimeInMillis) {
            ShallPlaySoundEffect[0] = ShallPlaySoundEffect[1] = false;

            var headUpdated = false;
            for (int i = _notesHead; i < _notes.Count; ++i) {
                var note = _notes[i];

                /*
                 * diff <  0 --- not shown
                 * diff == 0 --- begin to show
                 * diff == approachTime --- arrive at end
                 * diff >  approachTime --- ended
                 */
                var diff = musicTimeInMillis - note.Timing + _approachTime;
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
                if (diff > _approachTime) {
                    if (!note.EffectShown) {
                        NoteHit(note, musicTimeInMillis);
                        note.EffectShown = true;
                    }

                    // Hit and flick notes has Duration 0
                    if (diff > _approachTime + note.Duration) {
                        note.Done = true;
                    }
                }

                // update head to be the first note that is not done
                if (!note.Done && !headUpdated) {
                    _notesHead = i;
                    headUpdated = true;
                }
            }

            // Update bars
            for (int i = _barsHead; i < _bars.Count; ++i)
            {
                var bar = _bars[i];
                var diff = musicTimeInMillis - _bars[i].Timing + _approachTime;
                if (diff <= 0)
                    break;
                if (diff > _approachTime)
                {
                    ++_barsHead;
                    continue;
                }

                var t = diff/_approachTime;
                bar.T = t;
                bar.Y = (_noteEndY - _noteStartY)*t + _noteStartY;
            }

            // Update hit effects
            for (int i = 0; i < 5; ++i) {
                if (_hitEffectStartTime[i] == 0)
                    continue;

                var diff = musicTimeInMillis - _hitEffectStartTime[i];
                if (diff <= HitEffectMilliseconds) {
                    _hitEffectT[i] = 1 - diff / HitEffectMilliseconds;
                } else {
                    _hitEffectT[i] = 0;
                }
            }
        }

        #endregion

        #region Rendering

        private void DrawGrid(DrawingContext dc) {
            for (int i = 0; i < 5; ++i) {
                dc.DrawLine(GridPen, _startPoints[i], _endPoints[i]);
            }
        }

        private void DrawLines(DrawingContext dc) {
            for (int i = _notesHead; i <= _notesTail; ++i) {
                var note = _notes[i];

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (note.LastT == 0 || note.Done)
                    continue;

                // Hold line
                if (note.IsHoldStart) {
                    dc.DrawLine(LinePens[0], new Point(note.X, note.Y), new Point(note.HoldTarget.X, note.HoldTarget.Y));
                }

                // Sync line
                // check LastT so that when HoldNote arrives the line is gone
                if (note.SyncTarget != null && note.LastT < 1) {
                    dc.DrawLine(LinePens[1], new Point(note.X, note.Y), new Point(note.SyncTarget.X, note.SyncTarget.Y));
                }

                // Flicker line
                if (note.GroupTarget != null && note.LastT < 1) {
                    dc.DrawLine(LinePens[2], new Point(note.X, note.Y), new Point(note.GroupTarget.X, note.GroupTarget.Y));
                }
            }

            // bar lines
            for (int i = _barsHead; i < _bars.Count; ++i)
            {
                var bar = _bars[i];
                if (bar.T <= 0)
                    break;

                dc.DrawLine(BarPens[bar.DrawType], new Point(_noteX[0], bar.Y), new Point(_noteX[4], bar.Y));
            }
        }

        private void DrawNotes(DrawingContext dc) {
            for (int i = _notesHead; i <= _notesTail; ++i) {
                var note = _notes[i];
                if (note.Done)
                    continue;

                var center = new Point(note.X, note.Y);


                switch (note.DrawType) {
                    case 0:
                        dc.DrawEllipse(NoteShapeOutlineFill, NoteStrokePen, center, NoteRadius, NoteRadius);
                        dc.DrawEllipse(NormalNoteShapeFill, NormalNoteShapeStrokePen, center, NoteRadius - 4, NoteRadius - 4);
                        break;
                    case 1:
                        dc.PushTransform(new TranslateTransform(note.X - 15, note.Y - 15));
                        dc.DrawGeometry(NoteShapeOutlineFill, NoteStrokePen, LeftNoteOuterGeometry);
                        dc.PushTransform(FlickNoteOuterScale);
                        dc.DrawGeometry(FlickNoteShapeFillOuter, FlickNoteShapeStrokePen, LeftNoteOuterGeometry);
                        dc.Pop();
                        dc.PushTransform(FlickNoteInnerScale);
                        dc.DrawGeometry(FlickNoteShapeFillInner, null, LeftNoteInnerGeometry);
                        dc.Pop();
                        dc.Pop();
                        break;
                    case 2:
                        dc.PushTransform(new TranslateTransform(note.X - 15, note.Y - 15));
                        dc.DrawGeometry(NoteShapeOutlineFill, NoteStrokePen, RightNoteOuterGeometry);
                        dc.PushTransform(FlickNoteOuterScale);
                        dc.DrawGeometry(FlickNoteShapeFillOuter, FlickNoteShapeStrokePen, RightNoteOuterGeometry);
                        dc.Pop();
                        dc.PushTransform(FlickNoteInnerScale);
                        dc.DrawGeometry(FlickNoteShapeFillInner, null, RightNoteInnerGeometry);
                        dc.Pop();
                        dc.Pop();
                        break;
                    case 3:
                        dc.DrawEllipse(NoteShapeOutlineFill, NoteStrokePen, center, NoteRadius, NoteRadius);
                        dc.DrawEllipse(HoldNoteShapeFillOuter, HoldNoteShapeStrokePen, center, NoteRadius - 4, NoteRadius - 4);
                        dc.DrawEllipse(HoldNoteShapeFillInner, null, center, NoteRadius - 10, NoteRadius - 10);
                        break;
                }
            }
        }

        private void DrawEffect(DrawingContext dc) {
            for (int i = 0; i < 5; ++i) {
                var t = _hitEffectT[i];

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (t == 0)
                    continue;

                dc.PushOpacity(t);
                dc.DrawEllipse(HitEffectBrush, null, _endPoints[i], NoteSize, NoteSize);
                dc.Pop();
            }
        }

        protected override void OnRender(DrawingContext dc) {
            _renderCompleteHandle.Reset();

            base.OnRender(dc);

            if (!_isPreviewing) {
                _renderCompleteHandle.Set();
                return;
            }

            DrawGrid(dc);
            DrawLines(dc);
            DrawEffect(dc);
            DrawNotes(dc);

            _renderCompleteHandle.Set();
        }

        #endregion
    }
}
