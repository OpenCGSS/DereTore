using System.Windows;
using System.Windows.Controls;
using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    partial class ScoreViewerBase {

        public bool AreRelationIndicatorsVisible {
            get { return (bool)GetValue(AreRelationIndicatorsVisibleProperty); }
            set { SetValue(AreRelationIndicatorsVisibleProperty, value); }
        }

        public Score Score {
            get { return (Score)GetValue(ScoreProperty); }
            set { SetValue(ScoreProperty, value); }
        }

        public ScrollViewer ScrollViewer {
            get { return (ScrollViewer)GetValue(ScrollViewerProperty); }
            set { SetValue(ScrollViewerProperty, value); }
        }

        public double MinimumScrollOffset {
            get { return (double)GetValue(MinimumScrollOffsetProperty); }
            protected set { SetValue(MinimumScrollOffsetProperty, value); }
        }

        public static readonly DependencyProperty MinimumScrollOffsetProperty = DependencyProperty.Register(nameof(MinimumScrollOffset), typeof(double), typeof(ScoreViewerBase),
            new PropertyMetadata(-45d));

        public static readonly DependencyProperty AreRelationIndicatorsVisibleProperty = DependencyProperty.Register(nameof(AreRelationIndicatorsVisible), typeof(bool), typeof(ScoreViewerBase),
         new PropertyMetadata(false));

        public static readonly DependencyProperty ScoreProperty = DependencyProperty.Register(nameof(Score), typeof(Score), typeof(ScoreViewerBase),
            new PropertyMetadata(null, OnScoreChanged));

        public static readonly DependencyProperty ScrollViewerProperty = DependencyProperty.Register(nameof(ScrollViewer), typeof(ScrollViewer), typeof(ScoreViewerBase),
            new PropertyMetadata(null));

        private static void OnScoreChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var viewerBase = (ScoreViewerBase)obj;
            var newScore = (Score)e.NewValue;
            viewerBase.ReloadScore(newScore);
        }
    }
}
