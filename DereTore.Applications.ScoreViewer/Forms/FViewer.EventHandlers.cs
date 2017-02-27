using System;
using System.IO;
using System.Windows.Forms;
using DereTore.Applications.ScoreViewer.Controls;
using DereTore.Applications.ScoreViewer.Model;

namespace DereTore.Applications.ScoreViewer.Forms {
    partial class FViewer {

        private void UnregisterEventHandlers() {
            timer.Elapsed -= Timer_Tick;
            btnPlay.Click -= BtnPlay_Click;
            btnStop.Click -= BtnStop_Click;
            btnPause.Click -= BtnPause_Click;
            btnSelectAudio.Click -= BtnSelectAudio_Click;
            btnSelectScore.Click -= BtnSelectScore_Click;
            btnScoreLoad.Click -= BtnScoreLoad_Click;
            btnScoreUnload.Click -= BtnScoreUnload_Click;
            editor.NoteEnteringOrExitingStage -= Editor_NoteUpdated;
            editor.MouseWheel -= Editor_MouseWheel;
            Load -= FMain_Load;
            FormClosing -= FViewer_FormClosing;
            trkProgress.ValueChanged -= TrkProgress_ValueChanged;
            trkProgress.MouseDown -= TrkProgress_MouseDown;
            trkProgress.MouseUp -= TrkProgress_MouseUp;
            trkProgress.KeyDown -= TrkProgress_KeyDown;
            trkProgress.KeyUp -= TrkProgress_KeyUp;
            trkFallingSpeed.ValueChanged -= TrkFallingSpeed_ValueChanged;
            trkMusicVolume.ValueChanged -= TrkMusicVolume_ValueChanged;
            trkSfxVolume.ValueChanged -= TrkSfxVolume_ValueChanged;
        }

        private void RegisterEventHandlers() {
            timer.Elapsed += Timer_Tick;
            btnPlay.Click += BtnPlay_Click;
            btnStop.Click += BtnStop_Click;
            btnPause.Click += BtnPause_Click;
            btnSelectAudio.Click += BtnSelectAudio_Click;
            btnSelectScore.Click += BtnSelectScore_Click;
            btnScoreLoad.Click += BtnScoreLoad_Click;
            btnScoreUnload.Click += BtnScoreUnload_Click;
            editor.NoteEnteringOrExitingStage += Editor_NoteUpdated;
            editor.MouseWheel += Editor_MouseWheel;
            Load += FMain_Load;
            FormClosing += FViewer_FormClosing;
            trkProgress.ValueChanged += TrkProgress_ValueChanged;
            trkProgress.MouseDown += TrkProgress_MouseDown;
            trkProgress.MouseUp += TrkProgress_MouseUp;
            trkProgress.KeyDown += TrkProgress_KeyDown;
            trkProgress.KeyUp += TrkProgress_KeyUp;
            trkFallingSpeed.ValueChanged += TrkFallingSpeed_ValueChanged;
            trkMusicVolume.ValueChanged += TrkMusicVolume_ValueChanged;
            trkSfxVolume.ValueChanged += TrkSfxVolume_ValueChanged;
        }

        private void TrkSfxVolume_ValueChanged(object sender, EventArgs e) {
            PlayerSettings.SfxVolume = (float)(trkSfxVolume.Value - trkSfxVolume.Minimum) / (trkSfxVolume.Maximum - trkSfxVolume.Minimum);
        }

        private void TrkMusicVolume_ValueChanged(object sender, EventArgs e) {
            PlayerSettings.MusicVolume = (float)(trkMusicVolume.Value - trkMusicVolume.Minimum) / (trkMusicVolume.Maximum - trkMusicVolume.Minimum);
        }

        private void TrkFallingSpeed_ValueChanged(object sender, EventArgs e) {
            const float a = 2.0f;
            var value = (float)Math.Sqrt((float)trkFallingSpeed.Value / 2);
            value = a / value;
            RenderHelper.FutureTimeWindow = value;
        }

        private void Editor_MouseWheel(object sender, MouseEventArgs e) {
            if (!trkProgress.Enabled) {
                return;
            }
            int targetValue;
            if (e.Delta > 0) {
                targetValue = trkProgress.Value + trkProgress.LargeChange;
                if (targetValue > trkProgress.Maximum) {
                    targetValue = trkProgress.Maximum;
                }
                trkProgress.Value = targetValue;
            } else if (e.Delta < 0) {
                targetValue = trkProgress.Value - trkProgress.LargeChange;
                if (targetValue < trkProgress.Minimum) {
                    targetValue = trkProgress.Minimum;
                }
                trkProgress.Value = targetValue;
            }
        }

        private void TrkProgress_KeyUp(object sender, KeyEventArgs e) {
            --_userSeekingStack;
            if (_userSeekingStack <= 0) {
                if (btnPause.Enabled) {
                    _scorePlayer?.Play();
                }
            }
        }

