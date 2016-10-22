using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Entities.Extensions;
using DereTore.Applications.StarlightDirector.Extensions;
using DereTore.StarlightStage;
using LZ4;
using Microsoft.Win32;

namespace DereTore.Applications.StarlightDirector.UI.Controls.Pages {
    partial class BuildPage {

        public static readonly ICommand CmdBuildMusicArchive = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdRecheckAcbBuildEnvironment = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdBuildScoreDatabase = CommandHelper.RegisterCommand();

        private void CmdBuildMusicArchive_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Project?.HasMusic ?? false;
        }

        private void CmdBuildMusicArchive_Executed(object sender, ExecutedRoutedEventArgs e) {
            var selectedRecord = (LiveMusicRecord)((ComboBoxItem)CboSongList.SelectedItem).Tag;
            var songName = $"song_{selectedRecord.MusicID:0000}";
            var standardFileName = $"{songName}.acb";
            var saveDialog = new SaveFileDialog {
                OverwritePrompt = true,
                ValidateNames = true,
                Filter = Application.Current.FindResource<string>(App.ResourceKeys.AcbFileFilter),
                FileName = standardFileName
            };
            var saveResult = saveDialog.ShowDialog();
            if (saveResult ?? false) {
                var so = new StartOptions {
                    AcbFileName = saveDialog.FileName,
                    SongName = songName,
                    Key1 = CgssCipher.Key1.ToString("x8"),
                    Key2 = CgssCipher.Key2.ToString("x8"),
                    WaveFileName = Project.MusicFileName
                };
                var thread = new Thread(BuildAcb) {
                    IsBackground = true
                };
                thread.Start(so);
            }
        }

        private void CmdRecheckAcbBuildEnvironment_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void CmdRecheckAcbBuildEnvironment_Executed(object sender, ExecutedRoutedEventArgs e) {
            AcbBuildLog.Items.Clear();
            Log(Application.Current.FindResource<string>(App.ResourceKeys.AcbEnvironmentRechecking));
            CheckAcbBuildingEnvironment();
        }

