using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DereTore.Applications.StarlightDirector.Entities {
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), MemberSerialization = MemberSerialization.OptIn)]
    public sealed class Project : DependencyObject {

        public Project() {
            Version = CurrentVersion;
            Scores = new Dictionary<Difficulty, Score>();
            Difficulty = Difficulty.Debut;
        }

        public event EventHandler<EventArgs> DifficultyChanged;

        public static Project Current { get; set; }

        [JsonProperty]
        public Dictionary<Difficulty, Score> Scores { get; }

        [JsonProperty]
        public string MusicFileName { get; set; }

        public bool HasMusic => !string.IsNullOrEmpty(MusicFileName) && File.Exists(MusicFileName);

        public bool IsChanged { get; internal set; }

        public bool IsSaved => !string.IsNullOrEmpty(SaveFileName) && File.Exists(SaveFileName);

        public string SaveFileName { get; private set; }

        [JsonProperty]
        public string Version { get; private set; }

        public Score GetScore(Difficulty difficulty) {
            if (!Scores.ContainsKey(difficulty)) {
                var score = new Score(this, difficulty);
                Scores.Add(difficulty, score);
            }
            return Scores[difficulty];
        }

        public void SaveScoreToCsv(Difficulty difficulty, string fileName) {
            using (var stream = File.Open(fileName, FileMode.Create, FileAccess.Write)) {
                using (var writer = new StreamWriter(stream)) {
                    SaveScoreToCsv(difficulty, writer);
                }
            }
        }

        public void SaveScoreToCsv(Difficulty difficulty, TextWriter writer) {
            var score = GetScore(difficulty);
            var compiledScore = score.Compile();
            var csvString = compiledScore.GetCsvString();
            writer.Write(csvString);
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
                        score.ResolveReferences(project);
                        score.Difficulty = kv.Key;
                        score.Project = project;
                    }
                }
                return project;
            }
        }

        public Difficulty Difficulty {
            get { return (Difficulty)GetValue(DifficultyProperty); }
            set { SetValue(DifficultyProperty, value); }
        }

        public static readonly DependencyProperty DifficultyProperty = DependencyProperty.Register(nameof(Difficulty), typeof(Difficulty), typeof(Project),
            new PropertyMetadata(Difficulty.Master, OnDifficultyChanged));

        private static readonly string CurrentVersion = "0.1";

        private static void OnDifficultyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var project = obj as Project;
            Debug.Assert(project != null, "project != null");
            Debug.Print($"New difficulty: {e.NewValue}");
            project.DifficultyChanged.Raise(project, EventArgs.Empty);
        }

    }
}
