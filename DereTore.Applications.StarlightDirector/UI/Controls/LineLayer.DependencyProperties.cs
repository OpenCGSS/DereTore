using System.Windows;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.Extensions;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
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

        public static readonly DependencyProperty ConnectedNoteLineThicknessProperty = DependencyProperty.Register(nameof(ConnectedNoteLineThickness), typeof(double), typeof(LineLayer),
            new PropertyMetadata(16d));

        public static readonly DependencyProperty SyncNoteLineThicknessProperty = DependencyProperty.Register(nameof(SyncNoteLineThickness), typeof(double), typeof(LineLayer),
            new PropertyMetadata(6d));

        public static readonly DependencyProperty RelationBrushProperty = DependencyProperty.Register(nameof(RelationBrush), typeof(Brush), typeof(LineLayer),
            new PropertyMetadata(Application.Current.FindResource<Brush>(App.ResourceKeys.RelationBorderBrush)));

        public static readonly DependencyProperty SyncRelationBrushProperty = DependencyProperty.Register(nameof(SyncRelationBrush), typeof(Brush), typeof(LineLayer),
            new PropertyMetadata(Application.Current.FindResource<Brush>(App.ResourceKeys.SyncNoteBorderBrush)));

        public static readonly DependencyProperty FlickRelationBrushProperty = DependencyProperty.Register(nameof(FlickRelationBrush), typeof(Brush), typeof(LineLayer),
            new PropertyMetadata(Application.Current.FindResource<Brush>(App.ResourceKeys.FlickNoteBorderBrush)));

        public static readonly DependencyProperty HoldRelationBrushProperty = DependencyProperty.Register(nameof(HoldRelationBrush), typeof(Brush), typeof(LineLayer),
            new PropertyMetadata(Application.Current.FindResource<Brush>(App.ResourceKeys.HoldNoteBorderBrush)));

    }
}
