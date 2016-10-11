using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DereTore.Applications.StarlightDirector.Components;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;
using Microsoft.Win32;

namespace DereTore.Applications.StarlightDirector.UI.Controls.Pages {
    partial class BuildPage {

        public static readonly ICommand CmdBuildMusicArchive = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdBuildScoreDatabase = CommandHelper.RegisterCommand();

        private void CmdBuildMusicArchive_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Project?.HasMusic ?? false;
        }

        private void CmdBuildMusicArchive_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.Print("Not implemented: build music archive");
        }

        private void CmdBuildScoreDatabase_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = (Project?.Scores?.Count ?? 0) > 0;
        }

        private void CmdBuildScoreDatabase_Executed(object sender, ExecutedRoutedEventArgs e) {
            var selectedRecord = (LiveMusicRecord)((ComboBoxItem)CboSongList.SelectedItem).Tag;
            var standardFileName = $"musicscores_m{selectedRecord.LiveID:000}.bdb";
            string hashedFileName = null;
            if (CreateNameHashedBdbFile) {
                hashedFileName = StringHasher.GetHash(standardFileName, StringHasher.Sha1);
            }
            var saveDialog = new SaveFileDialog {
                OverwritePrompt = true,
                ValidateNames = true,
                Filter = Application.Current.FindResource<string>(App.ResourceKeys.BdbFileFilter),
                FileName = standardFileName
            };
            var saveResult = saveDialog.ShowDialog();
            if (saveResult ?? false) {
                if (hashedFileName != null) {
                    hashedFileName = Path.Combine((new FileInfo(saveDialog.FileName)).DirectoryName ?? string.Empty, hashedFileName);
                }
                var mainWindow = this.GetMainWindow();
                try {
                    var warnings = new List<string>();
                    BuildBdb(Project, saveDialog.FileName, hashedFileName, selectedRecord, warnings);
                    if (warnings.Count > 0) {
                        MessageBox.Show(warnings.BuildString(Environment.NewLine), mainWindow?.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    var format = string.IsNullOrEmpty(hashedFileName) ?
                        Application.Current.FindResource<string>(App.ResourceKeys.BdbBuildingCompletePromptTemplate1) :
                        Application.Current.FindResource<string>(App.ResourceKeys.BdbBuildingCompletePromptTemplate2);
                    var message = string.IsNullOrEmpty(hashedFileName) ? string.Format(format, saveDialog.FileName) : string.Format(format, saveDialog.FileName, hashedFileName);
                    MessageBox.Show(message, mainWindow?.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message, mainWindow?.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        private void BuildBdb(Project project, string bdbFileName, string hashedBdbFileName, LiveMusicRecord record, List<string> warnings) {
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
                        for (var i = (int)Difficulty.Debut; i <= (int)Difficulty.MasterPlus; ++i) {
                            var score = project.GetScore((Difficulty)i);
                            if (!record.DifficultyExists[i - 1]) {
                                if (score.HasAnyNote) {
                                    var format = Application.Current.FindResource<string>(App.ResourceKeys.NoCorrespondingDifficultyExistsPromptTemplate);
                                    warnings.Add(string.Format(format, DescribedEnumReader.Read((Difficulty)i, typeof(Difficulty)), record.LiveID.ToString("000")));
                                }
                                continue;
                            }
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
            if (!string.IsNullOrEmpty(hashedBdbFileName)) {
                File.Copy(bdbFileName, hashedBdbFileName, true);
            }
        }

        private static readonly string ScoreRecordFormat = "musicscores/m{0:000}/{0}_{1}.csv";
        private static readonly string Score2DCharaFormat = "musicscores/m{0:000}/m{0:000}_2dchara.csv";
        private static readonly string ScoreCyalumeFormat = "musicscores/m{0:000}/m{0:000}_cyalume.csv";
        private static readonly string ScoreLyricsFormat = "musicscores/m{0:000}/m{0:000}_lyrics.csv";
        private static readonly string Score2DCharaText = "0.03333,1,w1,,\n0.03333,2,w1,,\n0.03333,3,w1,,\n0.03333,4,w1,,\n0.03333,5,w1,,\n0.03333,MOT_mc_bg_black_00,seat100-0,,";
        private static readonly string ScoreCyalumeText = "time,move_type,BPM,color_pattern,color1,color2,color3,color4,color5,width1,width2,width3,width4,width5,3D_move_type,3D_BPM\n0,Uhoi,5,Random3,Pink01,Blue01,Yellow01,,,,,,,,Uhoi,5";
        private static readonly string ScoreLyricsText = "time,lyrics,size\n0,,";

    }
}
