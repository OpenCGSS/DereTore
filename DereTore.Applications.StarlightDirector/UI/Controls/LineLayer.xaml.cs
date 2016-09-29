using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.Components;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    /// <summary>
    /// LineContainer.xaml 的交互逻辑
    /// </summary>
    public partial class LineLayer {

        public LineLayer() {
            InitializeComponent();
            NoteRelations = new NoteRelationCollection();
        }

        public NoteRelationCollection NoteRelations { get; }

        public double NoteLineThickness {
            get { return (double)GetValue(NoteLineThicknessProperty); }
            set { SetValue(NoteLineThicknessProperty, value); }
        }

        public Brush SyncRelationBrush {
            get { return (Brush)GetValue(SyncRelationBrushProperty); }
            set { SetValue(SyncRelationBrushProperty, value); }
        }

        public Brush FlickRelationBrush {
            get { return (Brush)GetValue(FlickRelationBrushProperty); }
            set { SetValue(FlickRelationBrushProperty, value); }
        }

        public Brush HoldRelationBrush {
            get { return (Brush)GetValue(HoldRelationBrushProperty); }
            set { SetValue(HoldRelationBrushProperty, value); }
        }

        public static readonly DependencyProperty NoteLineThicknessProperty = DependencyProperty.Register(nameof(NoteLineThickness), typeof(double), typeof(LineLayer),
            new PropertyMetadata(4d));

        public static readonly DependencyProperty SyncRelationBrushProperty = DependencyProperty.Register(nameof(SyncRelationBrush), typeof(Brush), typeof(LineLayer),
            new PropertyMetadata(Brushes.DodgerBlue));

        public static readonly DependencyProperty FlickRelationBrushProperty = DependencyProperty.Register(nameof(FlickRelationBrush), typeof(Brush), typeof(LineLayer),
            new PropertyMetadata(Brushes.Orchid));

        public static readonly DependencyProperty HoldRelationBrushProperty = DependencyProperty.Register(nameof(HoldRelationBrush), typeof(Brush), typeof(LineLayer),
            new PropertyMetadata(Brushes.Yellow));

        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);
            DrawLines(drawingContext);
        }

        private void DrawLines(DrawingContext context) {
            var syncPen = new Pen(SyncRelationBrush, NoteLineThickness);
            var flickPen = new Pen(FlickRelationBrush, NoteLineThickness);
            var holdPen = new Pen(HoldRelationBrush, NoteLineThickness);
            foreach (var relation in NoteRelations) {
                var note1 = relation.ScoreNote1;
                var note2 = relation.ScoreNote2;
                Pen pen;
                switch (relation.Relation) {
                    case NoteRelation.None:
                        pen = null;
                        break;
                    case NoteRelation.Sync:
                        pen = syncPen;
                        break;
                    case NoteRelation.Flick:
                        pen = flickPen;
                        break;
                    case NoteRelation.Hold:
                        pen = holdPen;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(relation.Relation));
                }
                context.DrawLine(pen, new Point(note1.X, note1.Y), new Point(note2.X, note2.Y));
            }
        }

    }
}
