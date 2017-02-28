using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using StarlightDirector.Exchange;
using StarlightDirector.Extensions;

namespace StarlightDirector.UI.Controls.Pages {
    partial class ExportPage {

        public static readonly ICommand CmdExportToDelesteBeatmap = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdExportToCsv = CommandHelper.RegisterCommand();

        private void CmdExportToDelesteBeatmap_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            var mainWindow = this.GetMainWindow();
            if (mainWindow == null) {
                e.CanExecute = false;
            } else {
                e.CanExecute = mainWindow.Editor.Score != null;
            }
        }

        private void CmdExportToDelesteBeatmap_Executed(object sender, ExecutedRoutedEventArgs e) {
            var mainWindow = this.GetMainWindow();
            var saveDialog = new SaveFileDialog();
            saveDialog.OverwritePrompt = true;
            saveDialog.ValidateNames = true;
            saveDialog.Filter = Application.Current.FindResource<string>(App.ResourceKeys.DelesteTxtFileFilter);
            var result = saveDialog.ShowDialog();
            if (result ?? false) {
                ScoreIO.ExportToDelesteBeatmap(mainWindow.Editor.Score, saveDialog.FileName);
                var prompt = string.Format(Application.Current.FindResource<string>(App.ResourceKeys.ExportToDelesteBeatmapCompletePromptTemplate), saveDialog.FileName);
                MessageBox.Show(prompt, App.Title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CmdExportToCsv_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            var mainWindow = this.GetMainWindow();
            if (mainWindow == null) {
                e.CanExecute = false;
            } else {
                e.CanExecute = mainWindow.Editor.Score != null;
            }
        }

        private void CmdExportToCsv_Executed(object sender, ExecutedRoutedEventArgs e) {
            var mainWindow = this.GetMainWindow();
            var saveDialog = new SaveFileDialog();
            saveDialog.OverwritePrompt = true;
            saveDialog.ValidateNames = true;
            saveDialog.Filter = Application.Current.FindResource<string>(App.ResourceKeys.CsvFileFilter);
            var result = saveDialog.ShowDialog();
            if (result ?? false) {
                mainWindow.Project.ExportScoreToCsv(mainWindow.Project.Difficulty, saveDialog.FileName);
                var prompt = string.Format(Application.Current.FindResource<string>(App.ResourceKeys.ExportToCsvCompletePromptTemplate), saveDialog.FileName);
                MessageBox.Show(prompt, App.Title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

    }
}
