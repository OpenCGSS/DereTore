using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Text;
using DereTore.Applications.StarlightDirector.Entities;
using Newtonsoft.Json;

namespace DereTore.Applications.StarlightDirector.Exchange {
    public static partial class ProjectIO {

        public static void Save(Project project) {
            Save(project, project.SaveFileName);
        }

        public static void Save(Project project, bool createNewDatabase) {
            Save(project, project.SaveFileName, createNewDatabase);
        }

        public static void Save(Project project, string fileName) {
            var fileInfo = new FileInfo(fileName);
            var newDatabase = !fileInfo.Exists;
            Save(project, fileName, newDatabase);
        }

        public static void Save(Project project, string fileName, bool createNewDatabase) {
            var fileInfo = new FileInfo(fileName);
            fileName = fileInfo.FullName;
            if (createNewDatabase) {
                SQLiteConnection.CreateFile(fileName);
            }
            using (var connection = new SQLiteConnection($"Data Source={fileName}")) {
                connection.Open();
                SQLiteCommand createTable = null, setValue = null;

                using (var transaction = connection.BeginTransaction()) {
                    // Table structure
                    if (createNewDatabase) {
                        SQLiteHelper.CreateTable(transaction, MainTableName, ref createTable);
                        SQLiteHelper.CreateTable(transaction, ScoresTableName, ref createTable);
                        SQLiteHelper.CreateTable(transaction, ScoreSettingsTableName, ref createTable);
                    }

                    // Main
                    SQLiteHelper.SetValue(transaction, MainTableName, "music_file_name", project.MusicFileName ?? string.Empty, createNewDatabase, ref setValue);
                    SQLiteHelper.SetValue(transaction, MainTableName, "vesion", project.Version, createNewDatabase, ref setValue);

                    // Scores
                    var jsonSerializer = JsonSerializer.Create();
                    foreach (var difficulty in Difficulties) {
                        var score = project.GetScore(difficulty);
                        // Damn JsonSerializer
                        var tempFileName = Path.GetTempFileName();
                        using (var fileStream = File.Open(tempFileName, FileMode.Open, FileAccess.Write)) {
                            using (var writer = new StreamWriter(fileStream, Encoding.ASCII)) {
                                jsonSerializer.Serialize(writer, score);
                            }
                        }
                        var s = File.ReadAllText(tempFileName, Encoding.ASCII);
                        File.Delete(tempFileName);
                        SQLiteHelper.SetValue(transaction, ScoresTableName, ((int)difficulty).ToString("00"), s, createNewDatabase, ref setValue);
                    }

                    // Score settings
                    var settings = project.Settings;
                    SQLiteHelper.SetValue(transaction, ScoreSettingsTableName, "global_bpm", settings.GlobalBpm.ToString(CultureInfo.InvariantCulture), createNewDatabase, ref setValue);
                    SQLiteHelper.SetValue(transaction, ScoreSettingsTableName, "start_time_offset", settings.StartTimeOffset.ToString(CultureInfo.InvariantCulture), createNewDatabase, ref setValue);
                    SQLiteHelper.SetValue(transaction, ScoreSettingsTableName, "global_grid_per_signature", settings.GlobalGridPerSignature.ToString(), createNewDatabase, ref setValue);
                    SQLiteHelper.SetValue(transaction, ScoreSettingsTableName, "global_signature", settings.GlobalSignature.ToString(), createNewDatabase, ref setValue);

                    // Commit!
                    transaction.Commit();
                }

                // Cleanup
                setValue.Dispose();
                createTable?.Dispose();
                connection.Close();
            }
            project.SaveFileName = fileName;
            project.IsChanged = false;
        }

        public static Project Load(string fileName) {
            var fileInfo = new FileInfo(fileName);
            if (!fileInfo.Exists) {
                throw new FileNotFoundException(string.Empty, fileName);
            }
            fileName = fileInfo.FullName;
            var project = new Project {
                IsChanged = false,
                SaveFileName = fileName
            };
            using (var connection = new SQLiteConnection($"Data Source={fileName}")) {
                connection.Open();
                SQLiteCommand getValues = null;

                // Main
                var mainValues = SQLiteHelper.GetValues(connection, MainTableName, ref getValues);
                project.MusicFileName = mainValues["music_file_name"];
                project.Version = mainValues["version"];

                // Scores
                var scoreValues = SQLiteHelper.GetValues(connection, ScoresTableName, ref getValues);
                var jsonSerializer = JsonSerializer.CreateDefault();
                foreach (var difficulty in Difficulties) {
                    var indexString = ((int)difficulty).ToString("00");
                    var scoreJson = scoreValues[indexString];
                    Score score;
                    var scoreJsonBytes = Encoding.ASCII.GetBytes(scoreJson);
                    using (var memoryStream = new MemoryStream(scoreJsonBytes)) {
                        using (var reader = new StreamReader(memoryStream)) {
                            using (var jsonReader = new JsonTextReader(reader)) {
                                score = jsonSerializer.Deserialize<Score>(jsonReader);
                            }
                        }
                    }
                    score.ResolveReferences(project);
                    score.Difficulty = difficulty;
                    project.Scores.Add(difficulty, score);
                }

                // Score settings
                var scoreSettingsValues = SQLiteHelper.GetValues(connection, ScoreSettingsTableName, ref getValues);
                var settings = project.Settings;
                settings.GlobalBpm = double.Parse(scoreSettingsValues["global_bpm"]);
                settings.StartTimeOffset = double.Parse(scoreSettingsValues["start_time_offset"]);
                settings.GlobalGridPerSignature = int.Parse(scoreSettingsValues["global_grid_per_signature"]);
                settings.GlobalSignature = int.Parse(scoreSettingsValues["global_signature"]);

                // Cleanup
                getValues.Dispose();
                connection.Close();
            }

            GridLineFixup(project);
            return project;
        }

        private static void GridLineFixup(Project project) {
            // Signature fix-up
            var newGrids = ScoreSettings.DefaultGlobalGridPerSignature * ScoreSettings.DefaultGlobalSignature;
            var oldGrids = project.Settings.GlobalGridPerSignature * project.Settings.GlobalSignature;
            if (newGrids != oldGrids && newGrids % oldGrids == 0) {
                project.Settings.GlobalGridPerSignature = ScoreSettings.DefaultGlobalGridPerSignature;
                project.Settings.GlobalSignature = ScoreSettings.DefaultGlobalSignature;
                var k = newGrids / oldGrids;
                foreach (var difficulty in Difficulties) {
                    if (project.Scores.ContainsKey(difficulty)) {
                        var score = project.GetScore(difficulty);
                        foreach (var note in score.Notes) {
                            note.IndexInGrid *= k;
                        }
                    }
                }
            }
        }

        private static readonly string MainTableName = "main";
        private static readonly string ScoresTableName = "scores";
        private static readonly string ScoreSettingsTableName = "score_settings";

        private static readonly Difficulty[] Difficulties = { Difficulty.Debut, Difficulty.Regular, Difficulty.Pro, Difficulty.Master, Difficulty.MasterPlus };

    }
}
