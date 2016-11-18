using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.Extensions;

namespace DereTore.Applications.StarlightDirector.UI.Controls
{
    public class PreviewCanvas : Canvas
    {
        private List<int> _hitEffectCountdown;

        public int HitEffectFrames { get; set; }

        public EventWaitHandle RenderCompleteHandle { get; } = new EventWaitHandle(false, EventResetMode.AutoReset);

        // Type, x, y
        // Types: 0 - tap, 1 - left, 2 - right, 3 - hold
        public List<Tuple<int, double, double>> Notes { get; } = new List<Tuple<int, double, double>>();

        // Type, x1, y1, x2, y2
        // Types: 0 - hold, 1 - sync, 2 - group
        public List<Tuple<int, double, double, double, double>> Lines { get; } = new List<Tuple<int, double, double, double, double>>();

        public PreviewCanvas()
        {
            _hitEffectCountdown = new List<int>();
            for (int i = 0; i < 5; ++i)
            {
                _hitEffectCountdown.Add(0);
            }
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

        #region Position Computation



        #endregion

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            // draw lines

            foreach (var line in Lines)
            {
                var p1 = new Point(line.Item2, line.Item3);
                var p2 = new Point(line.Item4, line.Item5);

                dc.DrawLine(LinePens[line.Item1], p1, p2);
            }

            // draw notes

            foreach (var note in Notes)
            {
                var x = note.Item2;
                var y = note.Item3;
                var center = new Point(x, y);

                switch (note.Item1)
                {
                    case 0:
                    case 1:
                    case 2:
                        dc.DrawEllipse(NoteShapeOutlineFill, NoteStrokePen, center, 15, 15);
                        dc.DrawEllipse(NormalNoteShapeFill, NormalNoteShapeStrokePen, center, 11, 11);
                        break;
                    case 3:
                        dc.DrawEllipse(NoteShapeOutlineFill, NoteStrokePen, center, 15, 15);
                        dc.DrawEllipse(HoldNoteShapeFillOuter, HoldNoteShapeStrokePen, center, 11, 11);
                        dc.DrawEllipse(HoldNoteShapeFillInner, null, center, 5, 5);
                        break;
                }
            }

            RenderCompleteHandle.Set();
        }
    }
}
