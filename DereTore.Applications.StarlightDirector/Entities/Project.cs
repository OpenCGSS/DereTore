using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DereTore.Applications.StarlightDirector.Entities {
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class Project {

        public Project() {
            Version = CurrentVersion;
            Scores = new Dictionary<Difficulty, Score>();
            CompiledScores = new Dictionary<Difficulty, CompiledScore>();
        }

        public static Project CurrentProject { get; set; }

        public Dictionary<Difficulty, Score> Scores { get; private set; }

        [JsonIgnore]
        public Dictionary<Difficulty, CompiledScore> CompiledScores { get; private set; }

        [JsonIgnore]
        public WaveStream Music { get; internal set; }

        public string MusicFileName { get; set; }

        [JsonIgnore]
        public bool IsChanged { get; internal set; } = true;

        [JsonIgnore]
        public bool IsSaved => !string.IsNullOrEmpty(SaveFileName);

        [JsonIgnore]
        public string SaveFileName { get; private set; }

        public string Version { get; set; }

        public Score GetScore(Difficulty difficulty) {
            if (!Scores.ContainsKey(difficulty)) {
                var score = new Score(this, difficulty);
                Scores.Add(difficulty, score);
            }
            return Scores[difficulty];
        }

        public void CompileScore(Difficulty difficulty) {
            if (!Scores.ContainsKey(difficulty)) {
                throw new InvalidOperationException();
            }
            var score = Scores[difficulty];
            CompiledScore compiledScore;
            CompiledScores.TryGetValue(difficulty, out compiledScore);
            if (compiledScore == null) {
                compiledScore = score.Compile();
                CompiledScores.Add(difficulty, compiledScore);
            } else {
                score.CompileTo(compiledScore);
            }
        }

        public void SaveScoreToCsv(Difficulty difficulty, TextWriter writer) {
            if (!CompiledScores.ContainsKey(difficulty)) {
                CompileScore(difficulty);
            }
            var compiledScore = CompiledScores[difficulty];
            throw new NotImplementedException();
        }

        public void Save() {
            if (string.IsNullOrEmpty(SaveFileName)) {
                throw new InvalidOperationException("This function can only be used when the project is loaded from a file.");
            }
            Save(SaveFileName);
        }

        public void Save(string fileName) {
            using (var fileStream = File.Open(fileName, FileMode.Create, FileAccess.Write)) {
                using (var writer = new StreamWriter(fileStream)) {
                    Save(writer, fileName);
                }
            }
        }

        public void Save(TextWriter writer, string fileName) {
            writer.Write($"// DereTore Composer Project, version {Version}\n");
            var serializer = JsonSerializer.Create();
            serializer.Serialize(writer, this);
            if (string.IsNullOrEmpty(SaveFileName)) {
                var fileInfo = new FileInfo(fileName);
                SaveFileName = fileInfo.FullName;
            }
            IsChanged = false;
        }

        public static Project Load(string fileName) {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read)) {
                using (var reader = new StreamReader(fileStream)) {
                    return Load(reader, fileName);
                }
            }
        }

        public static Project Load(TextReader reader, string fileName) {
            reader.ReadLine();
            var fileInfo = new FileInfo(fileName);
            var serializer = JsonSerializer.Create();
            using (var jsonReader = new JsonTextReader(reader)) {
                var project = serializer.Deserialize<Project>(jsonReader);
                project.SaveFileName = fileInfo.FullName;
                project.IsChanged = false;
                if (project.Scores != null) {
                    foreach (var kv in project.Scores) {
                        var score = kv.Value;
                        score.ResolveReferences();
                        score.Difficulty = kv.Key;
                        score.Project = project;
                    }
                }
                return project;
            }
        }

        private static readonly string CurrentVersion = "0.1";

    }
}
