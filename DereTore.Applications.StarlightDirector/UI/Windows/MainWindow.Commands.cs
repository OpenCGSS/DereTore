using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;
using DereTore.Applications.StarlightDirector.UI.Controls;
using Microsoft.Win32;

namespace DereTore.Applications.StarlightDirector.UI.Windows {
    partial class MainWindow {

        public static readonly ICommand CmdFileNewProject = RC("Ctrl+N");
        public static readonly ICommand CmdFileOpenProject = RC("Ctrl+O");
        public static readonly ICommand CmdFileSaveProject = RC("Ctrl+S");
        public static readonly ICommand CmdFileSaveProjectAs = RC("F12");
        public static readonly ICommand CmdFileExit = RC("Ctrl+Shift+Q");

        public static readonly ICommand CmdEditModeSelect = RC("Ctrl+1", "Ctrl+NumPad1");
        public static readonly ICommand CmdEditModeEditSync = RC("Ctrl+2", "Ctrl+NumPad2");
        public static readonly ICommand CmdEditModeEditFlick = RC("Ctrl+3", "Ctrl+NumPad3");
        public static readonly ICommand CmdEditModeEditHold = RC("Ctrl+4", "Ctrl+NumPad4");
        public static readonly ICommand CmdEditModeClear = RC("Ctrl+0", "Ctrl+NumPad0");
        public static readonly ICommand CmdEditNoteEdit = RC();
        public static readonly ICommand CmdEditNoteDelete = RC();

        public static readonly ICommand CmdEditBarAppend = RC("Ctrl+Alt+I");

        public static readonly ICommand CmdPreviewStart = RC("F5");

        public static readonly ICommand CmdScoreSwitchDifficulty = RC();

        public static readonly ICommand CmdToolsExportScoreToCsv = RC();

        private void InitializeCommandBindings() {
            var cb = CommandBindings;

            var thisType = GetType();
            var icommandType = typeof(ICommand);
            var commandFields = thisType.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var commandField in commandFields) {
                if (commandField.FieldType != icommandType && !commandField.FieldType.IsSubclassOf(icommandType)) {
                    continue;
                }
                var command = (ICommand)commandField.GetValue(null);
                var name = commandField.Name;
                var executedHandlerInfo = thisType.GetMethod(name + "_Executed", BindingFlags.NonPublic | BindingFlags.Instance);
                var executedHandler = (ExecutedRoutedEventHandler)Delegate.CreateDelegate(typeof(ExecutedRoutedEventHandler), this, executedHandlerInfo);
                var canExecuteHandlerInfo = thisType.GetMethod(name + "_CanExecute", BindingFlags.NonPublic | BindingFlags.Instance);
                var canExecuteHandler = (CanExecuteRoutedEventHandler)Delegate.CreateDelegate(typeof(CanExecuteRoutedEventHandler), this, canExecuteHandlerInfo);
                cb.Add(new CommandBinding(command, executedHandler, canExecuteHandler));
            }
        }

        private void CmdFileNewProject_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void CmdFileNewProject_Executed(object sender, ExecutedRoutedEventArgs e) {
            if (Project != null) {
                if (Project.IsChanged && !Project.IsSaved) {
                    var result = MessageBox.Show("The project is changed. Do you want to save it first?", Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);
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
            }
            var project = new Project();
            Project.Current = project;
            Project = project;
        }

        private void CmdFileOpenProject_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void CmdFileOpenProject_Executed(object sender, ExecutedRoutedEventArgs e) {
            if (Project != null) {
                if (Project.IsChanged && !Project.IsSaved) {
                    var result = MessageBox.Show(Application.Current.FindResource<string>(App.ResourceKeys.ExitPrompt), Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);
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
            }
            var openDialog = new OpenFileDialog();
            openDialog.CheckPathExists = true;
            openDialog.Multiselect = false;
            openDialog.ShowReadOnly = false;
            openDialog.ReadOnlyChecked = false;
            openDialog.ValidateNames = true;
            openDialog.Filter = Application.Current.FindResource<string>(App.ResourceKeys.ProjectFileFilter);
            var dialogResult = openDialog.ShowDialog();
            if (dialogResult ?? false) {
                var project = Project.Load(openDialog.FileName);
                Project = Editor.Project = Project.Current = project;
                Editor.Score = project?.GetScore(project.Difficulty);
            }
        }

        private void CmdFileSaveProject_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Project?.IsChanged ?? false;
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
                project.Save();
                CmdFileSaveProject.RaiseCanExecuteChanged();
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
            if (result ?? false) {
                Project.Save(saveDialog.FileName);
                CmdFileSaveProjectAs.RaiseCanExecuteChanged();
            }
        }

        private void CmdFileExit_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void CmdFileExit_Executed(object sender, ExecutedRoutedEventArgs e) {
            Close();
        }

        private void CmdEditModeSelect_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.EditMode != EditMode.Select;
        }

