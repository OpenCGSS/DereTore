using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using DereTore.Applications.ScoreEditor.Controls;
using DereTore.Applications.ScoreEditor.Model;

namespace DereTore.Applications.ScoreEditor.Forms {
    partial class FViewer {

        private void UnregisterEventHandlers() {
            timer.Elapsed -= Timer_Tick;
            btnPlay.Click -= BtnPlay_Click;
            btnStop.Click -= BtnStop_Click;
            btnPause.Click -= BtnPause_Click;
            btnSelectAcb.Click -= BtnSelectAcb_Click;
            btnSelectScore.Click -= BtnSelectScore_Click;
            btnScoreLoad.Click -= BtnScoreLoad_Click;
            btnScoreUnload.Click -= BtnScoreUnload_Click;
            editor.NoteEnteringOrExitingStage -= Editor_NoteUpdated;
            editor.SelectedNoteChanged -= Editor_SelectedNoteChanged;
            editor.MouseDoubleClick -= Editor_MouseDoubleClick;
            editor.MouseWheel -= Editor_MouseWheel;
            Load -= FMain_Load;
            FormClosing -= FViewer_FormClosing;
            progress.ValueChanged -= Progress_ValueChanged;
            progress.MouseDown -= Progress_MouseDown;
            progress.MouseUp -= Progress_MouseUp;
            progress.KeyDown -= Progress_KeyDown;
            progress.KeyUp -= Progress_KeyUp;
            tsbNoteCreate.Click -= TsbNoteCreate_Click;
            tsbNoteEdit.Click -= TsbNoteEdit_Click;
            tsbNoteRemove.Click -= TsbNoteRemove_Click;
            tsbScoreSave.Click -= TsbScoreSave_Click;
            tsbRetimingToNow.Click -= TsbRetimingToNow_Click;
            tsbMakeSync.Click -= TsbMakeSync_Click;
        }

        private void RegisterEventHandlers() {
            timer.Elapsed += Timer_Tick;
            btnPlay.Click += BtnPlay_Click;
            btnStop.Click += BtnStop_Click;
            btnPause.Click += BtnPause_Click;
            btnSelectAcb.Click += BtnSelectAcb_Click;
            btnSelectScore.Click += BtnSelectScore_Click;
            btnScoreLoad.Click += BtnScoreLoad_Click;
            btnScoreUnload.Click += BtnScoreUnload_Click;
            editor.NoteEnteringOrExitingStage += Editor_NoteUpdated;
            editor.SelectedNoteChanged += Editor_SelectedNoteChanged;
            editor.MouseDoubleClick += Editor_MouseDoubleClick;
            editor.MouseWheel += Editor_MouseWheel;
            Load += FMain_Load;
            FormClosing += FViewer_FormClosing;
            progress.ValueChanged += Progress_ValueChanged;
            progress.MouseDown += Progress_MouseDown;
            progress.MouseUp += Progress_MouseUp;
            progress.KeyDown += Progress_KeyDown;
            progress.KeyUp += Progress_KeyUp;
            tsbNoteCreate.Click += TsbNoteCreate_Click;
            tsbNoteEdit.Click += TsbNoteEdit_Click;
            tsbNoteRemove.Click += TsbNoteRemove_Click;
            tsbScoreSave.Click += TsbScoreSave_Click;
            tsbRetimingToNow.Click += TsbRetimingToNow_Click;
            tsbMakeSync.Click += TsbMakeSync_Click;
        }

