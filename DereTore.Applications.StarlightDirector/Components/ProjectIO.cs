using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Text;
using DereTore.Applications.StarlightDirector.Entities;
using Newtonsoft.Json;

namespace DereTore.Applications.StarlightDirector.Components {
    public static class ProjectIO {

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
                        CreateTable(transaction, MainTableName, ref createTable);
                        CreateTable(transaction, ScoresTableName, ref createTable);
                        CreateTable(transaction, ScoreSettingsTableName, ref createTable);
                    }

                    // Main
                    transaction.SetValue(MainTableName, "music_file_name", project.MusicFileName ?? string.Empty, createNewDatabase, ref setValue);
                    transaction.SetValue(MainTableName, "vesion", project.Version, createNewDatabase, ref setValue);

                    // Scores
                    var jsonSerializer = JsonSerializer.Create();
                    foreach (var difficulty in Difficulties) {
                        var score = project.GetScore(difficulty);
                        // Damn JsonSerializer
                        var tempFileName = Path.GetTempFileName();
                        using (var fileStream = File.Open(tempFileName, FileMode.Open, FileAccess.Write)) {
                            using (var writer = new StreamWriter(fileStream, Encoding.UTF8)) {
                                jsonSerializer.Serialize(writer, score);
                            }
                        }
                        var s = File.ReadAllText(tempFileName, Encoding.UTF8);
                        File.Delete(tempFileName);
                        transaction.SetValue(ScoresTableName, ((int)difficulty).ToString("00"), s, createNewDatabase, ref setValue);
                    }

                    // Score settings
                    var settings = project.Settings;
                    transaction.SetValue(ScoreSettingsTableName, "global_bpm", settings.GlobalBpm.ToString(CultureInfo.InvariantCulture), createNewDatabase, ref setValue);
                    transaction.SetValue(ScoreSettingsTableName, "start_time_offset", settings.StartTimeOffset.ToString(CultureInfo.InvariantCulture), createNewDatabase, ref setValue);
                    transaction.SetValue(ScoreSettingsTableName, "global_grid_per_signature", settings.GlobalSignature.ToString(), createNewDatabase, ref setValue);
                    transaction.SetValue(ScoreSettingsTableName, "global_signature", settings.GlobalSignature.ToString(), createNewDatabase, ref setValue);

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
                var mainValues = GetValues(connection, MainTableName, ref getValues);
                project.MusicFileName = mainValues["music_file_name"];
                project.Version = mainValues["version"];

                // Scores
                var scoreValues = GetValues(connection, ScoresTableName, ref getValues);
                var jsonSerializer = JsonSerializer.CreateDefault();
                foreach (var difficulty in Difficulties) {
                    var indexString = ((int)difficulty).ToString("00");
                    var scoreJson = scoreValues[indexString];
                    Score score;
                    var scoreJsonBytes = Encoding.UTF8.GetBytes(scoreJson);
                    using (var memoryStream = new MemoryStream(scoreJsonBytes)) {
                        using (var reader = new StreamReader(memoryStream)) {
                            using (var jsonReader = new JsonTextReader(reader)) {
                                score = jsonSerializer.Deserialize<Score>(jsonReader);
                            }
                        }
                    }
                    score.ResolveReferences(project);
                    score.Difficulty = difficulty;
                    score.Project = project;
                    project.Scores.Add(difficulty, score);
                }

                // Score settings
                var scoreSettingsValues = GetValues(connection, ScoreSettingsTableName, ref getValues);
                var settings = project.Settings;
                settings.GlobalBpm = double.Parse(scoreSettingsValues["global_bpm"]);
                settings.StartTimeOffset = double.Parse(scoreSettingsValues["start_time_offset"]);
                settings.GlobalGridPerSignature = int.Parse(scoreSettingsValues["global_grid_per_signature"]);
                settings.GlobalSignature = int.Parse(scoreSettingsValues["global_signature"]);