        private void CmdBuildScoreDatabase_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = (Project?.Scores?.Count ?? 0) > 0;
        }

        private void CmdBuildScoreDatabase_Executed(object sender, ExecutedRoutedEventArgs e) {
            var selectedRecord = (LiveMusicRecord)((ComboBoxItem)CboSongList.SelectedItem).Tag;
            var standardFileName = $"musicscores_m{selectedRecord.LiveID:000}.bdb";
            string lz4FileName = null;
            if (CreateLz4CompressedBdbFile) {
                lz4FileName = standardFileName + ".lz4";
            }
            var saveDialog = new SaveFileDialog {
                OverwritePrompt = true,
                ValidateNames = true,
                Filter = Application.Current.FindResource<string>(App.ResourceKeys.BdbFileFilter),
                FileName = standardFileName
            };
            var saveResult = saveDialog.ShowDialog();
            if (!(saveResult ?? false)) {
                return;
            }
            if (lz4FileName != null) {
                lz4FileName = Path.Combine((new FileInfo(saveDialog.FileName)).DirectoryName ?? string.Empty, lz4FileName);
            }
            try {
                var difficultyMappings = new Dictionary<Difficulty, Difficulty> {
                    { Difficulty.Debut, MappingDebut },
                    { Difficulty.Regular, MappingRegular },
                    { Difficulty.Pro, MappingPro },
                    { Difficulty.Master, MappingMaster },
                    { Difficulty.MasterPlus, MappingMasterPlus }
                };
                BuildBdb(Project, saveDialog.FileName, selectedRecord, difficultyMappings);
                if (lz4FileName != null) {
                    var fileData = File.ReadAllBytes(saveDialog.FileName);
                    var compressedFileData = LZ4Codec.EncodeHC(fileData, 0, fileData.Length);
                    using (var compressedFileStream = File.Open(lz4FileName, FileMode.Create, FileAccess.Write)) {
                        // LZ4 header
                        compressedFileStream.WriteInt32LE(0x00000064);
                        compressedFileStream.WriteInt32LE(fileData.Length);
                        compressedFileStream.WriteInt32LE(compressedFileData.Length);
                        compressedFileStream.WriteInt32LE(0x00000001);
                        // File data
                        compressedFileStream.WriteBytes(compressedFileData);
                    }
                }
                var format = lz4FileName == null ?
                    Application.Current.FindResource<string>(App.ResourceKeys.BdbBuildingCompletePromptTemplate1) :
                    Application.Current.FindResource<string>(App.ResourceKeys.BdbBuildingCompletePromptTemplate2);
                var message = string.IsNullOrEmpty(lz4FileName) ? string.Format(format, saveDialog.FileName) : string.Format(format, saveDialog.FileName, lz4FileName);
                MessageBox.Show(message, App.Title, MessageBoxButton.OK, MessageBoxImage.Information);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, App.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private static void BuildBdb(Project project, string bdbFileName, LiveMusicRecord record, Dictionary<Difficulty, Difficulty> difficultyMappings) {
            var connectionString = $"Data Source={bdbFileName}";
            if (File.Exists(bdbFileName)) {
                File.Delete(bdbFileName);
            } else {
                SQLiteConnection.CreateFile(bdbFileName);
            }
            using (var connection = new SQLiteConnection(connectionString)) {
                connection.Open();
                using (var transaction = connection.BeginTransaction()) {
                    SQLiteCommand command;
                    using (command = transaction.Connection.CreateCommand()) {
                        command.CommandText = "CREATE TABLE blobs (name TEXT PRIMARY KEY, data BLOB NOT NULL);";
                        command.ExecuteNonQuery();
                    }
                    using (command = transaction.Connection.CreateCommand()) {
                        command.CommandText = "INSERT INTO blobs (name, data) VALUES (@name, @data);";
                        var nameParam = command.Parameters.Add("name", DbType.AnsiString);
                        var dataParam = command.Parameters.Add("data", DbType.Binary);
                        // update: Create Master+ entry, regardless of its existence in original BDB.
                        for (var i = (int)Difficulty.Debut; i <= (int)Difficulty.MasterPlus; ++i) {
                            var entryDifficulty = (Difficulty)i;
                            var userDifficulty = difficultyMappings[entryDifficulty];
                            var score = project.GetScore(userDifficulty);
                            var compiledScore = score.Compile();
                            var csv = compiledScore.GetCsvString();
                            var csvData = Encoding.ASCII.GetBytes(csv);
                            nameParam.Value = string.Format(ScoreRecordFormat, record.LiveID, i);
                            dataParam.Value = csvData;
                            command.ExecuteNonQuery();
                        }
                        nameParam.Value = string.Format(Score2DCharaFormat, record.LiveID);
                        dataParam.Value = Encoding.ASCII.GetBytes(Score2DCharaText);
                        command.ExecuteNonQuery();
                        nameParam.Value = string.Format(ScoreCyalumeFormat, record.LiveID);
                        dataParam.Value = Encoding.ASCII.GetBytes(ScoreCyalumeText);
                        command.ExecuteNonQuery();
                        nameParam.Value = string.Format(ScoreLyricsFormat, record.LiveID);
                        dataParam.Value = Encoding.ASCII.GetBytes(ScoreLyricsText);
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                connection.Close();
            }
        }

        private void BuildAcb(object paramObject) {
            var options = (StartOptions)paramObject;
            string temp1 = null, temp2 = null;
            try {
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
                    EnableAcbBuildButton(true);
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
                        EnableAcbBuildButton(true);
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
                    EnableAcbBuildButton(true);
                    return;
                }
                Log("ACB packing finished.");
                if (File.Exists(temp1)) {
                    File.Delete(temp1);
                }
                if (File.Exists(temp2)) {
                    File.Delete(temp2);
                }
            } catch (Exception ex) when (!(ex is ThreadAbortException)) {
                MessageBox.Show(ex.Message, App.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            } finally {
                if (temp1 != null && File.Exists(temp1)) {
                    File.Delete(temp1);
                }
                if (temp2 != null && File.Exists(temp2)) {
                    File.Delete(temp2);
                }
                EnableAcbBuildButton(true);
            }
        }

        private static readonly string ScoreRecordFormat = "musicscores/m{0:000}/{0}_{1}.csv";
        private static readonly string Score2DCharaFormat = "musicscores/m{0:000}/m{0:000}_2dchara.csv";
        private static readonly string ScoreCyalumeFormat = "musicscores/m{0:000}/m{0:000}_cyalume.csv";
        private static readonly string ScoreLyricsFormat = "musicscores/m{0:000}/m{0:000}_lyrics.csv";
        private static readonly string Score2DCharaText = "0.03333,1,w1,,\n0.03333,2,w1,,\n0.03333,3,w1,,\n0.03333,4,w1,,\n0.03333,5,w1,,\n0.03333,MOT_mc_bg_black_00,seat100-0,,";
        private static readonly string ScoreCyalumeText = "time,move_type,BPM,color_pattern,color1,color2,color3,color4,color5,width1,width2,width3,width4,width5,3D_move_type,3D_BPM\n0,Uhoi,5,Random3,Pink01,Blue01,Yellow01,,,,,,,,Uhoi,5";
        private static readonly string ScoreLyricsText = "time,lyrics,size\n0,,";

        private static readonly char[] CommandlineEscapeChars = { ' ', '&', '%', '#', '@', '!', ',', '~', '+', '=', '(', ')' };
        private readonly Action<string> _logDelegate;
        private readonly Action<bool> _enableAcbBuildButtoDelegate;

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

        private sealed class StartOptions {

            public string WaveFileName { get; set; }
            public string Key1 { get; set; }
            public string Key2 { get; set; }
            public string AcbFileName { get; set; }
            public string SongName { get; set; }

        }

    }
}
