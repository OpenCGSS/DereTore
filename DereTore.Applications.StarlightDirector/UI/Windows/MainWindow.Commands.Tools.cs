using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using DereTore.Applications.StarlightDirector.Components;
using DereTore.Applications.StarlightDirector.Extensions;
using Microsoft.Win32;

namespace DereTore.Applications.StarlightDirector.UI.Windows {
    partial class MainWindow {

        public static readonly ICommand CmdToolsBuildMusicArchive = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdToolsBuildScoreDatabase = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdToolsImportMusicArchive = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdToolsImportScoreDatabase = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdToolsExportScoreToCsv = CommandHelper.RegisterCommand();

        private void CmdToolsBuildMusicArchive_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Project?.HasMusic ?? false;
        }

        private void CmdToolsBuildMusicArchive_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.Print("Not implemented: build music archive");
            NotifyProjectChanged();
        }

        private void CmdToolsBuildScoreDatabase_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = (Editor.Project?.Scores?.Count ?? 0) > 0 && false;
        }

        private void CmdToolsBuildScoreDatabase_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.Print("Not implemented: build score database");
            NotifyProjectChanged();
        }

        private void CmdToolsImportMusicArchive_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = false;
        }

        private void CmdToolsImportMusicArchive_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.Print("Not implemented: import music archive");
            NotifyProjectChanged();
        }

        private void CmdToolsImportScoreDatabase_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = false;
        }

        private void CmdToolsImportScoreDatabase_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.Print("Not implemented: import score database");
            NotifyProjectChanged();
        }

        private void CmdToolsExportScoreToCsv_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdToolsExportScoreToCsv_Executed(object sender, ExecutedRoutedEventArgs e) {
            //var difficulty = (Difficulty)(DifficultySelector.SelectedIndex + 1);
            var saveDialog = new SaveFileDialog();
            saveDialog.OverwritePrompt = true;
            saveDialog.ValidateNames = true;
            saveDialog.Filter = Application.Current.FindResource<string>(App.ResourceKeys.CsvFileFilter);
            var result = saveDialog.ShowDialog();
            if (result ?? false) {
                Project.SaveScoreToCsv(Project.Difficulty, saveDialog.FileName);
            }
        }

    }
}
