using System;
using System.ComponentModel;
using System.Windows;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;
using DereTore.Applications.StarlightDirector.Interop;

namespace DereTore.Applications.StarlightDirector.UI.Windows {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow {

        public MainWindow() {
            InitializeComponent();
            CommandHelper.InitializeCommandBindings(this);
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
            Project = Project.Current;
        }

        private void Project_DifficultyChanged(object sender, EventArgs e) {
            var project = Project;
            Editor.Score = project?.GetScore(project.Difficulty);
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e) {
            if (!ShouldPromptSaving) {
                return;
            }
            var result = MessageBox.Show(Application.Current.FindResource<string>(App.ResourceKeys.ProjectChangedPrompt), App.Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);
            switch (result) {
                case MessageBoxResult.Yes:
                    if (CmdFileSaveProject.CanExecute(null)) {
                        CmdFileSaveProject.Execute(null);
                    }
                    if (!Project.IsSaved) {
                        e.Cancel = true;
                    }
                    break;
                case MessageBoxResult.No:
                    break;
                case MessageBoxResult.Cancel:
                    e.Cancel = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }

        private void MainWindow_OnSourceInitialized(object sender, EventArgs e) {
            this.RegisterWndProc(WndProc);
        }

        private void OnDwmColorizationColorChanged(object sender, EventArgs e) {
            AccentColorBrush = UIHelper.GetWindowColorizationBrush();
        }

        private IntPtr WndProc(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            switch (uMsg) {
                case NativeConstants.WM_DWMCOLORIZATIONCOLORCHANGED:
                    OnDwmColorizationColorChanged(this, EventArgs.Empty);
                    return IntPtr.Zero;
                default:
                    return IntPtr.Zero;
            }
        }

        private void NotifyProjectChanged() {
            if (Project != null) {
                Project.IsChanged = true;
            }
        }

        private bool ShouldPromptSaving => Project != null && Project.IsChanged;

    }
}
