using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;
using DereTore.Applications.StarlightDirector.Interop;

namespace DereTore.Applications.StarlightDirector.UI.Windows {
    public partial class MainWindow {

        public MainWindow() {
            InitializeComponent();
            CommandHelper.InitializeCommandBindings(this);
            _temporaryMessageTimer = new Timer(TemporaryMessageTimeout) {
                AutoReset = true
            };
            _temporaryMessageTimer.Elapsed += TemporaryMessageTimer_OnElapsed;
            _autoSaveTimer = new Timer(AutoSaveInterval);
            _autoSaveTimer.Elapsed += AutoSaveTimer_OnElapsed;
        }

        public void ShowTemporaryMessage(string message) {
            if (string.IsNullOrEmpty(message)) {
                return;
            }
            TemporaryMessage = message;
            if (IsTemporaryMessageVisible) {
                _temporaryMessageTimer.Stop();
                _temporaryMessageTimer.Start();
            } else {
                IsTemporaryMessageVisible = true;
                _temporaryMessageTimer.Start();
            }
        }

        internal void NotifyProjectChanged() {
            if (Project != null) {
                Project.IsChanged = true;
            }
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
            var backupFileName = GetBackupFileNameIfExists();
            if (backupFileName != null) {
                var format = Application.Current.FindResource<string>(App.ResourceKeys.AutoSaveFileFoundPromptTemplate);
                var message = string.Format(format, backupFileName);
                var messageResult = MessageBox.Show(message, App.Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                switch (messageResult) {
                    case MessageBoxResult.Yes:
                        LoadBackup();
                        break;
                    case MessageBoxResult.No:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(messageResult));
                }
            } else {
                Project = Project.Current;
            }
            _autoSaveTimer.Start();
        }

        private void Project_DifficultyChanged(object sender, EventArgs e) {
            var project = Project;
            Editor.Score = project?.GetScore(project.Difficulty);
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e) {
            if (ShouldPromptSaving) {
                var result = MessageBox.Show(Application.Current.FindResource<string>(App.ResourceKeys.ProjectChangedPrompt), App.Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);
                switch (result) {
                    case MessageBoxResult.Yes:
                        if (CmdFileSaveProject.CanExecute(null)) {
                            CmdFileSaveProject.Execute(null);
                        }
                        if (!Project.IsSaved) {
                            e.Cancel = true;
                            return;
                        }
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        return;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(result), result, null);
                }
            }
            _autoSaveTimer.Stop();
            ClearBackup();

            _temporaryMessageTimer.Elapsed -= TemporaryMessageTimer_OnElapsed;
            _temporaryMessageTimer?.Dispose();
            _temporaryMessageTimer = null;
            _autoSaveTimer.Elapsed -= AutoSaveTimer_OnElapsed;
            _autoSaveTimer?.Dispose();
            _autoSaveTimer = null;
        }

        private void MainWindow_OnSourceInitialized(object sender, EventArgs e) {
            this.RegisterWndProc(WndProc);
        }

        private void OnDwmColorizationColorChanged(object sender, EventArgs e) {
            AccentColorBrush = UIHelper.GetWindowColorizationBrush();
        }

        private void TemporaryMessageTimer_OnElapsed(object sender, ElapsedEventArgs e) {
            Dispatcher.Invoke(new Action(() => {
                _temporaryMessageTimer.Stop();
                IsTemporaryMessageVisible = false;
                TemporaryMessage = string.Empty;
            }));
        }

        private void AutoSaveTimer_OnElapsed(object sender, ElapsedEventArgs e) {
            Dispatcher.Invoke(new Action(SaveBackup));
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

        private bool ShouldPromptSaving => Project != null && Project.IsChanged;

        private Timer _temporaryMessageTimer;
        private Timer _autoSaveTimer;

        private static readonly double TemporaryMessageTimeout = TimeSpan.FromSeconds(6).TotalMilliseconds;
        private static readonly double AutoSaveInterval = TimeSpan.FromMinutes(3).TotalMilliseconds;

    }
}
