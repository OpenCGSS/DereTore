using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using DereTore.Applications.MusicToolchain.Properties;

namespace DereTore.Applications.MusicToolchain {
    public partial class FMain : Form {

        public FMain() {
            InitializeComponent();
            RegisterEventHandlers();
        }

        ~FMain() {
            UnregisterEventHandlers();
        }

        private void UnregisterEventHandlers() {
            Load -= FMain_Load;
            txtSourceWaveFile.DragDrop -= TxtSourceWaveFile_DragDrop;
            txtSourceWaveFile.DragEnter -= TxtSourceWaveFile_DragEnter;
            txtSourceWaveFile.QueryContinueDrag -= TxtSourceWaveFile_QueryContinueDrag;
            btnBrowseSourceWaveFile.Click -= BtnBrowseSourceWaveFile_Click;
            btnBrowseSaveLocation.Click -= BtnBrowseSaveLocation_Click;
            btnGo.Click -= BtnGo_Click;
        }

        private void RegisterEventHandlers() {
            Load += FMain_Load;
            txtSourceWaveFile.DragDrop += TxtSourceWaveFile_DragDrop;
            txtSourceWaveFile.DragEnter += TxtSourceWaveFile_DragEnter;
            txtSourceWaveFile.QueryContinueDrag += TxtSourceWaveFile_QueryContinueDrag;
            btnBrowseSourceWaveFile.Click += BtnBrowseSourceWaveFile_Click;
            btnBrowseSaveLocation.Click += BtnBrowseSaveLocation_Click;
            btnGo.Click += BtnGo_Click;
        }

        private void BtnBrowseSaveLocation_Click(object sender, EventArgs e) {
            var result = saveFileDialog.ShowDialog();
            if (result != DialogResult.Cancel && saveFileDialog.FileName.Length > 0) {
                txtSaveLocation.Text = saveFileDialog.FileName;
            }
        }

        private void BtnGo_Click(object sender, EventArgs e) {
            if (!CheckBeforeGo()) {
                return;
            }
            DisableControls();
            var thread = new Thread(Go) {
                IsBackground = true,
            };
            thread.Start(new StartOptions {
                WaveFileName = txtSourceWaveFile.Text,
                Key1 = txtKey1.Text,
                Key2 = txtKey2.Text,
                AcbFileName = txtSaveLocation.Text,
                SongName = "song_" + txtSongName.Text,
                ShouldCreateLz4 = chkAlsoCreateLz4.Checked
            });
        }

        private void TxtSourceWaveFile_DragEnter(object sender, DragEventArgs e) {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void BtnBrowseSourceWaveFile_Click(object sender, EventArgs e) {
            openFileDialog.FileName = string.Empty;
            var result = openFileDialog.ShowDialog();
            if (result != DialogResult.Cancel && openFileDialog.FileName.Length > 0) {
                txtSourceWaveFile.Text = openFileDialog.FileName;
            }
        }

        private void TxtSourceWaveFile_QueryContinueDrag(object sender, QueryContinueDragEventArgs e) {
            e.Action = DragAction.Continue;
        }

        private void TxtSourceWaveFile_DragDrop(object sender, DragEventArgs e) {
            var dataObject = e.Data as DataObject;
            if (dataObject == null || !dataObject.ContainsFileDropList()) {
                e.Effect = DragDropEffects.None;
                return;
            }
            var fileList = dataObject.GetFileDropList();
            if (fileList.Count > 1) {
                e.Effect = DragDropEffects.None;
                LogError("Please drag a single file.");
                return;
            }
            var fileName = fileList[0];
            if (!fileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)) {
                e.Effect = DragDropEffects.None;
                LogError($"The file '{fileName}' is not a wave file.");
                return;
            }
            e.Effect = DragDropEffects.Copy;
            txtSourceWaveFile.Text = fileName;
        }

        private void FMain_Load(object sender, EventArgs e) {
            if (CheckEnvironment()) {
                InitializeControls();
            }
        }

