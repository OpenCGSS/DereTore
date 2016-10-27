using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    partial class ScoreEditor {

        public double MinimumScrollOffset {
            get { return (double)GetValue(MinimumScrollOffsetProperty); }
            private set { SetValue(MinimumScrollOffsetProperty, value); }
        }

        public Score Score {
            get { return (Score)GetValue(ScoreProperty); }
            set { SetValue(ScoreProperty, value); }
        }

        public EditMode EditMode {
            get { return (EditMode)GetValue(EditModeProperty); }
            set { SetValue(EditModeProperty, value); }
        }

        public Project Project {
            get { return (Project)GetValue(ProjectProperty); }
            set { SetValue(ProjectProperty, value); }
        }

        public double SmallChange {
            get { return (double)GetValue(SmallChangeProperty); }
            set { SetValue(SmallChangeProperty, value); }
        }

        public double LargeChange {
            get { return (double)GetValue(LargeChangeProperty); }
            set { SetValue(LargeChangeProperty, value); }
        }

        public ScrollViewer ScrollViewer {
            get { return (ScrollViewer)GetValue(ScrollViewerProperty); }
            set { SetValue(ScrollViewerProperty, value); }
        }

        public static readonly DependencyProperty MinimumScrollOffsetProperty = DependencyProperty.Register(nameof(MinimumScrollOffset), typeof(double), typeof(ScoreEditor),
             new PropertyMetadata(-45d));

        public static readonly DependencyProperty ScoreProperty = DependencyProperty.Register(nameof(Score), typeof(Score), typeof(ScoreEditor),
            new PropertyMetadata(null, OnScoreChanged));

        public static readonly DependencyProperty EditModeProperty = DependencyProperty.Register(nameof(EditMode), typeof(EditMode), typeof(ScoreEditor),
            new PropertyMetadata(EditMode.Select));

        public static readonly DependencyProperty ProjectProperty = DependencyProperty.Register(nameof(Project), typeof(Project), typeof(ScoreEditor),
            new PropertyMetadata(null, OnProjectChanged));

        public static readonly DependencyProperty SmallChangeProperty = DependencyProperty.Register(nameof(SmallChange), typeof(double), typeof(ScoreEditor),
            new PropertyMetadata(50d));

        public static readonly DependencyProperty LargeChangeProperty = DependencyProperty.Register(nameof(LargeChange), typeof(double), typeof(ScoreEditor),
            new PropertyMetadata(250d));

        public static readonly DependencyProperty ScrollViewerProperty = DependencyProperty.Register(nameof(ScrollViewer), typeof(ScrollViewer), typeof(ScoreEditor),
            new PropertyMetadata(null));

        private static void OnScoreChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var editor = obj as ScoreEditor;
            Debug.Assert(editor != null, "editor != null");
            var newScore = (Score)e.NewValue;
            editor.ReloadScore(newScore);
        }

        private static void OnProjectChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var editor = (ScoreEditor)obj;
            var oldproject = (Project)e.OldValue;
            var newProject = (Project)e.NewValue;
            if (oldproject != null) {
                oldproject.GlobalSettingsChanged -= editor.OnScoreGlobalSettingsChanged;
            }
            if (newProject != null) {
                newProject.GlobalSettingsChanged += editor.OnScoreGlobalSettingsChanged;
            }
            CommandManager.InvalidateRequerySuggested();
        }

    }
}
