using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using DereTore.Applications.StarlightDirector.Components;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;
using Microsoft.Win32;

namespace DereTore.Applications.StarlightDirector.UI.Windows {
    partial class MainWindow {

        public static readonly ICommand CmdToolsBuildMusicArchive = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdToolsBuildScoreDatabase = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdToolsImportMusicArchive = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdToolsImportScoreDatabase = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdToolsExportScoreToCsv = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdToolsUtilitiesConvertSaveFormatV01 = CommandHelper.RegisterCommand();

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
            var saveDialog = new SaveFileDialog();
            saveDialog.OverwritePrompt = true;
            saveDialog.ValidateNames = true;
            saveDialog.Filter = Application.Current.FindResource<string>(App.ResourceKeys.CsvFileFilter);
            var result = saveDialog.ShowDialog();
            if (result ?? false) {
                Project.ExportScoreToCsv(Project.Difficulty, saveDialog.FileName);
                var prompt = string.Format(Application.Current.FindResource<string>(App.ResourceKeys.ExportToCsvCompletePromptTemplate), saveDialog.FileName);
                MessageBox.Show(prompt, Title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CmdToolsUtilitiesConvertSaveFormatV01_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void CmdToolsUtilitiesConvertSaveFormatV01_Executed(object sender, ExecutedRoutedEventArgs e) {
            var openDialog = new OpenFileDialog();
            openDialog.CheckPathExists = true;
            openDialog.ValidateNames = true;
            openDialog.Filter = Application.Current.FindResource<string>(App.ResourceKeys.ProjectFileV01Filter);
            var result = openDialog.ShowDialog();
            if (result ?? false) {
                var inputFileName = openDialog.FileName;
                var projectVersion = ProjectIO.CheckProjectFileVersion(inputFileName);
                string prompt;
                switch (projectVersion) {
                    case ProjectVersion.Unknown:
                        prompt = string.Format(Application.Current.FindResource<string>(App.ResourceKeys.ProjectVersionInvalidPromptTemplate), inputFileName);
                        MessageBox.Show(prompt, Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    case ProjectVersion.V0_2:
                        prompt = Application.Current.FindResource<string>(App.ResourceKeys.ProjectVersionUpToDatePrompt);
                        MessageBox.Show(prompt, Title, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                }
                var fileInfo = new FileInfo(inputFileName);
                var rawFileName = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
                rawFileName = rawFileName + "-" + Entities.Project.CurrentVersion + fileInfo.Extension;
                var outputFileName = Path.Combine(fileInfo.DirectoryName ?? string.Empty, rawFileName);
                var project = ProjectIO.LoadFromV01(inputFileName);
                ProjectIO.Save(project, outputFileName);
                prompt = string.Format(Application.Current.FindResource<string>(App.ResourceKeys.ConvertSaveFormatCompletePromptTemplate), inputFileName, outputFileName);
                MessageBox.Show(prompt, Title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

    }
}
