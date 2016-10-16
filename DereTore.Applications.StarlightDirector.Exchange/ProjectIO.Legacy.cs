using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using DereTore.Applications.StarlightDirector.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DereTore.Applications.StarlightDirector.Exchange {
    partial class ProjectIO {

        internal static ProjectVersion CheckProjectFileVersion(string fileName) {
            try {
                using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read)) {
                    var buffer = new byte[128];
                    fileStream.Read(buffer, 0, buffer.Length);
                    var separatorIndex = buffer.IndexOf((byte)'\n');
                    if (separatorIndex >= 0) {
                        if (separatorIndex > 0 && buffer[separatorIndex - 1] == '\r') {
                            --separatorIndex;
                        }
                        var stringBuffer = buffer.Take(separatorIndex).ToArray();
                        var versionString = Encoding.ASCII.GetString(stringBuffer);
                        const string standardVersionString = "// DereTore Composer Project, version 0.1";
                        if (versionString == standardVersionString) {
                            return ProjectVersion.V0_1;
                        }
                    }
                }
            } catch (Exception) {
            }
            try {
                string versionString = null;
                using (var connection = new SQLiteConnection($"Data Source={fileName}")) {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT value FROM {Names.Table_Main} WHERE key = @key;";
                    command.Parameters.Add("key", DbType.AnsiString).Value = Names.Field_Version;
                    var value = command.ExecuteScalar();
                    if (value != DBNull.Value) {
                        versionString = (string)value;
                    }
                    if (versionString == null) {
                        command.Parameters["key"].Value = Names.Field_Vesion;
                        value = command.ExecuteScalar();
                        if (value != DBNull.Value) {
                            versionString = (string)value;
                        }
                    }
                    if (versionString == null) {
                        command.Dispose();
                        command = connection.CreateCommand();
                        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' AND name = 'scores';";
                        value = command.ExecuteScalar();
                        if (value != DBNull.Value) {
                            // This is a bug from v0.3.x (maybe also v0.4.x), which occasionally leaves out the version ('vesion') field in 'main' table.
                            versionString = "0.2";
                        }
                    }
                    command.Dispose();
                    connection.Close();
                }
                if (string.IsNullOrEmpty(versionString)) {
                    return ProjectVersion.Unknown;
                }
                double v;
                double.TryParse(versionString, out v);
                if (v.Equals(0.2)) {
                    return ProjectVersion.V0_2;
                } else if (v.Equals(0.3)) {
                    return ProjectVersion.V0_3;
                } else {
                    return ProjectVersion.Unknown;
                }
            } catch (Exception ex) {
                return ProjectVersion.Unknown;
            }
        }

        private static Project LoadFromV01(string fileInput) {
            Project project;
            using (var fileStream = File.Open(fileInput, FileMode.Open, FileAccess.Read)) {
                using (var reader = new StreamReader(fileStream, Encoding.ASCII)) {
                    reader.ReadLine();
                    JObject p;
                    var jsonSerializer = JsonSerializer.Create();
                    using (var jsonReader = new JsonTextReader(reader)) {
                        p = (JObject)jsonSerializer.Deserialize(jsonReader);
                    }

                    project = new Project();
                    project.MusicFileName = p.Property("musicFileName").Value.Value<string>();
                    var scores = p.Property("scores").Value.ToObject<Dictionary<Difficulty, Score>>();
                    foreach (var kv in scores) {
                        project.Scores.Add(kv.Key, kv.Value);
                    }

                    // Score settings
                    var rawScores = p.Property("scores").Values();
                    ScoreSettings settings = null;
                    foreach (var token in rawScores) {
                        var rawScore = (JObject)((JProperty)token).Value;
                        var settingsProp = rawScore.Property("settings");
                        settings = settingsProp.Value.ToObject<ScoreSettings>();
                        if (settings != null) {
                            break;
                        }
                    }
                    if (settings != null) {
                        project.Settings.GlobalBpm = settings.GlobalBpm;
                        project.Settings.GlobalGridPerSignature = settings.GlobalGridPerSignature;
                        project.Settings.GlobalSignature = settings.GlobalSignature;
                        project.Settings.StartTimeOffset = settings.StartTimeOffset;
                    }
                }
            }

            foreach (var difficulty in Difficulties) {
                var score = project.GetScore(difficulty);
                score.ResolveReferences(project);
                score.Difficulty = difficulty;
            }

            project.SaveFileName = fileInput;

            GridLineFixup(project);
            return project;
        }

        private static Project LoadFromV02(string fileName) {
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
                var mainValues = SQLiteHelper.GetValues(connection, Names.Table_Main, ref getValues);
                project.MusicFileName = mainValues[Names.Field_MusicFileName];
                //project.Version = mainValues[Names.Field_Version];

                // Scores
                var scoreValues = SQLiteHelper.GetValues(connection, Names.Table_Scores, ref getValues);
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
                var scoreSettingsValues = SQLiteHelper.GetValues(connection, Names.Table_ScoreSettings, ref getValues);
                var settings = project.Settings;
                settings.GlobalBpm = double.Parse(scoreSettingsValues[Names.Field_GlobalBpm]);
                settings.StartTimeOffset = double.Parse(scoreSettingsValues[Names.Field_StartTimeOffset]);
                settings.GlobalGridPerSignature = int.Parse(scoreSettingsValues[Names.Field_GlobalGridPerSignature]);
                settings.GlobalSignature = int.Parse(scoreSettingsValues[Names.Field_GlobalSignature]);

                // Cleanup
                getValues.Dispose();
                connection.Close();
            }

            GridLineFixup(project);
            return project;
        }

    }
}
