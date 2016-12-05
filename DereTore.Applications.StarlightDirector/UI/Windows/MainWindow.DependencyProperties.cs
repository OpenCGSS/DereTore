using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector.UI.Windows {
    partial class MainWindow {

        public Project Project {
            get { return (Project)GetValue(ProjectProperty); }
            set { SetValue(ProjectProperty, value); }
        }

        public Brush AccentColorBrush {
            get { return (Brush)GetValue(AccentColorBrushProperty); }
            private set { SetValue(AccentColorBrushProperty, value); }
        }

        public bool IsTemporaryMessageVisible {
            get { return (bool)GetValue(IsTemporaryMessageVisibleProperty); }
            private set { SetValue(IsTemporaryMessageVisibleProperty, value); }
        }

        public string TemporaryMessage {
            get { return (string)GetValue(TemporaryMessageProperty); }
            private set { SetValue(TemporaryMessageProperty, value); }
        }

        public double PreviewFps
        {
            get { return (double)GetValue(PreviewFpsProperty); }
            set { SetValue(PreviewFpsProperty, value); }
        }

        public bool PreviewFromStart
        {
            get { return (bool)GetValue(PreviewFromStartProperty); }
            set { SetValue(PreviewFromStartProperty, value); }
        }

        public int PreviewSpeed
        {
            get { return (int)GetValue(PreviewSpeedProperty); }
            set { SetValue(PreviewSpeedProperty, value); }
        }

        public double PreviewStartOffset
        {
            get { return (double)GetValue(PreviewStartOffsetProperty); }
            set { SetValue(PreviewStartOffsetProperty, value); }
        }

        public static readonly DependencyProperty PreviewStartOffsetProperty =
            DependencyProperty.Register("PreviewStartOffset", typeof(double), typeof(MainWindow), new PropertyMetadata(0.0));

        public static readonly DependencyProperty PreviewSpeedProperty =
            DependencyProperty.Register("PreviewSpeed", typeof(int), typeof(MainWindow), new PropertyMetadata(8));

        public static readonly DependencyProperty PreviewFromStartProperty =
            DependencyProperty.Register("PreviewFromStart", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public static readonly DependencyProperty PreviewFpsProperty =
            DependencyProperty.Register("PreviewFps", typeof(double), typeof(MainWindow), new PropertyMetadata(60.0));

        public static readonly DependencyProperty ProjectProperty = DependencyProperty.Register(nameof(Project), typeof(Project), typeof(MainWindow),
            new PropertyMetadata(null, OnProjectChanged));

        public static readonly DependencyProperty AccentColorBrushProperty = DependencyProperty.Register(nameof(AccentColorBrush), typeof(Brush), typeof(MainWindow),
            new PropertyMetadata(UIHelper.GetWindowColorizationBrush()));

        public static readonly DependencyProperty IsTemporaryMessageVisibleProperty = DependencyProperty.Register(nameof(IsTemporaryMessageVisible), typeof(bool), typeof(MainWindow),
        new PropertyMetadata(false));

        public static readonly DependencyProperty TemporaryMessageProperty = DependencyProperty.Register(nameof(TemporaryMessage), typeof(string), typeof(MainWindow),
        new PropertyMetadata(string.Empty));

        private static void OnProjectChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var window = obj as MainWindow;
            var newProject = e.NewValue as Project;
            var oldProject = e.OldValue as Project;
            Debug.Assert(window != null);
            window.Editor.Project = newProject;
            window.Editor.Score = newProject?.GetScore(newProject.Difficulty);

            if (oldProject != null) {
                oldProject.DifficultyChanged -= window.Project_DifficultyChanged;
            }
            if (newProject != null) {
                newProject.DifficultyChanged += window.Project_DifficultyChanged;
            }
        }

    }
}
