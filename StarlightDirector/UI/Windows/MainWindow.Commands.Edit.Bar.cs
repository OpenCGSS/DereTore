using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using StarlightDirector.Extensions;

namespace StarlightDirector.UI.Windows {
    partial class MainWindow {

        public static readonly ICommand CmdEditBarAppend = CommandHelper.RegisterCommand("Ctrl+Alt+U");
        public static readonly ICommand CmdEditBarAppendMany = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdEditBarInsert = CommandHelper.RegisterCommand("Ctrl+Alt+I");
        public static readonly ICommand CmdEditBarEdit = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdEditBarDelete = CommandHelper.RegisterCommand("Ctrl+Delete");

        private void CmdEditBarAppend_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdEditBarAppend_Executed(object sender, ExecutedRoutedEventArgs e) {
            Editor.AppendScoreBar();
            NotifyProjectChanged();
        }

        private void CmdEditBarAppendMany_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdEditBarAppendMany_Executed(object sender, ExecutedRoutedEventArgs e) {
            var count = (int)(double)e.Parameter;
            Editor.AppendScoreBars(count);
            NotifyProjectChanged();
        }

        private void CmdEditBarInsert_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSingleSelectedScoreBar;
        }

        private void CmdEditBarInsert_Executed(object sender, ExecutedRoutedEventArgs e) {
            var scoreBar = Editor.GetSelectedScoreBar();
            if (scoreBar != null) {
                var newScoreBar = Editor.InsertScoreBar(scoreBar);
                Editor.SelectScoreBar(newScoreBar);
                Editor.ScrollToScoreBar(newScoreBar);
                NotifyProjectChanged();
            }
        }

        private void CmdEditBarEdit_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSingleSelectedScoreBar && false;
        }

        private void CmdEditBarEdit_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.Print("Not implemented: edit bar");
            NotifyProjectChanged();
        }

        private void CmdEditBarDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSingleSelectedScoreBar;
        }

        private void CmdEditBarDelete_Executed(object sender, ExecutedRoutedEventArgs e) {
            var scoreBar = Editor.GetSelectedScoreBar();
            if (scoreBar != null) {
                var result = MessageBox.Show(Application.Current.FindResource<string>(App.ResourceKeys.ConfirmDeleteBarPrompt), App.Title, MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                switch (result) {
                    case MessageBoxResult.Yes:
                        Editor.RemoveScoreBar(scoreBar);
                        NotifyProjectChanged();
                        break;
                    case MessageBoxResult.No:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(result));
                }
            }
        }

    }
}
