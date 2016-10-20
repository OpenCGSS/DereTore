using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Exchange;
using DereTore.Applications.StarlightDirector.Extensions;
using Microsoft.Win32;

namespace DereTore.Applications.StarlightDirector.UI.Windows {
    partial class MainWindow {

        public static readonly ICommand CmdFileNewProject = CommandHelper.RegisterCommand("Ctrl+N");
        public static readonly ICommand CmdFileOpenProject = CommandHelper.RegisterCommand("Ctrl+O");
        public static readonly ICommand CmdFileSaveProject = CommandHelper.RegisterCommand("Ctrl+S");
        public static readonly ICommand CmdFileSaveProjectAs = CommandHelper.RegisterCommand("F12");
        public static readonly ICommand CmdFilePreferences = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdFileExit = CommandHelper.RegisterCommand("Ctrl+W", "Ctrl+Shift+Q");

        private void CmdFileNewProject_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void CmdFileNewProject_Executed(object sender, ExecutedRoutedEventArgs e) {
            if (ShouldPromptSaving) {
                var result = MessageBox.Show(Application.Current.FindResource<string>(App.ResourceKeys.ProjectChangedPrompt), App.Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);
                switch (result) {
                    case MessageBoxResult.Yes:
                        CmdFileSaveProject.Execute(null);
                        return;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(result), result, null);
                }
            }
            var project = new Project();
            Project.Current = project;
            Project = project;
            _autoSaveTimer.Stop();
            _autoSaveTimer.Start();
            ClearBackup();
            ScrollViewer.ScrollToEnd();
        }

        private void CmdFileOpenProject_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void CmdFileOpenProject_Executed(object sender, ExecutedRoutedEventArgs e) {
            if (ShouldPromptSaving) {
                var result = MessageBox.Show(Application.Current.FindResource<string>(App.ResourceKeys.ProjectChangedPrompt), App.Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);
                switch (result) {
                    case MessageBoxResult.Yes:
                        CmdFileSaveProject.Execute(null);
                        return;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(result), result, null);
                }
            }
            var openDialog = new OpenFileDialog();
            openDialog.CheckPathExists = true;
            openDialog.Multiselect = false;
            openDialog.ShowReadOnly = false;
            openDialog.ReadOnlyChecked = false;
            openDialog.ValidateNames = true;
            openDialog.Filter = Application.Current.FindResource<string>(App.ResourceKeys.ProjectFileFilter);
            var dialogResult = openDialog.ShowDialog();
            if (!(dialogResult ?? false)) {
                return;
            }
            var fileName = openDialog.FileName;
            var projectVersion = ProjectIO.CheckProjectFileVersion(fileName);
            string prompt;
            var projectChanged = false;
            switch (projectVersion) {
                case ProjectVersion.Unknown:
                    prompt = string.Format(Application.Current.FindResource<string>(App.ResourceKeys.ProjectVersionInvalidPromptTemplate), fileName);
                    MessageBox.Show(prompt, Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                case ProjectVersion.V0_1:
                case ProjectVersion.V0_2:
                    prompt = string.Format(Application.Current.FindResource<string>(App.ResourceKeys.ProjectUpgradeNeededPromptTemplate), fileName);
                    MessageBox.Show(prompt, Title, MessageBoxButton.OK, MessageBoxImage.Information);
                    projectChanged = true;
                    break;
            }
            var project = ProjectIO.Load(fileName, projectVersion);
            Project = Editor.Project = Project.Current = project;
            project.IsChanged = projectChanged;
            Editor.Score = project.GetScore(project.Difficulty);
            _autoSaveTimer.Stop();
            _autoSaveTimer.Start();
            ClearBackup();
            SaveBackup();
            ScrollViewer.ScrollToEnd();
        }

        private void CmdFileSaveProject_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = ShouldPromptSaving;
        }

        private void CmdFileSaveProject_Executed(object sender, ExecutedRoutedEventArgs e) {
            var project = Project;
            if (!project.IsSaved) {
                if (CmdFileSaveProjectAs.CanExecute(e.Parameter)) {
                    CmdFileSaveProjectAs.Execute(e.Parameter);
                } else {
                    throw new InvalidOperationException();
                }
            } else {
                ProjectIO.Save(project);
                CmdFileSaveProject.RaiseCanExecuteChanged();
                _autoSaveTimer.Stop();
                _autoSaveTimer.Start();
                ClearBackup();
                SaveBackup();
            }
        }

        private void CmdFileSaveProjectAs_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void CmdFileSaveProjectAs_Executed(object sender, ExecutedRoutedEventArgs e) {
            var saveDialog = new SaveFileDialog();
            saveDialog.OverwritePrompt = true;
            saveDialog.ValidateNames = true;
            saveDialog.Filter = Application.Current.FindResource<string>(App.ResourceKeys.ProjectFileFilter);
            var result = saveDialog.ShowDialog();
            if (!(result ?? false)) {
                return;
            }
            ProjectIO.Save(Project, saveDialog.FileName);
            CmdFileSaveProjectAs.RaiseCanExecuteChanged();
            _autoSaveTimer.Stop();
            _autoSaveTimer.Start();
            ClearBackup();
            SaveBackup();
        }

        private void CmdFilePreferences_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = false;
        }

        private void CmdFilePreferences_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.Print("Not implemented: Preferences");
        }

        private void CmdFileExit_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void CmdFileExit_Executed(object sender, ExecutedRoutedEventArgs e) {
            Close();
        }

    }
}
