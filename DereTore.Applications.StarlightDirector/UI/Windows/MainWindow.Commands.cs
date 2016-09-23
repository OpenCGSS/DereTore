using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using DereTore.Applications.StarlightDirector.Components;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;
using DereTore.Applications.StarlightDirector.UI.Controls;
using Microsoft.Win32;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using AudioOut = NAudio.Wave.WasapiOut;

namespace DereTore.Applications.StarlightDirector.UI.Windows {
    partial class MainWindow {

        public static readonly ICommand CmdFileNewProject = CommandHelper.RC("Ctrl+N");
        public static readonly ICommand CmdFileOpenProject = CommandHelper.RC("Ctrl+O");
        public static readonly ICommand CmdFileSaveProject = CommandHelper.RC("Ctrl+S");
        public static readonly ICommand CmdFileSaveProjectAs = CommandHelper.RC("F12");
        public static readonly ICommand CmdFilePreferences = CommandHelper.RC();
        public static readonly ICommand CmdFileExit = CommandHelper.RC("Ctrl+W", "Ctrl+Shift+Q");

        public static readonly ICommand CmdEditModeSelect = CommandHelper.RC("Ctrl+1", "Ctrl+NumPad1");
        public static readonly ICommand CmdEditModeEditSync = CommandHelper.RC("Ctrl+2", "Ctrl+NumPad2");
        public static readonly ICommand CmdEditModeEditFlick = CommandHelper.RC("Ctrl+3", "Ctrl+NumPad3");
        public static readonly ICommand CmdEditModeEditHold = CommandHelper.RC("Ctrl+4", "Ctrl+NumPad4");
        public static readonly ICommand CmdEditModeClear = CommandHelper.RC("Ctrl+0", "Ctrl+NumPad0");
        public static readonly ICommand CmdEditNoteAdd = CommandHelper.RC();
        public static readonly ICommand CmdEditNoteEdit = CommandHelper.RC();
        public static readonly ICommand CmdEditNoteDelete = CommandHelper.RC();
        public static readonly ICommand CmdEditBarAppend = CommandHelper.RC("Ctrl+Alt+U");
        public static readonly ICommand CmdEditBarAppendMany = CommandHelper.RC();
        public static readonly ICommand CmdEditBarInsert = CommandHelper.RC();
        public static readonly ICommand CmdEditBarEdit = CommandHelper.RC();
        public static readonly ICommand CmdEditBarDelete = CommandHelper.RC();

        public static readonly ICommand CmdPreviewStart = CommandHelper.RC("F5");

        public static readonly ICommand CmdMusicSelectWaveFile = CommandHelper.RC();
        public static readonly ICommand CmdMusicPlay = CommandHelper.RC();
        public static readonly ICommand CmdMusicStop = CommandHelper.RC();

        public static readonly ICommand CmdScoreSwitchDifficulty = CommandHelper.RC();

        public static readonly ICommand CmdToolsBuildMusicArchive = CommandHelper.RC();
        public static readonly ICommand CmdToolsBuildScoreDatabase = CommandHelper.RC();
        public static readonly ICommand CmdToolsImportMusicArchive = CommandHelper.RC();
        public static readonly ICommand CmdToolsImportScoreDatabase = CommandHelper.RC();
        public static readonly ICommand CmdToolsExportScoreToCsv = CommandHelper.RC();

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
                // Caution! The property is set to true on deserialization.
                project.IsChanged = false;
                Editor.Score = project.GetScore(project.Difficulty);
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

        private void CmdFilePreferences_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = false;
        }

        private void CmdFilePreferences_Executed(object sender, ExecutedRoutedEventArgs e) {
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

        private void CmdEditNoteAdd_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = false;
        }

