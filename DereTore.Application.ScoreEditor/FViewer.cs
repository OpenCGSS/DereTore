using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DereTore.Application.ScoreEditor.Controls;
using DereTore.Application.ScoreEditor.Model;
using DereTore.HCA;
using DereTore.StarlightStage;
using App = System.Windows.Forms.Application;
using Timer = System.Timers.Timer;

namespace DereTore.Application.ScoreEditor {
    public partial class FViewer : Form {

        public FViewer() {
            InitializeComponent();
            RegisterEventHandlers();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void ClosePlayers() {
            SoundManager.Instance.Dispose();
        }

        private void UnregisterEventHandlers() {
            timer.Elapsed -= Timer_Tick;
            btnPlay.Click -= BtnPlay_Click;
            btnStop.Click -= BtnStop_Click;
            btnSelectAcb.Click -= BtnSelectAcb_Click;
            btnSelectScore.Click -= BtnSelectScore_Click;
            pictureBox1.NoteEnteringOrExitingStage -= ScoreEditorControl_NoteUpdated;
            Load -= FMain_Load;
            FormClosing -= FViewer_FormClosing;
        }

        private void RegisterEventHandlers() {
            timer.Elapsed += Timer_Tick;
            btnPlay.Click += BtnPlay_Click;
            btnStop.Click += BtnStop_Click;
            btnSelectAcb.Click += BtnSelectAcb_Click;
            btnSelectScore.Click += BtnSelectScore_Click;
            pictureBox1.NoteEnteringOrExitingStage += ScoreEditorControl_NoteUpdated;
            Load += FMain_Load;
            FormClosing += FViewer_FormClosing;
        }

        private void FViewer_FormClosing(object sender, FormClosingEventArgs e) {
            BtnStop_Click(this, EventArgs.Empty);
            ClosePlayers();
            UnregisterEventHandlers();
        }

        private void ScoreEditorControl_NoteUpdated(object sender, NoteEnteringOrExitingStageEventArgs e) {
            var note = e.Note;
            if (!note.Visible) {
                return;
            }
            if (note.IsSwipe) {
                SoundManager.Instance.PlayHca(SwipeSoundFileName);
            } else if (note.IsTap || note.IsHold) {
                SoundManager.Instance.PlayHca(TapSoundFileName);
            }
        }

        private void FMain_Load(object sender, EventArgs e) {
            InitializeControls();
            PreloadNoteSounds();
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

        private void BtnStop_Click(object sender, EventArgs e) {
            if (_player == null) {
                return;
            }
            _player.Stop();
            timer.Stop();
            _player.Dispose();
            _player = null;
            _acbStream?.Dispose();
            _acbStream = null;
            progress.Value = progress.Minimum;
            cboDifficulty.Enabled = true;
            btnPlay.Enabled = true;
            btnStop.Enabled = false;
            btnSelectAcb.Enabled = true;
            btnSelectScore.Enabled = true;
            lblSong.Text = string.Empty;
            lblTime.Text = string.Empty;
            if (!(pictureBox1.Disposing || pictureBox1.IsDisposed)) {
                pictureBox1.SetTime(TimeSpan.Zero);
                pictureBox1.Invalidate();
            }
        }

        private void BtnPlay_Click(object sender, EventArgs e) {
            if (!CheckPlayEnvironment()) {
                return;
            }
            _acbStream = File.Open(txtAcbFileName.Text, FileMode.Open, FileAccess.Read);
            _player = LiveMusicPlayer.FromStream(_acbStream, txtAcbFileName.Text, DecodeParams);
            _score = Score.FromFile(txtScoreFileName.Text, (Difficulty)(cboDifficulty.SelectedIndex + 1));
            pictureBox1.SetScore(_score);
            cboDifficulty.Enabled = false;
            btnPlay.Enabled = false;
            btnStop.Enabled = true;
            btnSelectAcb.Enabled = false;
            btnSelectScore.Enabled = false;
            lblSong.Text = string.Format(SongTipFormat, _player.HcaName);
            _player.Play();
            timer.Start();
            lblTime.Text = $"{_player.Elapsed}/{_player.TotalLength}";
        }

        private void Timer_Tick(object sender, EventArgs e) {
            var elapsed = _player.Elapsed;
            var total = _player.TotalLength;
            if (elapsed > total) {
                BtnStop_Click(this, EventArgs.Empty);
                return;
            }
            var ratio = elapsed.TotalSeconds / total.TotalSeconds;
            var val = (int)(ratio * (progress.Maximum - progress.Minimum)) + progress.Minimum;
            progress.Value = val;
            lblTime.Text = $"{elapsed}/{_player.TotalLength}";
            pictureBox1.SetTime(elapsed);
            pictureBox1.Invalidate();
            pictureBox1.JudgeNotesEnteringOrExiting(elapsed);
        }

        private bool CheckPlayEnvironment() {
            if (txtAcbFileName.TextLength == 0) {
                Alert("Please select the ACB file.");
                return false;
            }
            if (txtScoreFileName.TextLength == 0) {
                Alert("Please select the score file.");
                return false;
            }
            string[] scoreNames;
            var isScoreFile = Score.IsScoreFile(txtScoreFileName.Text, out scoreNames);
            if (!isScoreFile) {
                Alert($"The file '{txtScoreFileName.Text}' is not a score file.");
                return false;
            }
            if (!Score.ContainsDifficulty(scoreNames, (Difficulty)(cboDifficulty.SelectedIndex + 1))) {
                Alert($"The file '{txtScoreFileName.Text}' does not contain required difficulty '{cboDifficulty.SelectedItem}'.");
                return false;
            }
            return true;
        }

        private void InitializeControls() {
            openFileDialog.CheckFileExists = true;
            openFileDialog.AutoUpgradeEnabled = true;
            openFileDialog.DereferenceLinks = true;
            openFileDialog.Multiselect = false;
            openFileDialog.ReadOnlyChecked = false;
            openFileDialog.ShowReadOnly = false;
            openFileDialog.ValidateNames = true;
            cboDifficulty.SelectedIndex = 0;
            btnStop.Enabled = false;
        }

        private void PreloadNoteSounds() {
            SoundManager.Instance.PreloadHca(SwipeSoundFileName);
            SoundManager.Instance.PreloadHca(TapSoundFileName);
        }

        private DialogResult Alert(string message) {
            return MessageBox.Show(this, message, App.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private static readonly string AcbFilter = "ACB Files|*";
        private static readonly string ScoreFilter = "Score|*";
        private static readonly string DefaultSongName = "song_1001";

        private static readonly string SongTipFormat = "Song: {0}";

        private static readonly string SwipeSoundFileName = "Resources/se_live_flic_perfect.hca";
        private static readonly string TapSoundFileName = "Resources/se_live_tap_perfect.hca";

        private readonly Timer timer = new Timer(5);

        private LiveMusicPlayer _player;
        private Score _score;
        private Stream _acbStream;

        private static readonly DecodeParams DecodeParams = new DecodeParams {
            Key1 = CgssCipher.Key1,
            Key2 = CgssCipher.Key2
        };

    }
}
