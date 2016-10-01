using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DereTore.Applications.StarlightDirector.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DereTore.Applications.StarlightDirector.Components {
    partial class ProjectIO {

        internal static Project LoadFromV01(string fileInput) {
            Project project;
            using (var fileStream = File.Open(fileInput, FileMode.Open, FileAccess.Read)) {
                using (var reader = new StreamReader(fileStream, Encoding.UTF8)) {
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
            return project;
        }

    }
}
