using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;
using DereTore.Applications.StarlightDirector.UI.Controls;
using Microsoft.Win32;

namespace DereTore.Applications.StarlightDirector.UI.Windows {
    partial class MainWindow {

        private void InitializeCommandBindings() {
            var cb = CommandBindings;
            cb.Add(new CommandBinding(CmdEngageSyncMode, CmdEngageSyncMode_Executed, CmdEngageSyncMode_CanExecute));
            cb.Add(new CommandBinding(CmdEngageFlickMode, CmdEngageFlickMode_Executed, CmdEngageFlickMode_CanExecute));
            cb.Add(new CommandBinding(CmdEngageHoldMode, CmdEngageHoldMode_Executed, CmdEngageHoldMode_CanExecute));
            cb.Add(new CommandBinding(CmdFileSaveProject, CmdFileSaveProject_Executed, CmdFileSaveProject_CanExecute));
            cb.Add(new CommandBinding(CmdFileSaveProjectAs, CmdFileSaveProjectAs_Executed, CmdFileSaveProjectAs_CanExecute));
            var ib = InputBindings;
            ib.Add(new KeyBinding(CmdFileSaveProject, Key.S, ModifierKeys.Control));
        }

        private void CmdEngageSyncMode_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.Assert(Editor.EditMode == EditMode.Normal || Editor.EditMode == EditMode.EditSync, "Editor.EditMode == EditMode.Normal || Editor.EditMode == EditMode.EditSync");
            if (Editor.EditMode == EditMode.EditSync) {
                Editor.EditMode = EditMode.Normal;
                return;
            }
            Editor.EditMode = EditMode.EditSync;
        }

        private void CmdEngageSyncMode_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            var editMode = Editor.EditMode;
            switch (editMode) {
                case EditMode.Normal:
                    var selectedNotes = Editor.GetSelectedScoreNotes();
                    var count = selectedNotes.Count();
                    e.CanExecute = count == 1 || count == 2;
                    break;
                case EditMode.EditHold:
                case EditMode.EditFlick:
                    e.CanExecute = false;
                    break;
                case EditMode.EditSync:
                    e.CanExecute = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(editMode));
            }
        }

        private void CmdEngageFlickMode_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.Assert(Editor.EditMode == EditMode.Normal || Editor.EditMode == EditMode.EditFlick, "Editor.EditMode == EditMode.Normal || Editor.EditMode == EditMode.EditFlick");
            if (Editor.EditMode == EditMode.EditFlick) {
                Editor.EditMode = EditMode.Normal;
                return;
            }
            Editor.EditMode = EditMode.EditFlick;
        }

        private void CmdEngageFlickMode_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            var editMode = Editor.EditMode;
            switch (editMode) {
                case EditMode.Normal:
                    var selectedNotes = Editor.GetSelectedScoreNotes();
                    e.CanExecute = selectedNotes.Any();
                    break;
                case EditMode.EditSync:
                case EditMode.EditHold:
                    e.CanExecute = false;
                    break;
                case EditMode.EditFlick:
                    e.CanExecute = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(editMode));
            }
        }

        private void CmdEngageHoldMode_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.Assert(Editor.EditMode == EditMode.Normal || Editor.EditMode == EditMode.EditHold, "Editor.EditMode == EditMode.Normal || Editor.EditMode == EditMode.EditHold");
            if (Editor.EditMode == EditMode.EditHold) {
                Editor.EditMode = EditMode.Normal;
                return;
            }
            Editor.EditMode = EditMode.EditHold;
        }

        private void CmdEngageHoldMode_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            var editMode = Editor.EditMode;
            switch (editMode) {
                case EditMode.Normal:
                    var selectedNotes = Editor.GetSelectedScoreNotes();
                    e.CanExecute = selectedNotes.Count() == 1;
                    break;
                case EditMode.EditSync:
                case EditMode.EditFlick:
                    e.CanExecute = false;
                    break;
                case EditMode.EditHold:
                    e.CanExecute = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(editMode));
            }
        }

        private void CmdFileSaveProject_Executed(object sender, ExecutedRoutedEventArgs e) {
            var project = Project.CurrentProject;
            if (!project.IsSaved) {
                CmdFileSaveProjectAs.Execute(e.Parameter);
            } else {
                project.Save();
                CmdFileSaveProject.RaiseCanExecuteChanged();
            }
        }

        private void CmdFileSaveProject_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Project.CurrentProject?.IsChanged ?? false;
        }

        private void CmdFileSaveProjectAs_Executed(object sender, ExecutedRoutedEventArgs e) {
            var saveDialog = new SaveFileDialog();
            saveDialog.OverwritePrompt = true;
            saveDialog.ValidateNames = true;
            saveDialog.Filter = (string)Application.Current.FindResource(App.ResourceKeys.ProjectFileFilter);
            var result = saveDialog.ShowDialog();
            if (result.HasValue && result.Value) {
                Project.CurrentProject.Save(saveDialog.FileName);
                CmdFileSaveProjectAs.RaiseCanExecuteChanged();
            }
        }

        private void CmdFileSaveProjectAs_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        public static readonly ICommand CmdEngageSyncMode = new RoutedCommand();
        public static readonly ICommand CmdEngageFlickMode = new RoutedCommand();
        public static readonly ICommand CmdEngageHoldMode = new RoutedCommand();

        public static readonly ICommand CmdFileSaveProject = new RoutedCommand();
        public static readonly ICommand CmdFileSaveProjectAs = new RoutedCommand();

    }
}