        private void TrkProgress_KeyDown(object sender, KeyEventArgs e) {
            ++_userSeekingStack;
            _scorePlayer?.Pause();
        }

        private void TrkProgress_MouseUp(object sender, MouseEventArgs e) {
            --_userSeekingStack;
            if (_userSeekingStack <= 0) {
                if (btnPause.Enabled) {
                    _scorePlayer?.Play();
                }
            }
        }

        private void TrkProgress_MouseDown(object sender, MouseEventArgs e) {
            ++_userSeekingStack;
            _scorePlayer?.Pause();
        }

        private void TrkProgress_ValueChanged(object sender, EventArgs e) {
            lock (_liveMusicSyncObject) {
                if (_codeValueChange) {
                    return;
                }
            }
            if (_scorePlayer != null) {
                _scorePlayer.CurrentTime = TimeSpan.FromSeconds(_musicWaveStream.TotalTime.TotalSeconds * ((double)(trkProgress.Value - trkProgress.Minimum) / trkProgress.Maximum));
            }
        }

        private void FViewer_FormClosing(object sender, FormClosingEventArgs e) {
            if (btnScoreUnload.Enabled) {
                this.ShowMessageBox("Please unload the score and the audio file before exiting.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Cancel = true;
                return;
            }
            ClosePlayers();
            BtnScoreUnload_Click(this, EventArgs.Empty);
            UnregisterEventHandlers();
        }

        private void Editor_NoteUpdated(object sender, NoteEnteringOrExitingStageEventArgs e) {
        }

        public void UpdateSfx(TimeSpan rawMusicTime) {
            lock (_sfxSyncObject) {
                if (_sfxManager == null) {
                    return;
                }
                var now = (rawMusicTime + _sfxManager.BufferOffset).TotalSeconds;
                if (now <= _sfxBufferTime) {
                    return;
                }
                var prev = _sfxBufferTime;
                if (chkSfxOn.Checked) {
                    foreach (var note in _score.Notes) {
                        if (!(note.HitTiming < prev) && (note.HitTiming < now)) {
                            if (note.IsFlick) {
                                _sfxManager.PlayWave(_currentFlickHcaFileName, TimeSpan.FromSeconds(note.HitTiming), PlayerSettings.SfxVolume);
                            } else if (note.IsTap || note.IsHold || note.IsSlide) {
                                _sfxManager.PlayWave(_currentTapHcaFileName, TimeSpan.FromSeconds(note.HitTiming), PlayerSettings.SfxVolume);
                            }
                        }
                    }
                }
                _sfxBufferTime = now;
            }
        }

        public void MusicPlayer_PositionChanged(object sender, EventArgs e) {
            lock (_sfxSyncObject) {
                _sfxBufferTime = _scorePlayer.CurrentTime.TotalSeconds;
                _sfxManager?.StopAll();
                UpdateSfx(_scorePlayer.CurrentTime);
            }
        }

        private void FMain_Load(object sender, EventArgs e) {
            InitializeControls();
            // Enable preview to see more realistic effects.
            editor.IsPreview = true;
            trkFallingSpeed.Value = trkFallingSpeed.Minimum + (int)((float)(trkFallingSpeed.Maximum - trkFallingSpeed.Minimum) / 2);
            trkMusicVolume.Value = (int)(0.7f * (trkMusicVolume.Maximum - trkMusicVolume.Minimum)) + trkMusicVolume.Minimum;
            trkSfxVolume.Value = (int)(0.5f * (trkSfxVolume.Maximum - trkSfxVolume.Minimum)) + trkSfxVolume.Minimum;
        }

        private void BtnSelectScore_Click(object sender, EventArgs e) {
            openFileDialog.Filter = ScoreFilter;
            var result = openFileDialog.ShowDialog();
            if (result != DialogResult.Cancel && openFileDialog.FileName.Length > 0) {
                txtScoreFileName.Text = openFileDialog.FileName;
            }
        }

        private void BtnSelectAudio_Click(object sender, EventArgs e) {
            openFileDialog.Filter = AudioFilter;
            var result = openFileDialog.ShowDialog();
            if (result != DialogResult.Cancel && openFileDialog.FileName.Length > 0) {
                txtAudioFileName.Text = openFileDialog.FileName;
            }
        }

        private void BtnScoreUnload_Click(object sender, EventArgs e) {
            timer.Stop();
            BtnStop_Click(sender, e);
            _sfxManager?.Dispose();
            _sfxManager = null;
            if (_scorePlayer != null) {
                _scorePlayer.PositionChanged -= MusicPlayer_PositionChanged;
                _scorePlayer.PlaybackStopped -= MusicPlayer_PlaybackStopped;
                _scorePlayer.Dispose();
                _scorePlayer = null;
            }
            _musicWaveStream?.Dispose();
            _musicWaveStream = null;
            if (_audioFileStream != null) {
                _audioFileStream.Dispose();
                _audioFileStream = null;
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
            var audioFileName = txtAudioFileName.Text;
            var audioFileInfo = new FileInfo(audioFileName);
            var audioFileExtension = audioFileInfo.Extension.ToLowerInvariant();
            if (audioFileExtension == ExtensionAcb) {
                _audioFileStream = File.Open(audioFileName, FileMode.Open, FileAccess.Read);
                _musicWaveStream = LiveMusicWaveStream.FromAcbStream(_audioFileStream, audioFileName, DefaultCgssDecodeParams);
                PlayerSettings.MusicFileOffset = TimeSpan.Zero;
            } else if (audioFileExtension == ExtensionWav) {
                _audioFileStream = File.Open(audioFileName, FileMode.Open, FileAccess.Read);
                _musicWaveStream = LiveMusicWaveStream.FromWaveStream(_audioFileStream);
                PlayerSettings.MusicFileOffset = TimeSpan.FromSeconds(
                    (double)HCA.HcaEncoderInfo.EncoderDelayInSamples / _musicWaveStream.WaveFormat.SampleRate);
            } else if (audioFileExtension == ExtensionHca) {
                _audioFileStream = File.Open(audioFileName, FileMode.Open, FileAccess.Read);
                _musicWaveStream = LiveMusicWaveStream.FromHcaStream(_audioFileStream, DefaultCgssDecodeParams);
                PlayerSettings.MusicFileOffset = TimeSpan.Zero;
            } else {
                throw new ArgumentOutOfRangeException(nameof(audioFileExtension), $"Unsupported audio format: '{audioFileExtension}'.");
            }
            _scorePlayer = new ScorePlayer();
            _scorePlayer.PlaybackStopped += MusicPlayer_PlaybackStopped;
            _scorePlayer.AddInputStream(_musicWaveStream, PlayerSettings.MusicVolume);
            _sfxManager = new SfxManager(_scorePlayer);
            PreloadNoteSounds();
            _sfxBufferTime = 0d;
            _scorePlayer.PositionChanged += MusicPlayer_PositionChanged;
            var sfxDirName = string.Format(SoundEffectAudioDirectoryNameFormat, cboSoundEffect.SelectedIndex.ToString("00"));
            _currentTapHcaFileName = $"{sfxDirName}/{TapHcaName}";
            _currentFlickHcaFileName = $"{sfxDirName}/{FlickHcaName}";
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
            lblSong.Text = string.Format(SongTipFormat, _musicWaveStream.HcaName);
            timer.Start();
        }

        private void MusicPlayer_PlaybackStopped(object sender, NAudio.Wave.StoppedEventArgs e) {
            BtnStop_Click(this, EventArgs.Empty);
        }

        private void BtnStop_Click(object sender, EventArgs e) {
            if (_scorePlayer == null) {
                return;
            }
            // Neccessary. Sometimes the stack just doesn't clear.
            _userSeekingStack = 0;
            _scorePlayer.Stop();
            _scorePlayer.CurrentTime = TimeSpan.Zero;
            SetControlsEnabled(ViewerState.Loaded);
            lock (_liveMusicSyncObject) {
                _codeValueChange = true;
                trkProgress.Value = trkProgress.Minimum;
                _codeValueChange = false;
            }
            if (!(editor.Disposing || editor.IsDisposed)) {
                editor.SetTime(TimeSpan.Zero);
                editor.Invalidate();
            }
            editor.MouseEventsEnabled = true;
        }

        private void BtnPause_Click(object sender, EventArgs e) {
            _scorePlayer.Pause();
            SetControlsEnabled(ViewerState.LoadedAndPaused);
            editor.MouseEventsEnabled = true;
        }

        private void BtnPlay_Click(object sender, EventArgs e) {
            if (_scorePlayer == null) {
                return;
            }
            if (_scorePlayer.IsPaused) {
                SetControlsEnabled(ViewerState.LoadedAndPlaying);
            } else {
                SetControlsEnabled(ViewerState.LoadedAndPlaying);
            }
            editor.MouseEventsEnabled = false;
            _scorePlayer.Play();
            lblTime.Text = $"{_scorePlayer.CurrentTime}/{_musicWaveStream.TotalTime}";
        }

        private void Timer_Tick(object sender, EventArgs e) {
            var player = _scorePlayer;
            if (player == null) {
                return;
            }
            var elapsed = player.CurrentTime;
            var total = _musicWaveStream.TotalTime;
            if (elapsed >= total) {
                BtnStop_Click(this, EventArgs.Empty);
                return;
            }
            var ratio = elapsed.TotalSeconds / total.TotalSeconds;
            var val = (int)(ratio * (trkProgress.Maximum - trkProgress.Minimum)) + trkProgress.Minimum;
            lock (_liveMusicSyncObject) {
                _codeValueChange = true;
                trkProgress.Value = val;
                _codeValueChange = false;
            }
            lblTime.Text = $"{elapsed}/{total}";
            editor.SetTime(elapsed);
            editor.Invalidate();
            editor.JudgeNotesEnteringOrExiting(elapsed);
            UpdateSfx(elapsed);
        }

    }
}