                // Cleanup
                getValues.Dispose();
                connection.Close();
            }

            return project;
        }

        private static void SetValue(this SQLiteTransaction transaction, string tableName, string key, string value, bool creatingNewDatabase, ref SQLiteCommand command) {
            SetValue(transaction.Connection, tableName, key, value, creatingNewDatabase, ref command);
        }

        private static void SetValue(this SQLiteConnection connection, string tableName, string key, string value, bool creatingNewDatabase, ref SQLiteCommand command) {
            if (creatingNewDatabase) {
                InsertValue(connection, tableName, key, value, ref command);
            } else {
                UpdateValue(connection, tableName, key, value, ref command);
            }
        }

        private static void InsertValue(this SQLiteTransaction transaction, string tableName, string key, string value, ref SQLiteCommand command) {
            InsertValue(transaction.Connection, tableName, key, value, ref command);
        }

        private static void InsertValue(this SQLiteConnection connection, string tableName, string key, string value, ref SQLiteCommand command) {
            if (command == null) {
                command = connection.CreateCommand();
                command.CommandText = $"INSERT INTO {tableName} (key, value) VALUES (@key, @value);";
                command.Parameters.Add("key", DbType.String).Value = key;
                command.Parameters.Add("value", DbType.String).Value = value;
            } else {
                command.CommandText = $"INSERT INTO {tableName} (key, value) VALUES (@key, @value);";
                command.Parameters["key"].Value = key;
                command.Parameters["value"].Value = value;
            }
            command.ExecuteNonQuery();
        }

        private static void UpdateValue(this SQLiteTransaction transaction, string tableName, string key, string value, ref SQLiteCommand command) {
            UpdateValue(transaction.Connection, tableName, key, value, ref command);
        }

        private static void UpdateValue(this SQLiteConnection connection, string tableName, string key, string value, ref SQLiteCommand command) {
            if (command == null) {
                command = connection.CreateCommand();
                command.CommandText = $"UPDATE {tableName} SET value = @value WHERE key = @key;";
                command.Parameters.Add("key", DbType.String).Value = key;
                command.Parameters.Add("value", DbType.String).Value = value;
            } else {
                command.CommandText = $"UPDATE {tableName} SET value = @value WHERE key = @key;";
                command.Parameters["key"].Value = key;
                command.Parameters["value"].Value = value;
            }
            command.ExecuteNonQuery();
        }

        private static string GetValue(this SQLiteTransaction transaction, string tableName, string key, ref SQLiteCommand command) {
            return GetValue(transaction.Connection, tableName, key, ref command);
        }

        private static string GetValue(this SQLiteConnection connection, string tableName, string key, ref SQLiteCommand command) {
            if (command == null) {
                command = connection.CreateCommand();
                command.CommandText = $"SELECT value FROM {tableName} WHERE key = @key;";
                command.Parameters.Add("key", DbType.String).Value = key;
            } else {
                command.CommandText = $"SELECT value FROM {tableName} WHERE key = @key;";
                command.Parameters["key"].Value = key;
            }
            var value = command.ExecuteScalar();
            return (string)value;
        }

        private static StringDictionary GetValues(this SQLiteTransaction transaction, string tableName, ref SQLiteCommand command) {
            return GetValues(transaction.Connection, tableName, ref command);
        }

        private static StringDictionary GetValues(this SQLiteConnection connection, string tableName, ref SQLiteCommand command) {
            if (command == null) {
                command = connection.CreateCommand();
            }
            command.CommandText = $"SELECT key, value FROM {tableName};";
            var result = new StringDictionary();
            using (var reader = command.ExecuteReader()) {
                while (reader.Read()) {
                    var row = reader.GetValues();
                    result.Add(row[0], row[1]);
                }
            }
            return result;
        }

        private static void CreateTable(this SQLiteTransaction transaction, string tableName, ref SQLiteCommand command) {
            CreateTable(transaction.Connection, tableName, ref command);
        }

        private static void CreateTable(SQLiteConnection connection, string tableName, ref SQLiteCommand command) {
            if (command == null) {
                command = connection.CreateCommand();
            }
            command.CommandText = $"CREATE TABLE {tableName} (key TEXT, value TEXT);";
            command.ExecuteNonQuery();
        }

        private static readonly string MainTableName = "main";
        private static readonly string ScoresTableName = "scores";
        private static readonly string ScoreSettingsTableName = "score_settings";

        private static readonly Difficulty[] Difficulties = { Difficulty.Debut, Difficulty.Regular, Difficulty.Pro, Difficulty.Master, Difficulty.MasterPlus };

    }
}
