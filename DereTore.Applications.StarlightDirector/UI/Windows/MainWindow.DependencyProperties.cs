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
