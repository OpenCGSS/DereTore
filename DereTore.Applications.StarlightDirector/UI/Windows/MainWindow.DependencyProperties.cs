using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.Components;
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

        public static readonly DependencyProperty ProjectProperty = DependencyProperty.Register(nameof(Project), typeof(Project), typeof(MainWindow),
            new PropertyMetadata(null, OnProjectChanged));

        public static readonly DependencyProperty AccentColorBrushProperty = DependencyProperty.Register(nameof(AccentColorBrush), typeof(Brush), typeof(MainWindow),
            new PropertyMetadata(UIHelper.GetWindowColorizationBrush()));

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