        private void Go(object paramObject) {
            var options = (StartOptions)paramObject;
            string temp1, temp2;
            int code;
            var startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            Log("Encoding HCA...");
            startInfo.FileName = "hcaenc.exe";
            temp1 = Path.GetTempFileName();
            Log($"Target: {temp1}");
            startInfo.Arguments = GetArgsString(SanitizeString(options.WaveFileName), SanitizeString(temp1));
            using (var proc = Process.Start(startInfo)) {
                proc.WaitForExit();
                code = proc.ExitCode;
            }
            if (code != 0) {
                LogError($"hcaenc exited with code {code}.");
                if (File.Exists(temp1)) {
                    File.Delete(temp1);
                }
                EnableControls();
                return;
            }
            Log("Encoding finished.");

            Log("Converting HCA...");
            if (options.Key1.Length > 0 && options.Key2.Length > 0) {
                startInfo.FileName = "hcacc.exe";
                temp2 = Path.GetTempFileName();
                Log($"Target: {temp2}");
                startInfo.Arguments = GetArgsString(SanitizeString(temp1), SanitizeString(temp2), "-ot", "56", "-o1", options.Key1, "-o2", options.Key2);
                using (var proc = Process.Start(startInfo)) {
                    proc.WaitForExit();
                    code = proc.ExitCode;
                }
                if (code != 0) {
                    LogError($"hcacc exited with code {code}.");
                    if (File.Exists(temp1)) {
                        File.Delete(temp1);
                    }
                    if (File.Exists(temp2)) {
                        File.Delete(temp2);
                    }
                    EnableControls();
                    return;
                }
                Log("Conversion finished.");
            } else {
                temp2 = temp1;
                Log("Unnecessary.");
            }

            Log("Packing ACB...");
            startInfo.FileName = "AcbMaker.exe";
            Log($"Target: {options.AcbFileName}");
            startInfo.Arguments = GetArgsString(SanitizeString(temp2), SanitizeString(options.AcbFileName), "-n", options.SongName);
            using (var proc = Process.Start(startInfo)) {
                proc.WaitForExit();
                code = proc.ExitCode;
            }
            if (code != 0) {
                LogError($"AcbMaker exited with code {code}.");
                if (File.Exists(temp1)) {
                    File.Delete(temp1);
                }
                if (File.Exists(temp2)) {
                    File.Delete(temp2);
                }
                EnableControls();
                return;
            }
            Log("ACB packing finished.");
            if (File.Exists(temp1)) {
                File.Delete(temp1);
            }
            if (File.Exists(temp2)) {
                File.Delete(temp2);
            }

            if (options.ShouldCreateLz4) {
                Log("Performing LZ4 compression...");
                startInfo.FileName = "LZ4.exe";
                var lz4CompressedFileName = options.AcbFileName + ".lz4";
                startInfo.Arguments = GetArgsString(SanitizeString(options.AcbFileName), SanitizeString(lz4CompressedFileName));
                using (var proc = Process.Start(startInfo)) {
                    proc.WaitForExit();
                    code = proc.ExitCode;
                }
                if (code != 0) {
                    LogError($"LZ4 exited with code {code}.");
                    EnableControls();
                    return;
                }
                Log("LZ4 compression finished.");
            }

            EnableControls();
        }

        private void DisableControls() {
            if (InvokeRequired) {
                Invoke(_disableControlsDelegate);
                return;
            }
            foreach (Control control in Controls) {
                var textBox = control as TextBox;
                if (textBox != null) {
                    if (!textBox.ReadOnly) {
                        textBox.Enabled = false;
                    }
                } else {
                    control.Enabled = false;
                }
            }
        }

        private void EnableControls() {
            if (InvokeRequired) {
                Invoke(_enableControlsDelegate);
                return;
            }
            foreach (Control control in Controls) {
                var textBox = control as TextBox;
                if (textBox != null) {
                    if (!textBox.ReadOnly) {
                        textBox.Enabled = true;
                    }
                }
                control.Enabled = true;
            }
        }

        private bool CheckEnvironment() {
            Log("Checking environment...");
            _logDelegate = Log;
            _enableControlsDelegate = EnableControls;
            _disableControlsDelegate = DisableControls;
            var criticalFiles = new[] { "hcaenc.exe", "hcacc.exe", "AcbMaker.exe", "LZ4.exe", "hcaenc_lite.dll" };
            var missingFiles = criticalFiles.Where(s => !File.Exists(s)).ToList();
            if (missingFiles.Count > 0) {
                DisableControls();
                LogError("Missing file(s): " + missingFiles.BuildString());
                return false;
            } else {
                Log("Environment is OK.");
                return true;
            }
        }

