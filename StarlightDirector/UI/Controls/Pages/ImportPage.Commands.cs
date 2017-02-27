using System;
using System.Windows;
using System.Windows.Input;
using DereTore;
using Fluent;
using Microsoft.Win32;
using StarlightDirector.Entities;
using StarlightDirector.Exchange;
using StarlightDirector.Extensions;
using StarlightDirector.UI.Windows;

namespace StarlightDirector.UI.Controls.Pages {
    partial class ImportPage {

        public static readonly ICommand CmdImportDelesteBeatmap = CommandHelper.RegisterCommand();

        private void CmdImportDelesteBeatmap_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void CmdImportDelesteBeatmap_Executed(object sender, ExecutedRoutedEventArgs e) {
            var mainWindow = this.GetMainWindow();
            var messageBoxResult = MessageBox.Show(Application.Current.FindResource<string>(App.ResourceKeys.DelesteImportingWillReplaceCurrentScorePrompt), App.Title, MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (messageBoxResult == MessageBoxResult.Yes) {
                MainWindow.CmdFileSaveProject.Execute(null);
            }
            var openDialog = new OpenFileDialog();
            openDialog.CheckFileExists = true;
            openDialog.ValidateNames = true;
            openDialog.Filter = Application.Current.FindResource<string>(App.ResourceKeys.DelesteTxtFileFilter);
            var dialogResult = openDialog.ShowDialog();
            if (dialogResult ?? false) {
                string[] warnings;
                bool hasErrors;
                var project = mainWindow.Project;
                var tempProject = new Project();
                tempProject.Settings.CopyFrom(project.Settings);
                var score = ScoreIO.LoadFromDelesteBeatmap(tempProject, mainWindow.Project.Difficulty, openDialog.FileName, out warnings, out hasErrors);
                if (warnings != null) {
                    MessageBox.Show(warnings.BuildString(Environment.NewLine), App.Title, MessageBoxButton.OK, hasErrors ? MessageBoxImage.Error : MessageBoxImage.Exclamation);
                    if (!hasErrors) {
                        messageBoxResult = MessageBox.Show(Application.Current.FindResource<string>(App.ResourceKeys.DelesteWarningsAppearedPrompt), App.Title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                        if (messageBoxResult == MessageBoxResult.No) {
                            return;
                        }
                    } else {
                        return;
                    }
                }
                // Redirect project reference.
                score.Project = project;
                project.SetScore(project.Difficulty, score);
                project.Settings.CopyFrom(tempProject.Settings);
                mainWindow.Editor.Score = score;
                mainWindow.NotifyProjectChanged();
                var backstage = (Backstage)mainWindow.Ribbon.Menu;
                backstage.IsOpen = false;
            }
        }

    }
}
