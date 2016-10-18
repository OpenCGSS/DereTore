using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Entities.Gaming;
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
            if (_musicListInitialized) {
                return;
            }
            FillDifficultyMappingComboBoxes();
            FillMusicComboBoxes();
            CheckAcbBuildingEnvironment();
            AcbBuildLogScroller.ScrollToEnd();
            _musicListInitialized = true;
        }

        private void FillDifficultyMappingComboBoxes() {
            MappingDebut = Difficulty.Debut;
            MappingRegular = Difficulty.Regular;
            MappingPro = Difficulty.Pro;
            MappingMaster = Difficulty.Master;
            MappingMasterPlus = Difficulty.MasterPlus;
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
                                var color = (int)(long)dataRow["live_type"];
                                if (color > 0) {
                                    record.Attribute |= (MusicAttribute)(1 << (color - 1));
                                }
                                var isEvent = (long)dataRow["event_type"] > 0;
                                if (isEvent) {
                                    record.Attribute |= MusicAttribute.Event;
                                }
                                // お願い！シンデレラ (solo ver.)
                                if (record.MusicID == 1901) {
                                    record.Attribute |= MusicAttribute.Solo;
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
                var extraDescription = DescribedEnumReader.Read(record.Attribute, typeof(MusicAttribute));
                var textBlock = new TextBlock(new Run(record.MusicName));
                if (!string.IsNullOrEmpty(extraDescription)) {
                    var inline = new Run(" - " + extraDescription);
                    inline.Foreground = SystemColors.GrayTextBrush;
                    textBlock.Inlines.Add(inline);
                }
                var cbItem = new ComboBoxItem {
                    Content = textBlock
                };
                item.Content = cbItem;
                item.Tag = record;
                CboSongList.Items.Add(item);
            }
        }

        private void CheckAcbBuildingEnvironment() {
            var missingFiles = AcbMakerCriticalFiles.Where(s => !File.Exists(s)).ToArray();
            if (missingFiles.Length > 0) {
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

        public static readonly Difficulty[] Difficulties = { Difficulty.Debut, Difficulty.Regular, Difficulty.Pro, Difficulty.Master, Difficulty.MasterPlus };

        private static readonly string MasterMdbPath = "Resources/GameData/master.mdb";

        private static readonly string FormatFilter = @"
SELECT live_data.id AS live_id, music_data.id AS music_id, music_data.name AS music_name,
    difficulty_1 AS d1, difficulty_2 AS d2, difficulty_3 AS d3, difficulty_4 AS d4, difficulty_5 AS d5,
    live_data.sort AS sort, live_data.type as live_type, live_data.event_type as event_type
FROM live_data, music_data
WHERE live_data.music_data_id = music_data.id
ORDER BY sort;";

        private static readonly string[] AcbMakerCriticalFiles = { "hcaenc.exe", "hcacc.exe", "AcbMaker.exe", "LZ4.exe", "hcaenc_lite.dll" };

    }
}
