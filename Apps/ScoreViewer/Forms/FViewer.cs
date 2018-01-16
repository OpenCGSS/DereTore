using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using DereTore.Apps.ScoreViewer.Model;
using DereTore.Common.StarlightStage;
using DereTore.Exchange.Audio.HCA;
using DereTore.Interop.OS;
//using Timer = System.Timers.Timer;
using Timer = DereTore.Apps.ScoreViewer.ThreadingTimer;

namespace DereTore.Apps.ScoreViewer.Forms {
    public partial class FViewer : Form {

        static FViewer() {
            try {
                NativeStructures.DEVMODE devMode;
                NativeMethods.EnumDisplaySettings(null, NativeConstants.ENUM_CURRENT_SETTINGS, out devMode);
                ScreenRefreshRate = devMode.dmDisplayFrequency;
            } catch (Exception ex) {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
                ScreenRefreshRate = 60;
            }
        }

        public FViewer() {
            InitializeComponent();
            RegisterEventHandlers();
            CheckForIllegalCrossThreadCalls = false;
        }

        private bool CheckPlayEnvironment() {
            if (txtAudioFileName.TextLength == 0) {
                this.ShowMessageBox("Please select the ACB file.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            if (txtScoreFileName.TextLength == 0) {
                this.ShowMessageBox("Please select the score file.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            if (!File.Exists(txtAudioFileName.Text)) {
                this.ShowMessageBox($"The audio file '{txtAudioFileName.Text}' does not exist.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            if (!File.Exists(txtScoreFileName.Text)) {
                this.ShowMessageBox($"The score file '{txtScoreFileName.Text}' does not exist.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            var scoreFileName = txtScoreFileName.Text;
            var scoreFileExtension = new FileInfo(scoreFileName).Extension.ToLowerInvariant();
            if (scoreFileExtension == ExtensionBdb) {
                string[] scoreNames;
                var isScoreFile = Score.IsScoreFile(txtScoreFileName.Text, out scoreNames);
                if (!isScoreFile) {
                    this.ShowMessageBox($"The file '{scoreFileName}' is not a score database file.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
                if (!Score.ContainsDifficulty(scoreNames, (Difficulty)(cboDifficulty.SelectedIndex + 1))) {
                    this.ShowMessageBox($"The file '{scoreFileName}' does not contain required difficulty '{cboDifficulty.SelectedItem}'.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            } else if (scoreFileExtension == ExtensionCsv) {
                // Don't have an idea how to fully check the file.
            } else {
                this.ShowMessageBox($"The file {scoreFileName} is neither a score database or a single score file.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            return true;
        }

        private void ClosePlayers() {
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
            cboSoundEffect.SelectedIndex = 0;
            SetControlsEnabled(ViewerState.Initialized);
        }

        private void PreloadNoteSounds() {
            const int sfxTypeCount = 4;
            for (var i = 0; i < sfxTypeCount; ++i) {
                var sfxDirName = string.Format(SoundEffectAudioDirectoryNameFormat, i.ToString("00"));
                foreach (var waveAudioName in new[] { TapHcaName, FlickHcaName }) {
                    var fileName = $"{sfxDirName}/{waveAudioName}";
                    _sfxManager.PreloadWave(fileName);
                }
            }
        }

        private void SetControlsEnabled(ViewerState state) {
            switch (state) {
                case ViewerState.Initialized:
                    btnSelectAudio.Enabled = true;
                    btnSelectScore.Enabled = true;
                    cboDifficulty.Enabled = true;
                    cboSoundEffect.Enabled = true;
                    trkProgress.Enabled = false;
                    btnScoreLoad.Enabled = true;
                    btnScoreUnload.Enabled = false;
                    btnPlay.Enabled = false;
                    btnPause.Enabled = false;
                    btnStop.Enabled = false;
                    chkLimitFrameRate.Enabled = true;
                    break;
                case ViewerState.Loaded:
                    btnSelectAudio.Enabled = false;
                    btnSelectScore.Enabled = false;
                    cboDifficulty.Enabled = false;
                    cboSoundEffect.Enabled = false;
                    trkProgress.Enabled = true;
                    btnScoreLoad.Enabled = false;
                    btnScoreUnload.Enabled = true;
                    btnPlay.Enabled = true;
                    btnPause.Enabled = false;
                    btnStop.Enabled = false;
                    chkLimitFrameRate.Enabled = true;
                    break;
                case ViewerState.LoadedAndPlaying:
                    btnSelectAudio.Enabled = false;
                    btnSelectScore.Enabled = false;
                    cboDifficulty.Enabled = false;
                    cboSoundEffect.Enabled = false;
                    trkProgress.Enabled = true;
                    btnScoreLoad.Enabled = false;
                    btnScoreUnload.Enabled = true;
                    btnPlay.Enabled = false;
                    btnPause.Enabled = true;
                    btnStop.Enabled = true;
                    chkLimitFrameRate.Enabled = false;
                    break;
                case ViewerState.LoadedAndPaused:
                    btnSelectAudio.Enabled = false;
                    btnSelectScore.Enabled = false;
                    cboDifficulty.Enabled = false;
                    cboSoundEffect.Enabled = false;
                    trkProgress.Enabled = true;
                    btnScoreLoad.Enabled = false;
                    btnScoreUnload.Enabled = true;
                    btnPlay.Enabled = true;
                    btnPause.Enabled = false;
                    btnStop.Enabled = true;
                    chkLimitFrameRate.Enabled = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private static bool _naiveFrameLimiterEnabled = true;

        private static readonly string DefaultTitle = "DereTore: Score Viewer";
        private static readonly uint ScreenRefreshRate;

        private static readonly string AudioFilter = "All Supported Audio Formats (*.wav;*.acb;*.hca)|*.wav;*.acb;*.hca|ACB Archive (*.acb)|*.acb|Wave Audio (*.wav)|*.wav|HCA Audio (*.hca)|*.hca";
        private static readonly string ScoreFilter = "All Supported Score Formats (*.bdb;*.csv)|*.bdb;*.csv|Score Database (*.bdb)|*.bdb|Single Score (*.csv)|*.csv";

        private static readonly string ExtensionAcb = ".acb";
        private static readonly string ExtensionWav = ".wav";
        private static readonly string ExtensionHca = ".hca";
        private static readonly string ExtensionBdb = ".bdb";
        private static readonly string ExtensionCsv = ".csv";

        private static readonly string SongTipFormat = "Song: {0}";

        private static readonly string SoundEffectAudioDirectoryNameFormat = "Resources/SFX/se_live_{0}";
        private static readonly string FlickHcaName = "se_live_flic_perfect.wav";
        private static readonly string TapHcaName = "se_live_tap_perfect.wav";
        private string _currentFlickHcaFileName;
        private string _currentTapHcaFileName;

        private readonly Timer timer = new Timer(5);
        private uint _lastRedrawTime;

        private AudioManager _audioManager;
        private ScorePlayer _scorePlayer;
        private LiveMusicWaveStream _musicWaveStream;
        private SfxManager _sfxManager;
        private double _sfxBufferTime;
        private Score _score;
        private Stream _audioFileStream;

        private int _userSeekingStack = 0;

        private static readonly DecodeParams DefaultCgssDecodeParams = DecodeParams.CreateDefault(CgssCipher.Key1, CgssCipher.Key2);

        private bool _codeValueChange;
        private readonly object _liveMusicSyncObject = new object();
        private readonly object _sfxSyncObject = new object();

    }
}
