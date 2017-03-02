using System.Windows;
using System.Windows.Media;
using StarlightDirector.Extensions;

namespace StarlightDirector.UI.Controls.Primitives {
    partial class LineLayer {

        public double ConnectedNoteLineThickness {
            get { return (double)GetValue(ConnectedNoteLineThicknessProperty); }
            set { SetValue(ConnectedNoteLineThicknessProperty, value); }
        }

        public double SyncNoteLineThickness {
            get { return (double)GetValue(SyncNoteLineThicknessProperty); }
            set { SetValue(SyncNoteLineThicknessProperty, value); }
        }

        public Brush RelationBrush {
            get { return (Brush)GetValue(RelationBrushProperty); }
            set { SetValue(RelationBrushProperty, value); }
        }

        public static readonly DependencyProperty ConnectedNoteLineThicknessProperty = DependencyProperty.Register(nameof(ConnectedNoteLineThickness), typeof(double), typeof(LineLayer),
            new PropertyMetadata(16d));

        public static readonly DependencyProperty SyncNoteLineThicknessProperty = DependencyProperty.Register(nameof(SyncNoteLineThickness), typeof(double), typeof(LineLayer),
            new PropertyMetadata(6d));

        public static readonly DependencyProperty RelationBrushProperty = DependencyProperty.Register(nameof(RelationBrush), typeof(Brush), typeof(LineLayer),
            new PropertyMetadata(Application.Current.FindResource<Brush>(App.ResourceKeys.RelationBorderBrush)));

    }
}
