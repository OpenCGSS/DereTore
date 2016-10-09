using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using DereTore.Applications.StarlightDirector.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DereTore.Applications.StarlightDirector.Conversion {
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
                using (var connection = new SQLiteConnection($"Data Source={fileName}")) {
                    connection.Open();
                    connection.Close();
                }
                return ProjectVersion.V0_2;
            } catch (Exception) {
            }
            return ProjectVersion.Unknown;
        }

        internal static Project LoadFromV01(string fileInput) {
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

            // Signature fix-up
            var newGrids = ScoreSettings.DefaultGlobalGridPerSignature * ScoreSettings.DefaultGlobalSignature;
            var oldGrids = project.Settings.GlobalGridPerSignature * project.Settings.GlobalSignature;
            if (newGrids % oldGrids == 0) {
                project.Settings.GlobalGridPerSignature = ScoreSettings.DefaultGlobalGridPerSignature;
                project.Settings.GlobalSignature = ScoreSettings.DefaultGlobalSignature;
                var k = newGrids / oldGrids;
                foreach (var difficulty in Difficulties) {
                    if (project.Scores.ContainsKey(difficulty)) {
                        var score = project.GetScore(difficulty);
                        foreach (var bar in score.Bars) {
                            foreach (var note in bar.Notes) {
                                note.PositionInGrid *= k;
                            }
                        }
                    }
                }
            }

            return project;
        }

    }
}
