using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;

namespace DereTore.Applications.StarlightDirector.UI.Windows {
    partial class MainWindow {

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
            var backupFileName = GetBackupFileNameIfExists();
            if (backupFileName != null) {
                var format = Application.Current.FindResource<string>(App.ResourceKeys.AutoSaveFileFoundPromptTemplate);
                var message = string.Format(format, backupFileName);
                var messageResult = MessageBox.Show(message, App.Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                switch (messageResult) {
                    case MessageBoxResult.Yes:
                        LoadBackup();
                        ScrollViewer.ScrollToEnd();
                        break;
                    case MessageBoxResult.No:
                        Project = Project.Current;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(messageResult));
                }
            } else {
                Project = Project.Current;
            }
            _autoSaveTimer.Start();
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
            if (ScorePreviewer.IsPreviewing) {
                ScorePreviewer.EndPreview();
            }

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

        private void ScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
                if (e.Delta > 0) {
                    Editor.ZoomIn();
                } else {
                    Editor.ZoomOut();
                }
                e.Handled = true;
            }
        }

        private void ScrollSpeedComboBoxItem_OnSelected(object sender, RoutedEventArgs e) {
            var item = (ComboBoxItem)e.OriginalSource;
            var newValue = double.Parse((string)item.Content);
            if (ScrollViewer != null) {
                CustomScroll.SetScrollSpeed(ScrollViewer, newValue);
            }
        }

        private void Project_DifficultyChanged(object sender, EventArgs e) {
            var project = Project;
            Editor.Score = project?.GetScore(project.Difficulty);
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

        private void PreviewFpsComboBoxItem_Selected(object sender, RoutedEventArgs e) {
            var item = e.OriginalSource as ComboBoxItem;
            double fps;
            var s = item?.Content?.ToString();
            if (s == "Unlimited") {
                PreviewFps = double.PositiveInfinity;
                return;
            }

            if (double.TryParse(item?.Content?.ToString(), out fps)) {
                PreviewFps = fps;
            }
        }

        private void PreviewSpeedComboBoxItem_Selected(object sender, RoutedEventArgs e) {
            var item = e.OriginalSource as ComboBoxItem;
            int speed;
            if (int.TryParse(item?.Content?.ToString(), out speed)) {
                PreviewSpeed = speed;
            }
        }
    }
}
