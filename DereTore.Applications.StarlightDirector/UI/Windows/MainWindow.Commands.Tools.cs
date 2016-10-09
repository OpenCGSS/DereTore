using System;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using DereTore.Applications.StarlightDirector.Components;
using DereTore.Applications.StarlightDirector.Conversion;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;
using Microsoft.Win32;
using ProjectIO = DereTore.Applications.StarlightDirector.Conversion.ProjectIO;

namespace DereTore.Applications.StarlightDirector.UI.Windows {
    partial class MainWindow {

        public static readonly ICommand CmdToolsBuildMusicArchive = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdToolsBuildScoreDatabase = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdToolsImportMusicArchive = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdToolsImportScoreDatabase = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdToolsImportDelesteBeatmap = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdToolsExportScoreToCsv = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdToolsExportScoreToInsideBdb = CommandHelper.RegisterCommand();
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

        private void CmdToolsImportDelesteBeatmap_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Project != null;
        }

        private void CmdToolsImportDelesteBeatmap_Executed(object sender, ExecutedRoutedEventArgs e) {
            var openDialog = new OpenFileDialog();
            openDialog.CheckFileExists = true;
            openDialog.ValidateNames = true;
            openDialog.Filter = Application.Current.FindResource<string>(App.ResourceKeys.DelesteTxtFileFilter);
            var dialogResult = openDialog.ShowDialog();
            if (dialogResult ?? false) {
                string[] warnings;
                var score = ScoreIO.LoadFromDelesteBeatmap(Project, Project.Difficulty, openDialog.FileName, out warnings);
                if (warnings != null) {
                    MessageBox.Show(warnings.BuildString(Environment.NewLine), Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    var messageBoxResult = MessageBox.Show("Continue?", Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (messageBoxResult == MessageBoxResult.No) {
                        return;
                    }
                }
                Project.SetScore(Project.Difficulty, score);
                Editor.Score = score;
                NotifyProjectChanged();
            }
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

        private void CmdToolsExportScoreToInsideBdb_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdToolsExportScoreToInsideBdb_Executed(object sender, ExecutedRoutedEventArgs e) {
            var saveDialog = new SaveFileDialog();
            saveDialog.OverwritePrompt = true;
            saveDialog.ValidateNames = true;
            saveDialog.Filter = Application.Current.FindResource<string>(App.ResourceKeys.BdbFileFilter);
            var saveResult = saveDialog.ShowDialog();
            if (saveResult ?? false) {
                try {
                    const string templateBdbPath = "Resources/Testing/musicscores_m001.bdb";
                    var templateFileInfo = new FileInfo(templateBdbPath);
                    File.Copy(templateFileInfo.FullName, saveDialog.FileName, true);
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                var difficulty = Project.Difficulty;
                var csv = Project.ExportScoreToCsv(difficulty);
                var fileName = saveDialog.FileName;
                try {
                    string prompt;
                    bool operationSucceeded = false;
                    using (var connection = new SQLiteConnection($"Data Source={fileName}")) {
                        connection.Open();
                        using (var command = connection.CreateCommand()) {
                            command.CommandText = $@"SELECT name FROM blobs WHERE name LIKE 'musicscores/m___/%\_{(int)difficulty}.csv' ESCAPE '\';";
                            using (var adapter = new SQLiteDataAdapter(command)) {
                                using (var dataTable = new DataTable()) {
                                    adapter.Fill(dataTable);
                                    if (dataTable.Rows.Count > 0) {
                                        string nameToReplace = null;
                                        var testRegex = new Regex($@"^musicscores/m[\d]+/[\d]+_{(int)difficulty}.csv$");
                                        foreach (DataRow row in dataTable.Rows) {
                                            var name = (string)row[0];
                                            if (testRegex.IsMatch(name)) {
                                                nameToReplace = name;
                                                break;
                                            }
                                        }
                                        if (nameToReplace != null) {
                                            command.CommandText = "UPDATE blobs SET data = @value WHERE name = @name;";
                                            command.Parameters.Add("name", DbType.AnsiString).Value = nameToReplace;
                                            command.Parameters.Add("value", DbType.AnsiString).Value = csv;
                                            command.ExecuteNonQuery();
                                            operationSucceeded = true;
                                        } else {
                                            prompt = string.Format(Application.Current.FindResource<string>(App.ResourceKeys.NoCorrespondingDifficultyExistsPromptTemplate),
                                                DescribedEnumReader.Read(difficulty, typeof(Difficulty)), fileName);
                                            MessageBox.Show(prompt, Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                        }
                                    } else {
                                        prompt = string.Format(Application.Current.FindResource<string>(App.ResourceKeys.NoCorrespondingDifficultyExistsPromptTemplate),
                                            DescribedEnumReader.Read(difficulty, typeof(Difficulty)), fileName);
                                        MessageBox.Show(prompt, Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                    }
                                }
                            }
                        }
                        connection.Close();
                    }
                    if (operationSucceeded) {
                        prompt = string.Format(Application.Current.FindResource<string>(App.ResourceKeys.ExportAndReplaceBdbCompletePromptTemplate), fileName,
                            DescribedEnumReader.Read(difficulty, typeof(Difficulty)));
                        MessageBox.Show(prompt, Title, MessageBoxButton.OK, MessageBoxImage.Information);
                    } else {
                        prompt = Application.Current.FindResource<string>(App.ResourceKeys.ErrorOccurredPrompt);
                        MessageBox.Show(prompt, Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
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
