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

        public static readonly ICommand CmdFileNewProject = CommandHelper.RegisterCommand("Ctrl+N");
        public static readonly ICommand CmdFileOpenProject = CommandHelper.RegisterCommand("Ctrl+O");
        public static readonly ICommand CmdFileSaveProject = CommandHelper.RegisterCommand("Ctrl+S");
        public static readonly ICommand CmdFileSaveProjectAs = CommandHelper.RegisterCommand("F12");
        public static readonly ICommand CmdFilePreferences = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdFileExit = CommandHelper.RegisterCommand("Ctrl+W", "Ctrl+Shift+Q");

        public static readonly ICommand CmdEditModeSelect = CommandHelper.RegisterCommand("Alt+1", "Alt+NumPad1");
        public static readonly ICommand CmdEditModeEditSync = CommandHelper.RegisterCommand("Alt+2", "Alt+NumPad2");
        public static readonly ICommand CmdEditModeEditFlick = CommandHelper.RegisterCommand("Alt+3", "Alt+NumPad3");
        public static readonly ICommand CmdEditModeEditHold = CommandHelper.RegisterCommand("Alt+4", "Alt+NumPad4");
        public static readonly ICommand CmdEditModeClear = CommandHelper.RegisterCommand("Alt+0", "Alt+NumPad0");
        public static readonly ICommand CmdEditSelectAll = CommandHelper.RegisterCommand("Ctrl+A");
        public static readonly ICommand CmdEditNoteAdd = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdEditNoteEdit = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdEditNoteDelete = CommandHelper.RegisterCommand("Delete");
        public static readonly ICommand CmdEditBarAppend = CommandHelper.RegisterCommand("Ctrl+Alt+U");
        public static readonly ICommand CmdEditBarAppendMany = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdEditBarInsert = CommandHelper.RegisterCommand("Ctrl+Alt+I");
        public static readonly ICommand CmdEditBarEdit = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdEditBarDelete = CommandHelper.RegisterCommand("Ctrl+Delete");

        public static readonly ICommand CmdPreviewStart = CommandHelper.RegisterCommand("F5");

        public static readonly ICommand CmdMusicSelectWaveFile = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdMusicPlay = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdMusicStop = CommandHelper.RegisterCommand();

        public static readonly ICommand CmdScoreSwitchDifficulty = CommandHelper.RegisterCommand();

        public static readonly ICommand CmdToolsBuildMusicArchive = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdToolsBuildScoreDatabase = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdToolsImportMusicArchive = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdToolsImportScoreDatabase = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdToolsExportScoreToCsv = CommandHelper.RegisterCommand();
        
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
            if (dialogResult ?? false) {
                var project = Project.Load(openDialog.FileName);
                Project = Editor.Project = Project.Current = project;
                // Caution! The property is set to true on deserialization.
                project.IsChanged = false;
                Editor.Score = project.GetScore(project.Difficulty);
            }
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
        }

        private void CmdEditModeEditSync_Executed(object sender, ExecutedRoutedEventArgs e) {
            if (Editor.EditMode == EditMode.Sync) {
                Editor.EditMode = EditMode.Select;
                return;
            }
            Editor.EditMode = EditMode.Sync;
            Debug.Print("Edit mode: sync");
        }

        private void CmdEditModeEditFlick_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.EditMode != EditMode.Flick;
        }

        private void CmdEditModeEditFlick_Executed(object sender, ExecutedRoutedEventArgs e) {
            if (Editor.EditMode == EditMode.Flick) {
                Editor.EditMode = EditMode.Select;
                return;
            }
            Editor.EditMode = EditMode.Flick;
            Debug.Print("Edit mode: flick");
        }

        private void CmdEditModeEditHold_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.EditMode != EditMode.Hold;
        }

        private void CmdEditModeEditHold_Executed(object sender, ExecutedRoutedEventArgs e) {
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

        private void CmdEditSelectAll_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.ScoreNotes.Count > 0;
        }

        private void CmdEditSelectAll_Executed(object sender, ExecutedRoutedEventArgs e) {
            Editor.SelectAllScoreNotes();
        }

        private void CmdEditNoteAdd_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSingleSelectedScoreBar;
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
            var scoreNotes = Editor.GetSelectedScoreNotes();
            Editor.RemoveScoreNotes(scoreNotes);
            NotifyProjectChanged();
        }

        private void CmdEditBarAppend_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdEditBarAppend_Executed(object sender, ExecutedRoutedEventArgs e) {
            Editor.AppendScoreBar();
            NotifyProjectChanged();
        }

        private void CmdEditBarAppendMany_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdEditBarAppendMany_Executed(object sender, ExecutedRoutedEventArgs e) {
            var count = (int)(double)e.Parameter;
            Editor.AppendScoreBars(count);
            NotifyProjectChanged();
        }

        private void CmdEditBarInsert_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSingleSelectedScoreBar;
        }

        private void CmdEditBarInsert_Executed(object sender, ExecutedRoutedEventArgs e) {
            var scoreBar = Editor.GetSelectedScoreBar();
            if (scoreBar != null) {
                var newScoreBar = Editor.InsertScoreBar(scoreBar);
                Editor.SelectScoreBar(newScoreBar);
                Editor.ScrollToScoreBar(newScoreBar);
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
                var result = MessageBox.Show(Application.Current.FindResource<string>(App.ResourceKeys.ConfirmDeleteBar), App.Title, MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                switch (result) {
                    case MessageBoxResult.Yes:
                        Editor.RemoveScoreBar(scoreBar);
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
            MessageBox.Show(Application.Current.FindResource<string>(App.ResourceKeys.PreviewNotImplemented), App.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
                Project.SaveScoreToCsv(Project.Difficulty, saveDialog.FileName);
            }
        }

    }
}
