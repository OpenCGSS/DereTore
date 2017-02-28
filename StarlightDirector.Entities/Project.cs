using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using DereTore;
using Newtonsoft.Json;
using StarlightDirector.Entities.Extensions;

namespace StarlightDirector.Entities {
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

        public ScoreSettings Settings { get; }

        public HashSet<int> ExistingIDs { get; } = new HashSet<int>();

        [JsonProperty]
        public string MusicFileName {
            get { return (string)GetValue(MusicFileNameProperty); }
            set { SetValue(MusicFileNameProperty, value); }
        }

        public bool HasMusic {
            get { return (bool)GetValue(HasMusicProperty); }
            private set { SetValue(HasMusicProperty, value); }
        }

        public bool IsChanged { get; internal set; }

        public bool IsSaved {
            get { return (bool)GetValue(IsSavedProperty); }
            private set { SetValue(IsSavedProperty, value); }
        }

        public string SaveFileName {
            get { return (string)GetValue(SaveFileNameProperty); }
            internal set { SetValue(SaveFileNameProperty, value); }
        }

        [JsonProperty]
        public string Version {
            get { return (string)GetValue(VersionProperty); }
            internal set { SetValue(VersionProperty, value); }
        }

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

        public static readonly DependencyProperty MusicFileNameProperty = DependencyProperty.Register(nameof(MusicFileName), typeof(string), typeof(Project),
            new PropertyMetadata(null, OnMusicFileNameChanged));

        public static readonly DependencyProperty HasMusicProperty = DependencyProperty.Register(nameof(HasMusic), typeof(bool), typeof(Project),
           new PropertyMetadata(false));

        public static readonly DependencyProperty SaveFileNameProperty = DependencyProperty.Register(nameof(SaveFileName), typeof(string), typeof(Project),
            new PropertyMetadata(null, OnSaveFileNameChanged));

        public static readonly DependencyProperty IsSavedProperty = DependencyProperty.Register(nameof(IsSaved), typeof(bool), typeof(Project),
           new PropertyMetadata(false));

        public static readonly DependencyProperty VersionProperty = DependencyProperty.Register(nameof(Version), typeof(string), typeof(Project),
            new PropertyMetadata(CurrentVersion));

        public static string CurrentVersion => ProjectVersion.Current.ToString();

        private static void OnDifficultyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var project = obj as Project;
            Debug.Assert(project != null, "project != null");
            Debug.Print($"New difficulty: {e.NewValue}");
            project.DifficultyChanged.Raise(project, EventArgs.Empty);
        }

        private static void OnMusicFileNameChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var project = obj as Project;
            Debug.Assert(project != null, "project != null");
            var newValue = (string)e.NewValue;
            project.HasMusic = !string.IsNullOrEmpty(newValue) && File.Exists(newValue);
            project.OnSettingsChanged(project, EventArgs.Empty);
        }

        private static void OnSaveFileNameChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var project = obj as Project;
            Debug.Assert(project != null, "project != null");
            var newValue = (string)e.NewValue;
            project.IsSaved = !string.IsNullOrEmpty(newValue) && File.Exists(newValue);
        }

        private void OnSettingsChanged(object sender, EventArgs e) {
            IsChanged = true;

            foreach (var score in Scores.Values) {
                score.UpdateBarTimings();
            }

            GlobalSettingsChanged.Raise(sender, e);
        }

    }
}
