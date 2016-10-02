using System.Windows;
using System.Windows.Media;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    partial class LineLayer {

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

    }
}
