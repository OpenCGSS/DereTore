using System;
using System.Data;
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
            } else {
                File.Delete(fileName);
            }
            using (var connection = new SQLiteConnection($"Data Source={fileName}")) {
                connection.Open();
                SQLiteCommand createTable = null, setValue = null, insertNote = null, insertNoteID = null;

                using (var transaction = connection.BeginTransaction()) {
                    // Table structure
                    SQLiteHelper.CreateKeyValueTable(transaction, Names.Table_Main, ref createTable);
                    SQLiteHelper.CreateScoresTables(transaction, ref createTable);
                    SQLiteHelper.CreateKeyValueTable(transaction, Names.Table_ScoreSettings, ref createTable);
                    SQLiteHelper.CreateKeyValueTable(transaction, Names.Table_Metadata, ref createTable);

                    // Main
                    SQLiteHelper.InsertValue(transaction, Names.Table_Main, Names.Field_MusicFileName, project.MusicFileName ?? string.Empty, ref setValue);
                    SQLiteHelper.InsertValue(transaction, Names.Table_Main, Names.Field_Version, project.Version, ref setValue);

                    // Notes
                    SQLiteHelper.InsertNoteID(transaction, EntityID.Invalid, ref insertNoteID);
                    foreach (var difficulty in Difficulties) {
                        var score = project.GetScore(difficulty);
                        foreach (var note in score.Notes) {
                            SQLiteHelper.InsertNoteID(transaction, note.ID, ref insertNoteID);
                        }
                    }
                    foreach (var difficulty in Difficulties) {
                        var score = project.GetScore(difficulty);
                        foreach (var note in score.Notes) {
                            SQLiteHelper.InsertNote(transaction, note, ref insertNote);
                        }
                    }

                    // Score settings
                    var settings = project.Settings;
                    SQLiteHelper.InsertValue(transaction, Names.Table_ScoreSettings, Names.Field_GlobalBpm, settings.GlobalBpm.ToString(CultureInfo.InvariantCulture), ref setValue);
                    SQLiteHelper.InsertValue(transaction, Names.Table_ScoreSettings, Names.Field_StartTimeOffset, settings.StartTimeOffset.ToString(CultureInfo.InvariantCulture), ref setValue);
                    SQLiteHelper.InsertValue(transaction, Names.Table_ScoreSettings, Names.Field_GlobalGridPerSignature, settings.GlobalGridPerSignature.ToString(), ref setValue);
                    SQLiteHelper.InsertValue(transaction, Names.Table_ScoreSettings, Names.Field_GlobalSignature, settings.GlobalSignature.ToString(), ref setValue);

                    // Metadata (none for now)

                    // Commit!
                    transaction.Commit();
                }

                // Cleanup
                insertNoteID?.Dispose();
                insertNote?.Dispose();
                setValue?.Dispose();
                createTable?.Dispose();
                connection.Close();
            }
            project.SaveFileName = fileName;
            project.IsChanged = false;
        }

        public static Project Load(string fileName) {
            var version = CheckProjectFileVersion(fileName);
            return Load(fileName, version);
        }


        internal static Project Load(string fileName, ProjectVersion versionOverride) {
            switch (versionOverride) {
                case ProjectVersion.Unknown:
                    throw new ArgumentOutOfRangeException(nameof(versionOverride));
                case ProjectVersion.V0_1:
                    return LoadFromV01(fileName);
                case ProjectVersion.V0_2:
                    return LoadFromV02(fileName);
            }
            return LoadCurrentVersion(fileName);
        }

        private static Project LoadCurrentVersion(string fileName) {
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
                project.Version = mainValues[Names.Field_Version];

                // Scores
                foreach (var difficulty in Difficulties) {
                    var score = new Score(project, difficulty);
                    ReadScore(connection, score);
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

                // Metadata (none for now)

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

        private static void ReadScore(SQLiteConnection connection, Score score) {
            using (var table = new DataTable()) {
                SQLiteHelper.ReadNotesTable(connection, score.Difficulty, table);

                foreach (DataRow row in table.Rows) {
                    var id = (int)(long)row[Names.Column_ID];
                    var bar = (int)(long)row[Names.Column_BarIndex];
                    var grid = (int)(long)row[Names.Column_IndexInGrid];
                    var start = (NotePosition)(long)row[Names.Column_StartPosition];
                    var finish = (NotePosition)(long)row[Names.Column_FinishPosition];
                    var flick = (NoteFlickType)(long)row[Names.Column_FlickType];
                    var prevFlick = (int)(long)row[Names.Column_PrevFlickNoteID];
                    var nextFlick = (int)(long)row[Names.Column_NextFlickNoteID];
                    var sync = (int)(long)row[Names.Column_SyncTargetID];
                    var hold = (int)(long)row[Names.Column_HoldTargetID];

                    EnsureBarIndex(score, bar);
                    var b = score.Bars[bar];
                    var note = b.AddNote(id);
                    note.IndexInGrid = grid;
                    note.StartPosition = start;
                    note.FinishPosition = finish;
                    note.FlickType = flick;
                    note.PrevFlickNoteID = prevFlick;
                    note.NextFlickNoteID = nextFlick;
                    note.SyncTargetID = sync;
                    note.HoldTargetID = hold;
                }
            }
        }

        private static void EnsureBarIndex(Score score, int index) {
            if (score.Bars.Count > index) {
                return;
            }
            for (var i = score.Bars.Count; i <= index; ++i) {
                var bar = new Bar(score, i);
                score.Bars.Add(bar);
            }
        }

        private static readonly Difficulty[] Difficulties = { Difficulty.Debut, Difficulty.Regular, Difficulty.Pro, Difficulty.Master, Difficulty.MasterPlus };

    }
}
