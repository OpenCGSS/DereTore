using System.Windows;
using System.Windows.Input;
using StarlightDirector.Entities;

namespace StarlightDirector.UI.Controls {
    partial class ScoreEditor {

        public EditMode EditMode {
            get { return (EditMode)GetValue(EditModeProperty); }
            set { SetValue(EditModeProperty, value); }
        }

        public Project Project {
            get { return (Project)GetValue(ProjectProperty); }
            set { SetValue(ProjectProperty, value); }
        }

        public static readonly DependencyProperty EditModeProperty = DependencyProperty.Register(nameof(EditMode), typeof(EditMode), typeof(ScoreEditor),
            new PropertyMetadata(EditMode.Select));

        public static readonly DependencyProperty ProjectProperty = DependencyProperty.Register(nameof(Project), typeof(Project), typeof(ScoreEditor),
            new PropertyMetadata(null, OnProjectChanged));

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