        private void CmdEditNoteAdd_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.Print("Not implemented: add note");
            NotifyProjectChanged();
        }

        private void CmdEditNoteEdit_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSingleSelectedScoreNote;
        }

        private void CmdEditNoteEdit_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.Print("Not implemented: edit note");
            NotifyProjectChanged();
        }

        private void CmdEditNoteDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSelectedScoreNotes;
        }

        private void CmdEditNoteDelete_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.Print("Not implemented: delete note");
            NotifyProjectChanged();
        }

        private void CmdEditBarAppend_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdEditBarAppend_Executed(object sender, ExecutedRoutedEventArgs e) {
            Editor.AppendBar();
            NotifyProjectChanged();
        }

        private void CmdEditBarAppendMany_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdEditBarAppendMany_Executed(object sender, ExecutedRoutedEventArgs e) {
            var count = (int)(double)e.Parameter;
            for (var i = 0; i < count; ++i) {
                Editor.AppendBar();
            }
            NotifyProjectChanged();
        }

        private void CmdEditBarInsert_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSingleSelectedScoreBar;
        }

        private void CmdEditBarInsert_Executed(object sender, ExecutedRoutedEventArgs e) {
            var scoreBar = Editor.GetSelectedScoreBar();
            if (scoreBar != null) {
                Editor.InsertBar(scoreBar);
                NotifyProjectChanged();
            }
        }

        private void CmdEditBarEdit_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSingleSelectedScoreBar && false;
        }

        private void CmdEditBarEdit_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.Print("Not implemented: edit bar");
            NotifyProjectChanged();
        }

        private void CmdEditBarDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSingleSelectedScoreBar;
        }

        private void CmdEditBarDelete_Executed(object sender, ExecutedRoutedEventArgs e) {
            var scoreBar = Editor.GetSelectedScoreBar();
            if (scoreBar != null) {
                var result = MessageBox.Show(Application.Current.FindResource<string>(App.ResourceKeys.ConfirmDeleteBar), Title, MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                switch (result) {
                    case MessageBoxResult.Yes:
                        Editor.RemoveBar(scoreBar);
                        NotifyProjectChanged();
                        break;
                    case MessageBoxResult.No:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(result));
                }
            }
        }

        private void CmdPreviewStart_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdPreviewStart_Executed(object sender, ExecutedRoutedEventArgs e) {
            MessageBox.Show(Application.Current.FindResource<string>(App.ResourceKeys.PreviewNotImplemented), Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void CmdMusicSelectWaveFile_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Project != null;
        }

        private void CmdMusicSelectWaveFile_Executed(object sender, ExecutedRoutedEventArgs e) {
            var openDialog = new OpenFileDialog();
            openDialog.CheckPathExists = true;
            openDialog.Multiselect = false;
            openDialog.ShowReadOnly = false;
            openDialog.ReadOnlyChecked = false;
            openDialog.ValidateNames = true;
            openDialog.Filter = Application.Current.FindResource<string>(App.ResourceKeys.WaveFileFilter);
            var dialogResult = openDialog.ShowDialog();
            if (dialogResult ?? false) {
                Editor.Project.MusicFileName = openDialog.FileName;
                CmdMusicSelectWaveFile.RaiseCanExecuteChanged();
                NotifyProjectChanged();
            }
        }

        private void CmdMusicPlay_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            var r = Editor.Project?.HasMusic ?? false;
            if (r) {
                if (_selectedWaveOut != null) {
                    r = _selectedWaveOut.PlaybackState != PlaybackState.Playing;
                } else {
                    r = Project.HasMusic;
                }
            }
            e.CanExecute = r;
        }

        private void CmdMusicPlay_Executed(object sender, ExecutedRoutedEventArgs e) {
            var o = _selectedWaveOut;
            if (o == null) {
                o = new AudioOut(AudioClientShareMode.Shared, 0);
                var fileStream = new FileStream(Project.MusicFileName, FileMode.Open, FileAccess.Read);
                _waveReader = new WaveFileReader(fileStream);
                o.PlaybackStopped += SelectedWaveOut_PlaybackStopped;
                o.Init(_waveReader);
                _selectedWaveOut = o;
            }
            if (o.PlaybackState == PlaybackState.Playing) {
                _waveReader.Position = 0;
            } else {
                o.Play();
            }
            CmdMusicPlay.RaiseCanExecuteChanged();
        }

        private void CmdMusicStop_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            var r = Editor.Project?.HasMusic ?? false;
            if (r) {
                r = _selectedWaveOut != null && _selectedWaveOut.PlaybackState == PlaybackState.Playing;
            }
            e.CanExecute = r;
        }

        private void CmdMusicStop_Executed(object sender, ExecutedRoutedEventArgs e) {
            var o = _selectedWaveOut;
            o?.Stop();
            SelectedWaveOut_PlaybackStopped(o, EventArgs.Empty);
            CmdMusicStop.RaiseCanExecuteChanged();
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
                //Project.Current.SaveScoreToCsv(difficulty, saveDialog.FileName);
            }
        }

    }
}
