using System;
using System.IO;
using System.Linq;
using System.Windows;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Exchange;
using DereTore.Applications.StarlightDirector.Extensions;

namespace DereTore.Applications.StarlightDirector.UI.Windows {
    partial class MainWindow {

        private void SaveBackup() {
            if (Project == null) {
                return;
            }
            var path = App.GetDirectoryPath(App.DirectorPath.AutoBackup);
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            string name;
            if (!string.IsNullOrEmpty(Project.SaveFileName)) {
                var fileInfo = new FileInfo(Project.SaveFileName);
                name = fileInfo.Name;
            } else {
                name = "unnamed";
            }
            path = Path.Combine(path, name);
            ProjectIO.SaveAsBackup(Project, path);
            var format = Application.Current.FindResource<string>(App.ResourceKeys.ProjectAutoSavedToPromptTemplate);
            var message = string.Format(format, path);
            ShowTemporaryMessage(message);
        }

        private void ClearBackup() {
            var directoryPath = App.GetDirectoryPath(App.DirectorPath.AutoBackup);
            if (!Directory.Exists(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
            }
            var directory = new DirectoryInfo(directoryPath);
            var files = directory.GetFiles();
            foreach (var fileInfo in files) {
                if (fileInfo.Exists) {
                    try {
                        // It happens sometimes. E.g., when Starlight Director is writing the autosave file at the same time.
                        fileInfo.Delete();
                    } catch (IOException ex) {
                        MessageBox.Show(ex.Message, App.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
            }
        }

        private void LoadBackup() {
            var path = App.GetDirectoryPath(App.DirectorPath.AutoBackup);
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            var directory = new DirectoryInfo(path);
            var fileInfo = directory.EnumerateFiles().FirstOrDefault();
            if (fileInfo == null) {
                return;
            }
            Project project;
            string format, message;
            try {
                project = ProjectIO.Load(fileInfo.FullName);
            } catch (Exception ex) {
                // In case the autosave also failed.
                var detailedMessage = ex.Message + Environment.NewLine + ex.StackTrace;
                format = Application.Current.FindResource<string>(App.ResourceKeys.AutoSaveRestorationFailedPromptTemplate);
                message = string.Format(format, detailedMessage);
                MessageBox.Show(message, App.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                project = new Project();
            }
            Project = Editor.Project = Project.Current = project;
            project.IsChanged = true;
            project.SaveFileName = null;
            Editor.Score = project.GetScore(project.Difficulty);
            format = Application.Current.FindResource<string>(App.ResourceKeys.LoadedProjectFromAutoSavPromptTemplate);
            message = string.Format(format, fileInfo.FullName);
            ShowTemporaryMessage(message);
        }

        private string GetBackupFileNameIfExists() {
            var path = App.GetDirectoryPath(App.DirectorPath.AutoBackup);
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            var directory = new DirectoryInfo(path);
            return directory.EnumerateFiles().FirstOrDefault()?.FullName;
        }

    }
}