        private void TsbMakeSync_Click(object sender, EventArgs e) {
            var selectedNote = editor.SelectedNote;
            if (selectedNote == null) {
                return;
            }
            Note anotherNote;
            var score = _score;
            if (selectedNote.IsSync) {
                this.ShowMessageBox("The selected note is already synced.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            var basis = FMakeSync.SelectNotes(score, selectedNote, out anotherNote);
            if (anotherNote == null) {
                return;
            }
            if (anotherNote.IsSync) {
                this.ShowMessageBox("The other note is already synced.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            score.MakeSync(selectedNote, anotherNote, basis);
        }

        private void TsbRetimingToNow_Click(object sender, EventArgs e) {
            var selectedNote = editor.SelectedNote;
            var player = _player;
            if (selectedNote != null && player != null) {
                var timing = player.CurrentTime.TotalSeconds;
                // Only used for confirmation.
                var temp = selectedNote.Clone();
                temp.HitTiming = timing;
                if (ConfirmNoteEdition(NoteEdition.ResetTiming, selectedNote, temp)) {
                    _score.ResetTimingTo(selectedNote, timing);
                }
            }
        }

        private void TsbScoreSave_Click(object sender, EventArgs e) {
            if (_score == null) {
                return;
            }
            var csvText = _score.SaveToCsv();
            Debug.Print(csvText);
        }

        private void TsbNoteRemove_Click(object sender, EventArgs e) {
            var selectedNote = editor.SelectedNote;
            if (selectedNote == null) {
                return;
            }
            if (ConfirmNoteEdition(NoteEdition.Remove, selectedNote, null)) {
                _score.RemoveNote(selectedNote);
            }
        }

        private void TsbNoteEdit_Click(object sender, EventArgs e) {
            var selectedNote = editor.SelectedNote;
            if (selectedNote == null) {
                return;
            }
            var newNote = FEditNote.ShowEditNote(this, selectedNote);
            if (newNote != null) {
                if (ConfirmNoteEdition(NoteEdition.Edit, selectedNote, newNote)) {
                    _score.EditNote(selectedNote, newNote);
                    propertyGrid.Refresh();
                }
            }
        }

        private void TsbNoteCreate_Click(object sender, EventArgs e) {
            var player = _player;
            if (player == null) {
                return;
            }
            var note = FEditNote.ShowCreateNote(this, player.CurrentTime.TotalSeconds);
            if (note != null) {
                note.InitializeAsTap();
                _score.AddNote(note);
            }
        }

        private void Editor_MouseWheel(object sender, MouseEventArgs e) {
            if (!progress.Enabled) {
                return;
            }
            var selectedNote = editor.SelectedNote;
            if (selectedNote == null) {
                int targetValue;
                if (e.Delta > 0) {
                    targetValue = progress.Value + progress.LargeChange;
                    if (targetValue > progress.Maximum) {
                        targetValue = progress.Maximum;
                    }
                    progress.Value = targetValue;
                } else if (e.Delta < 0) {
                    targetValue = progress.Value - progress.LargeChange;
                    if (targetValue < progress.Minimum) {
                        targetValue = progress.Minimum;
                    }
                    progress.Value = targetValue;
                }
            } else {
                var score = _score;
                var newNote = selectedNote.Clone();
                if (e.Delta > 0) {
                    newNote.HitTiming = selectedNote.GetAddTimingResult();
                    if (ConfirmNoteEdition(NoteEdition.Edit, selectedNote, newNote)) {
                        score.AddTiming(selectedNote);
                    }
                } else if (e.Delta < 0) {
                    newNote.HitTiming = selectedNote.GetSubtractTimingResult();
                    if (ConfirmNoteEdition(NoteEdition.Edit, selectedNote, newNote)) {
                        score.SubtractTiming(selectedNote);
                    }
                }
            }
        }

        private void Editor_MouseDoubleClick(object sender, MouseEventArgs e) {
            tsbNoteEdit.PerformClick();
        }

        private void Editor_SelectedNoteChanged(object sender, EventArgs e) {
            propertyGrid.SelectedObject = editor.SelectedNote;
            var anyNoteSelected = editor.SelectedNote != null;
            tsbNoteEdit.Enabled = anyNoteSelected;
            tsbNoteRemove.Enabled = anyNoteSelected;
            tsbMakeSync.Enabled = anyNoteSelected;
            tsbMakeFlick.Enabled = anyNoteSelected;
            tsbMakeHold.Enabled = anyNoteSelected;
            tsbResetToTap.Enabled = anyNoteSelected;
            tsbRetimingToNow.Enabled = anyNoteSelected;
        }

        private void Progress_KeyUp(object sender, KeyEventArgs e) {
            --_userSeekingStack;
            if (_userSeekingStack <= 0) {
                SoundManager.Instance.IsUserSeeking = false;
                if (btnPause.Enabled) {
                    _player?.Play();
                }
            }
        }

        private void Progress_KeyDown(object sender, KeyEventArgs e) {
            ++_userSeekingStack;
            SoundManager.Instance.IsUserSeeking = true;
            _player?.Pause();
        }

        private void Progress_MouseUp(object sender, MouseEventArgs e) {
            --_userSeekingStack;
            if (_userSeekingStack <= 0) {
                SoundManager.Instance.IsUserSeeking = false;
                if (btnPause.Enabled) {
                    _player?.Play();
                }
            }
        }

        private void Progress_MouseDown(object sender, MouseEventArgs e) {
            ++_userSeekingStack;
            SoundManager.Instance.IsUserSeeking = true;
            _player?.Pause();
        }

        private void Progress_ValueChanged(object sender, EventArgs e) {
            lock (_liveMusicSyncObject) {
                if (_codeValueChange) {
                    return;
                }
            }
            if (_player != null) {
                _player.CurrentTime = TimeSpan.FromSeconds(_player.TotalLength.TotalSeconds * ((double)(progress.Value - progress.Minimum) / progress.Maximum));
            }
        }

        private void FViewer_FormClosing(object sender, FormClosingEventArgs e) {
            if (btnScoreUnload.Enabled) {
                this.ShowMessageBox("Please unload the score and the ACB before exiting.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Cancel = true;
                return;
            }
            ClosePlayers();
            BtnScoreUnload_Click(this, EventArgs.Empty);
            UnregisterEventHandlers();
        }

        private void Editor_NoteUpdated(object sender, NoteEnteringOrExitingStageEventArgs e) {
            if (e.IsEntering) {
                return;
            }
            var note = e.Note;
            if (note.IsFlick) {
                SoundManager.Instance.PlayHca(FlickSoundFileName);
            } else if (note.IsTap || note.IsHold) {
                SoundManager.Instance.PlayHca(TapSoundFileName);
            }
        }

        private void FMain_Load(object sender, EventArgs e) {
            InitializeControls();
            PreloadNoteSounds();
            // Enable preview to see more realistic effects.
            editor.IsPreview = true;
        }

        private void BtnSelectScore_Click(object sender, EventArgs e) {
            openFileDialog.Filter = ScoreFilter;
            var result = openFileDialog.ShowDialog();
            if (result != DialogResult.Cancel && openFileDialog.FileName.Length > 0) {
                txtScoreFileName.Text = openFileDialog.FileName;
            }
        }

        private void BtnSelectAcb_Click(object sender, EventArgs e) {
            openFileDialog.Filter = AcbFilter;
            var result = openFileDialog.ShowDialog();
            if (result != DialogResult.Cancel && openFileDialog.FileName.Length > 0) {
                txtAcbFileName.Text = openFileDialog.FileName;
            }
        }

        private void BtnScoreUnload_Click(object sender, EventArgs e) {
            timer.Stop();
            BtnStop_Click(sender, e);
            if (_player != null) {
                _player.PlaybackStopped -= Player_PlaybackStopped;
                _player.Dispose();
                _player = null;
            }
            if (_acbStream != null) {
                _acbStream.Dispose();
                _acbStream = null;
            }
            _score = null;
            editor.Score = null;
            SetControlsEnabled(ViewerState.Initialized);
            lblSong.Text = string.Empty;
            lblTime.Text = string.Empty;
        }

        private void BtnScoreLoad_Click(object sender, EventArgs e) {
            if (!CheckPlayEnvironment()) {
                return;
            }
            _acbStream = File.Open(txtAcbFileName.Text, FileMode.Open, FileAccess.Read);
            _player = LiveMusicPlayer.FromStream(_acbStream, txtAcbFileName.Text, DecodeParams);
            _player.PlaybackStopped += Player_PlaybackStopped;
            Score score;
            var scoreFileName = txtScoreFileName.Text;
            var scoreFileExtension = new FileInfo(scoreFileName).Extension.ToLowerInvariant();
            if (scoreFileExtension == ExtensionBdb) {
                score = Score.FromBdbFile(scoreFileName, (Difficulty)(cboDifficulty.SelectedIndex + 1));
            } else if (scoreFileExtension == ExtensionCsv) {
                score = Score.FromCsvFile(scoreFileName);
            } else {
                throw new ArgumentException("What?", nameof(scoreFileExtension));
            }
            _score = score;
            editor.Score = _score;
            SetControlsEnabled(ViewerState.Loaded);
            lblSong.Text = string.Format(SongTipFormat, _player.HcaName);
            timer.Start();
        }

        private void Player_PlaybackStopped(object sender, NAudio.Wave.StoppedEventArgs e) {
            BtnStop_Click(this, EventArgs.Empty);
        }

        private void BtnStop_Click(object sender, EventArgs e) {
            if (_player == null) {
                return;
            }
            // Neccessary. Sometimes the stack just doesn't clear.
            _userSeekingStack = 0;
            _player.Stop();
            _player.CurrentTime = TimeSpan.Zero;
            SetControlsEnabled(ViewerState.Loaded);
            lock (_liveMusicSyncObject) {
                _codeValueChange = true;
                progress.Value = progress.Minimum;
                _codeValueChange = false;
            }
            if (!(editor.Disposing || editor.IsDisposed)) {
                editor.SetTime(TimeSpan.Zero);
                editor.Invalidate();
            }
            editor.MouseEventsEnabled = true;
        }

        private void BtnPause_Click(object sender, EventArgs e) {
            _player.Pause();
            SetControlsEnabled(ViewerState.LoadedAndPaused);
            editor.MouseEventsEnabled = true;
        }

        private void BtnPlay_Click(object sender, EventArgs e) {
            if (_player == null) {
                return;
            }
            if (_player.IsPaused) {
                SetControlsEnabled(ViewerState.LoadedAndPlaying);
            } else {
                SetControlsEnabled(ViewerState.LoadedAndPlaying);
            }
            editor.MouseEventsEnabled = false;
            _player.Play();
            lblTime.Text = $"{_player.CurrentTime}/{_player.TotalLength}";
        }

        private void Timer_Tick(object sender, EventArgs e) {
            var player = _player;
            if (player == null) {
                return;
            }
            var elapsed = player.CurrentTime;
            var total = player.TotalLength;
            if (elapsed >= total) {
                BtnStop_Click(this, EventArgs.Empty);
                return;
            }
            var ratio = elapsed.TotalSeconds / total.TotalSeconds;
            var val = (int)(ratio * (progress.Maximum - progress.Minimum)) + progress.Minimum;
            lock (_liveMusicSyncObject) {
                _codeValueChange = true;
                progress.Value = val;
                _codeValueChange = false;
            }
            lblTime.Text = $"{elapsed}/{player.TotalLength}";
            editor.SetTime(elapsed);
            editor.Invalidate();
            editor.JudgeNotesEnteringOrExiting(elapsed);
        }

    }
}
