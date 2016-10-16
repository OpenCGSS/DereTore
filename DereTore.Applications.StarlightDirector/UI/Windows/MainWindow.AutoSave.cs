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
                    fileInfo.Delete();
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
            var project = ProjectIO.Load(fileInfo.FullName);
            Project = Editor.Project = Project.Current = project;
            project.IsChanged = true;
            project.SaveFileName = null;
            Editor.Score = project.GetScore(project.Difficulty);
            var format = Application.Current.FindResource<string>(App.ResourceKeys.LoadedProjectFromAutoSavPromptTemplate);
            var message = string.Format(format, fileInfo.FullName);
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
