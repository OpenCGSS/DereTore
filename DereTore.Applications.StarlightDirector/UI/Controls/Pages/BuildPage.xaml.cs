using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;

namespace DereTore.Applications.StarlightDirector.UI.Controls.Pages {
    public partial class BuildPage : IDirectorPage {

        public BuildPage() {
            InitializeComponent();
            CommandHelper.InitializeCommandBindings(this);
            _logDelegate = Log;
            _enableAcbBuildButtoDelegate = EnableAcbBuildButton;
        }

        private void BuildPage_OnLoaded(object sender, RoutedEventArgs e) {
            if (!_musicListInitialized) {
                FillMusicComboBoxes();
                CheckAcbBuildingEnvironment();
                AcbBuildLogScroller.ScrollToEnd();
                _musicListInitialized = true;
            }
        }

        private void FillMusicComboBoxes() {
            var connectionString = $"Data Source={MasterMdbPath}";
            var musicList = new List<LiveMusicRecord>();
            try {
                using (var connection = new SQLiteConnection(connectionString)) {
                    connection.Open();
                    using (var adapter = new SQLiteDataAdapter(FormatFilter, connection)) {
                        using (var dataTable = new DataTable()) {
                            adapter.Fill(dataTable);
                            foreach (DataRow dataRow in dataTable.Rows) {
                                var liveID = (long)dataRow["live_id"];
                                if (liveID >= 500) {
                                    // Event songs, or お願い！シンデレラ (solo ver.)
                                    continue;
                                }
                                var record = new LiveMusicRecord() {
                                    LiveID = (int)(long)dataRow["live_id"],
                                    MusicID = (int)(long)dataRow["music_id"],
                                    MusicName = ((string)dataRow["music_name"]).Replace(@" \n", " ").Replace(@"\n", " "),
                                    DifficultyExists = new bool[5]
                                };
                                for (var i = (int)Difficulty.Debut; i <= (int)Difficulty.MasterPlus; ++i) {
                                    var v = (int)(long)dataRow["d" + i];
                                    record.DifficultyExists[i - 1] = v > 0;
                                }
                                musicList.Add(record);
                            }
                        }
                    }
                    connection.Close();
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, App.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            foreach (var record in musicList) {
                var item = new ComboBoxItem();
                item.Content = record.MusicName;
                item.Tag = record;
                CboSongList.Items.Add(item);
            }
        }

        private void CheckAcbBuildingEnvironment() {
            var criticalFiles = new[] { "hcaenc.exe", "hcacc.exe", "AcbMaker.exe", "LZ4.exe", "hcaenc_lite.dll" };
            var missingFiles = criticalFiles.Where(s => !File.Exists(s)).ToList();
            if (missingFiles.Count > 0) {
                LogError(Application.Current.FindResource<string>(App.ResourceKeys.AcbEnvironmentFilesMissing));
                foreach (var missingFile in missingFiles) {
                    Log(missingFile);
                }
            } else {
                Log("Environment is OK.");
                IsAcbBuildingEnvironmentOK = true;
            }
        }

        private void LogError(string error) {
            var format = Application.Current.FindResource<string>(App.ResourceKeys.AcbEnvironmentLogErrorTemplate);
            var message = string.Format(format, error);
            Log(message);
        }

        private void Log(string message) {
            if (Dispatcher.CheckAccess()) {
                var listItem = new ListBoxItem {
                    Content = message
                };
                AcbBuildLog.Items.Add(listItem);
                AcbBuildLogScroller.ScrollToEnd();
            } else {
                Dispatcher.Invoke(_logDelegate, message);
            }
        }

        private void EnableAcbBuildButton(bool enabled) {
            if (Dispatcher.CheckAccess()) {
                IsAcbBuildingEnvironmentOK = enabled;
            } else {
                Dispatcher.Invoke(_enableAcbBuildButtoDelegate, enabled);
            }
        }

        private bool _musicListInitialized;

        private static readonly string MasterMdbPath = "Resources/GameData/master.mdb";

        private static readonly string FormatFilter = @"
SELECT live_data.id AS live_id, music_data.id AS music_id, music_data.name AS music_name,
    difficulty_1 AS d1, difficulty_2 AS d2, difficulty_3 AS d3, difficulty_4 AS d4, difficulty_5 AS d5, live_data.sort AS sort
FROM live_data, music_data
WHERE live_data.music_data_id = music_data.id
ORDER BY sort;";

        private static readonly string[] AcbMakerCriticalFiles = { "hcaenc.exe", "hcacc.exe", "AcbMaker.exe", "LZ4.exe", "hcaenc_lite.dll" };

    }
}
