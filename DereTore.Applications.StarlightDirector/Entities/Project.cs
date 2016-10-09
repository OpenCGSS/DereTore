using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Newtonsoft.Json;

namespace DereTore.Applications.StarlightDirector.Entities {
    public sealed class Project : DependencyObject {

        public Project() {
            Version = CurrentVersion;
            Scores = new Dictionary<Difficulty, Score>();
            Difficulty = Difficulty.Debut;
            Settings = ScoreSettings.CreateDefault();
            Settings.SettingChanged += OnSettingsChanged;
        }

        ~Project() {
            Settings.SettingChanged -= OnSettingsChanged;
        }

        public event EventHandler<EventArgs> DifficultyChanged;
        public event EventHandler<EventArgs> GlobalSettingsChanged;

        public static Project Current { get; set; }

        [JsonProperty]
        public Dictionary<Difficulty, Score> Scores { get; }

        public ScoreSettings Settings { get; private set; }

        [JsonProperty]
        public string MusicFileName { get; set; }

        public bool HasMusic => !string.IsNullOrEmpty(MusicFileName) && File.Exists(MusicFileName);

        public bool IsChanged { get; internal set; }

        public bool IsSaved => !string.IsNullOrEmpty(SaveFileName) && File.Exists(SaveFileName);

        public string SaveFileName { get; internal set; }

        [JsonProperty]
        public string Version { get; internal set; }

        public Score GetScore(Difficulty difficulty) {
            if (!Scores.ContainsKey(difficulty)) {
                var score = new Score(this, difficulty);
                Scores.Add(difficulty, score);
            }
            return Scores[difficulty];
        }

        public void SetScore(Difficulty difficulty, Score score) {
            Scores[difficulty] = score;
        }

        public void ExportScoreToCsv(Difficulty difficulty, string fileName) {
            using (var stream = File.Open(fileName, FileMode.Create, FileAccess.Write)) {
                using (var writer = new StreamWriter(stream)) {
                    ExportScoreToCsv(difficulty, writer);
                }
            }
        }

        public void ExportScoreToCsv(Difficulty difficulty, TextWriter writer) {
            var score = GetScore(difficulty);
            var compiledScore = score.Compile();
            var csvString = compiledScore.GetCsvString();
            writer.Write(csvString);
        }

        public string ExportScoreToCsv(Difficulty difficulty) {
            var score = GetScore(difficulty);
            var compiledScore = score.Compile();
            var csvString = compiledScore.GetCsvString();
            return csvString;
        }

        public Difficulty Difficulty {
            get { return (Difficulty)GetValue(DifficultyProperty); }
            set { SetValue(DifficultyProperty, value); }
        }

        public static readonly DependencyProperty DifficultyProperty = DependencyProperty.Register(nameof(Difficulty), typeof(Difficulty), typeof(Project),
            new PropertyMetadata(Difficulty.Master, OnDifficultyChanged));

        public static string CurrentVersion => "0.2";

        private static void OnDifficultyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var project = obj as Project;
            Debug.Assert(project != null, "project != null");
            Debug.Print($"New difficulty: {e.NewValue}");
            project.DifficultyChanged.Raise(project, EventArgs.Empty);
        }

        private void OnSettingsChanged(object sender, EventArgs e) {
            IsChanged = true;
            GlobalSettingsChanged.Raise(sender, e);
        }

    }
}