        private void CmdEditModeSelect_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.Print("Edit mode: select");
            Editor.EditMode = EditMode.Select;
        }

        private void CmdEditModeEditSync_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.EditMode != EditMode.Sync;
            //var editMode = Editor.EditMode;
            //switch (editMode) {
            //    case EditMode.Select:
            //        var selectedNotes = Editor.GetSelectedScoreNotes();
            //        var count = selectedNotes.Count();
            //        e.CanExecute = count == 1 || count == 2;
            //        break;
            //    case EditMode.EditHold:
            //    case EditMode.EditFlick:
            //    case EditMode.EditSync:
            //        e.CanExecute = false;
            //        break;
            //    default:
            //        throw new ArgumentOutOfRangeException(nameof(editMode));
            //}
        }

        private void CmdEditModeEditSync_Executed(object sender, ExecutedRoutedEventArgs e) {
            //Debug.Assert(Editor.EditMode == EditMode.Select || Editor.EditMode == EditMode.EditSync, "Editor.EditMode == EditMode.Select || Editor.EditMode == EditMode.EditSync");
            if (Editor.EditMode == EditMode.Sync) {
                Editor.EditMode = EditMode.Select;
                return;
            }
            Editor.EditMode = EditMode.Sync;
            Debug.Print("Edit mode: sync");
        }

        private void CmdEditModeEditFlick_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.EditMode != EditMode.Flick;
            //var editMode = Editor.EditMode;
            //switch (editMode) {
            //    case EditMode.Select:
            //        var selectedNotes = Editor.GetSelectedScoreNotes();
            //        e.CanExecute = selectedNotes.Any();
            //        break;
            //    case EditMode.EditHold:
            //    case EditMode.EditFlick:
            //    case EditMode.EditSync:
            //        e.CanExecute = false;
            //        break;
            //    default:
            //        throw new ArgumentOutOfRangeException(nameof(editMode));
            //}
        }

        private void CmdEditModeEditFlick_Executed(object sender, ExecutedRoutedEventArgs e) {
            //Debug.Assert(Editor.EditMode == EditMode.Select || Editor.EditMode == EditMode.EditFlick, "Editor.EditMode == EditMode.Select || Editor.EditMode == EditMode.EditFlick");
            if (Editor.EditMode == EditMode.Flick) {
                Editor.EditMode = EditMode.Select;
                return;
            }
            Editor.EditMode = EditMode.Flick;
            Debug.Print("Edit mode: flick");
        }

        private void CmdEditModeEditHold_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.EditMode != EditMode.Hold;
            //var editMode = Editor.EditMode;
            //switch (editMode) {
            //    case EditMode.Select:
            //        var selectedNotes = Editor.GetSelectedScoreNotes();
            //        e.CanExecute = selectedNotes.Count() == 1;
            //        break;
            //    case EditMode.EditHold:
            //    case EditMode.EditFlick:
            //    case EditMode.EditSync:
            //        e.CanExecute = false;
            //        break;
            //    default:
            //        throw new ArgumentOutOfRangeException(nameof(editMode));
            //}
        }

        private void CmdEditModeEditHold_Executed(object sender, ExecutedRoutedEventArgs e) {
            //Debug.Assert(Editor.EditMode == EditMode.Select || Editor.EditMode == EditMode.EditHold, "Editor.EditMode == EditMode.Select || Editor.EditMode == EditMode.EditHold");
            if (Editor.EditMode == EditMode.Hold) {
                Editor.EditMode = EditMode.Select;
                return;
            }
            Editor.EditMode = EditMode.Hold;
            Debug.Print("Edit mode: hold");
        }

        private void CmdEditModeClear_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.EditMode != EditMode.Clear;
        }

        private void CmdEditModeClear_Executed(object sender, ExecutedRoutedEventArgs e) {
            if (Editor.EditMode == EditMode.Clear) {
                Editor.EditMode = EditMode.Select;
                return;
            }
            Editor.EditMode = EditMode.Clear;
            Debug.Print("Edit mode: clear");
        }

        private void CmdEditNoteEdit_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSingleSelectedScoreNote;
        }

        private void CmdEditNoteEdit_Executed(object sender, ExecutedRoutedEventArgs e) {

        }

        private void CmdEditNoteDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSelectedScoreNotes;
        }

        private void CmdEditNoteDelete_Executed(object sender, ExecutedRoutedEventArgs e) {

        }

        private void CmdPreviewStart_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdPreviewStart_Executed(object sender, ExecutedRoutedEventArgs e) {
            MessageBox.Show(Application.Current.FindResource<string>(App.ResourceKeys.PreviewNotImplemented), Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void CmdScoreSwitchDifficulty_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            if (Project == null) {
                e.CanExecute = false;
            } else {
                e.CanExecute = Project.Difficulty != (Difficulty)e.Parameter;
            }
        }

        private void CmdScoreSwitchDifficulty_Executed(object sender, ExecutedRoutedEventArgs e) {
            if (Project != null) {
                Project.Difficulty = (Difficulty)e.Parameter;
            }
        }

        private void CmdToolsExportScoreToCsv_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void CmdToolsExportScoreToCsv_Executed(object sender, ExecutedRoutedEventArgs e) {
            //var difficulty = (Difficulty)(DifficultySelector.SelectedIndex + 1);
            var saveDialog = new SaveFileDialog();
            saveDialog.OverwritePrompt = true;
            saveDialog.ValidateNames = true;
            saveDialog.Filter = Application.Current.FindResource<string>(App.ResourceKeys.CsvFileFilter);
            var result = saveDialog.ShowDialog();
            if (result ?? false) {
                //Project.Current.SaveScoreToCsv(difficulty, saveDialog.FileName);
            }
        }

        private void CmdEditBarAppend_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Project != null;
        }

        private void CmdEditBarAppend_Executed(object sender, ExecutedRoutedEventArgs e) {
            Editor.AppendBar();
        }

    }
}