        private bool CheckBeforeGo() {
            if (txtSourceWaveFile.TextLength == 0) {
                LogError("Please specify the source file.");
                return false;
            }
            if (txtSaveLocation.TextLength == 0) {
                LogError("Please specify the save location.");
                return false;
            }
            if (txtSongName.TextLength == 0) {
                LogError("Please enter the song name.");
                return false;
            }
            var songName = txtSongName.Text;
            if (songName.Any(c => c > 127)) {
                LogError("Unsupported song name. Please make sure the name consists only ASCII characters.");
                return false;
            }
            if (songName.Any(c => c < '0' || c > '9')) {
                LogError("Song name must be 'song_####' where '#' is a digit.");
                return false;
            }
            if (songName.Length != 4) {
                LogError("Song name must constist of 4 digits.");
                return false;

            }
            var keyRegex = new Regex(@"^[0-9A-Fa-f]{8}$", RegexOptions.CultureInvariant);
            if (txtKey1.TextLength > 0) {
                if (!keyRegex.IsMatch(txtKey1.Text)) {
                    LogError("Format of key #1 is invalid.");
                    return false;
                }
            }
            if (txtKey2.TextLength > 0) {
                if (!keyRegex.IsMatch(txtKey2.Text)) {
                    LogError("Format of key #2 is invalid.");
                    return false;
                }
            }
            return true;
        }

        private void InitializeControls() {
            txtKey1.Text = CgssKey1.ToString("x8");
            txtKey2.Text = CgssKey2.ToString("x8");
            txtSongName.Text = DefaultSongNumber.ToString();
            txtSourceWaveFile.AllowDrop = true;
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
            openFileDialog.Multiselect = false;
            openFileDialog.AutoUpgradeEnabled = true;
            openFileDialog.DereferenceLinks = true;
            openFileDialog.ShowReadOnly = false;
            openFileDialog.ReadOnlyChecked = false;
            saveFileDialog.ValidateNames = true;
            openFileDialog.Filter = Resources.BrowseForWaveFilter;
            saveFileDialog.AutoUpgradeEnabled = true;
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.ValidateNames = true;
            saveFileDialog.Filter = Resources.BrowseForAcbFilter;
        }

        private void Log(string message) {
            if (InvokeRequired) {
                Invoke(_logDelegate, message);
            } else {
                txtLog.AppendText(message);
                txtLog.AppendText(Environment.NewLine);
                txtLog.SelectionStart = txtLog.TextLength;
            }
        }

        private void LogError(string message) {
            Log("[ERROR] " + message);
        }

        private static string GetArgsString(params string[] args) {
            return args.Aggregate((total, next) => total + " " + next);
        }

        private static string SanitizeString(string s) {
            var shouldCoverWithQuotes = false;
            if (s.IndexOf('"') >= 0) {
                s = s.Replace("\"", "\"\"\"");
                shouldCoverWithQuotes = true;
            }
            if (s.IndexOfAny(CommandlineEscapeChars) >= 0) {
                shouldCoverWithQuotes = true;
            }
            if (s.Any(c => c > 127)) {
                shouldCoverWithQuotes = true;
            }
            return shouldCoverWithQuotes ? "\"" + s + "\"" : s;
        }

        private static readonly uint CgssKey1 = 0xF27E3B22;
        private static readonly uint CgssKey2 = 0x00003657;
        private static readonly int DefaultSongNumber = 1001;
        private static readonly char[] CommandlineEscapeChars = { ' ', '&', '%', '#', '@', '!', ',', '~', '+', '=', '(', ')' };
        private Action<string> _logDelegate;
        private Action _enableControlsDelegate;
        private Action _disableControlsDelegate;

        private sealed class StartOptions {

            public string WaveFileName { get; set; }
            public string Key1 { get; set; }
            public string Key2 { get; set; }
            public string AcbFileName { get; set; }
            public string SongName { get; set; }
            public bool ShouldCreateLz4 { get; set; }

        }

    }
}
