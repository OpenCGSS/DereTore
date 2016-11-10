using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector.Exchange {
    public static partial class ProjectIO {

        public static void Save(Project project) {
            Save(project, project.SaveFileName);
        }

        public static void Save(Project project, string fileName) {
            var fileInfo = new FileInfo(fileName);
            var newDatabase = !fileInfo.Exists;
            Save(project, fileName, newDatabase, false);
        }

        public static void SaveAsBackup(Project project, string fileName) {
            Save(project, fileName, true, true);
        }

        public static Project Load(string fileName) {
            var version = CheckProjectFileVersion(fileName);
            return Load(fileName, version);
        }

        internal static Project Load(string fileName, ProjectVersion versionOverride)
        {
            Project project = null;
            switch (versionOverride) {
                case ProjectVersion.Unknown:
                    throw new ArgumentOutOfRangeException(nameof(versionOverride));
                case ProjectVersion.V0_1:
                    project = LoadFromV01(fileName);
                    break;
                case ProjectVersion.V0_2:
                    project = LoadFromV02(fileName);
                    break;
            }

            if (project == null)
                project = LoadCurrentVersion(fileName);

            // Update bar timings, sort notes
            foreach (var difficulty in Difficulties)
            {
                var score = project.GetScore(difficulty);
                foreach (var bar in score.Bars)
                {
                    bar.UpdateTimings();
                    bar.Notes.Sort(Note.TimingComparison);
                }

                score.Notes.Sort(Note.TimingComparison);
            }

            return project;
        }

        private static void Save(Project project, string fileName, bool createNewDatabase, bool isBackup) {
            var fileInfo = new FileInfo(fileName);
            fileName = fileInfo.FullName;
            if (createNewDatabase) {
                SQLiteConnection.CreateFile(fileName);
            } else {
                File.Delete(fileName);
            }
            using (var connection = new SQLiteConnection($"Data Source={fileName}")) {
                connection.Open();
                SQLiteCommand setValue = null, insertNote = null, insertNoteID = null, insertBarParams = null, insertSpecialNote = null;

                using (var transaction = connection.BeginTransaction()) {
                    // Table structure
                    SQLiteHelper.CreateKeyValueTable(transaction, Names.Table_Main);
                    SQLiteHelper.CreateScoresTable(transaction);
                    SQLiteHelper.CreateKeyValueTable(transaction, Names.Table_ScoreSettings);
                    SQLiteHelper.CreateKeyValueTable(transaction, Names.Table_Metadata);
                    SQLiteHelper.CreateBarParamsTable(transaction);
                    SQLiteHelper.CreateSpecialNotesTable(transaction);

                    // Main
                    SQLiteHelper.InsertValue(transaction, Names.Table_Main, Names.Field_MusicFileName, project.MusicFileName ?? string.Empty, ref setValue);
                    SQLiteHelper.InsertValue(transaction, Names.Table_Main, Names.Field_Version, project.Version, ref setValue);

                    // Notes
                    SQLiteHelper.InsertNoteID(transaction, EntityID.Invalid, ref insertNoteID);
                    foreach (var difficulty in Difficulties) {
                        var score = project.GetScore(difficulty);
                        foreach (var note in score.Notes) {
                            if (note.IsGamingNote) {
                                SQLiteHelper.InsertNoteID(transaction, note.ID, ref insertNoteID);
                            }
                        }
                    }
                    foreach (var difficulty in Difficulties) {
                        var score = project.GetScore(difficulty);
                        foreach (var note in score.Notes) {
                            if (note.IsGamingNote) {
                                SQLiteHelper.InsertNote(transaction, note, ref insertNote);
                            }
                        }
                    }

                    // Score settings
                    var settings = project.Settings;
                    SQLiteHelper.InsertValue(transaction, Names.Table_ScoreSettings, Names.Field_GlobalBpm, settings.GlobalBpm.ToString(CultureInfo.InvariantCulture), ref setValue);
                    SQLiteHelper.InsertValue(transaction, Names.Table_ScoreSettings, Names.Field_StartTimeOffset, settings.StartTimeOffset.ToString(CultureInfo.InvariantCulture), ref setValue);
                    SQLiteHelper.InsertValue(transaction, Names.Table_ScoreSettings, Names.Field_GlobalGridPerSignature, settings.GlobalGridPerSignature.ToString(), ref setValue);
                    SQLiteHelper.InsertValue(transaction, Names.Table_ScoreSettings, Names.Field_GlobalSignature, settings.GlobalSignature.ToString(), ref setValue);

                    // Bar params && Special notes
                    foreach (var difficulty in Difficulties) {
                        var score = project.GetScore(difficulty);
                        foreach (var bar in score.Bars) {
                            if (bar.Params != null) {
                                SQLiteHelper.InsertBarParams(transaction, bar, ref insertBarParams);
                            }
                        }
                        foreach (var note in score.Notes.Where(note => !note.IsGamingNote)) {
                            SQLiteHelper.InsertNoteID(transaction, note.ID, ref insertNoteID);
                            SQLiteHelper.InsertSpecialNote(transaction, note, ref insertSpecialNote);
                        }
                    }

                    // Metadata (none for now)

                    // Commit!
                    transaction.Commit();
                }

                // Cleanup
                insertNoteID?.Dispose();
                insertNote?.Dispose();
                setValue?.Dispose();
                connection.Close();
            }
            if (!isBackup) {
                project.SaveFileName = fileName;
                project.IsChanged = false;
            }
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
                    score.FixSyncNotes();
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

                // Bar params
                if (SQLiteHelper.DoesTableExist(connection, Names.Table_BarParams)) {
                    foreach (var difficulty in Difficulties) {
                        var score = project.GetScore(difficulty);
                        ReadBarParams(connection, score);
                    }
                }

                // Special notes
                if (SQLiteHelper.DoesTableExist(connection, Names.Table_SpecialNotes)) {
                    foreach (var difficulty in Difficulties) {
                        var score = project.GetScore(difficulty);
                        ReadSpecialNotes(connection, score);
                    }
                }

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
            if (newGrids == oldGrids) {
                return;
            }
            project.Settings.GlobalGridPerSignature = ScoreSettings.DefaultGlobalGridPerSignature;
            project.Settings.GlobalSignature = ScoreSettings.DefaultGlobalSignature;
            if (newGrids % oldGrids == 0) {
                // Expanding (e.g. 48 -> 96)
                var k = newGrids / oldGrids;
                foreach (var difficulty in Difficulties) {
                    if (!project.Scores.ContainsKey(difficulty)) {
                        continue;
                    }
                    var score = project.GetScore(difficulty);
                    foreach (var note in score.Notes) {
                        note.IndexInGrid *= k;
                    }
                }
            } else if (oldGrids % newGrids == 0) {
                // Shrinking (e.g. 384 -> 96)
                var k = oldGrids / newGrids;
                var incompatibleNotes = new List<Note>();
                foreach (var difficulty in Difficulties) {
                    if (!project.Scores.ContainsKey(difficulty)) {
                        continue;
                    }
                    var score = project.GetScore(difficulty);
                    foreach (var note in score.Notes) {
                        if (note.IndexInGrid % k != 0) {
                            incompatibleNotes.Add(note);
                        } else {
                            note.IndexInGrid /= k;
                        }
                    }
                }
                if (incompatibleNotes.Count > 0) {
                    Debug.Print("Notes on incompatible grid lines are found. Removing.");
                    foreach (var note in incompatibleNotes) {
                        note.Bar.RemoveNote(note);
                    }
                }
            }
        }

        private static void ReadScore(SQLiteConnection connection, Score score) {
            using (var table = new DataTable()) {
                SQLiteHelper.ReadNotesTable(connection, score.Difficulty, table);
                foreach (DataRow row in table.Rows) {
                    var id = (int)(long)row[Names.Column_ID];
                    var barIndex = (int)(long)row[Names.Column_BarIndex];
                    var grid = (int)(long)row[Names.Column_IndexInGrid];
                    var start = (NotePosition)(long)row[Names.Column_StartPosition];
                    var finish = (NotePosition)(long)row[Names.Column_FinishPosition];
                    var flick = (NoteFlickType)(long)row[Names.Column_FlickType];
                    var prevFlick = (int)(long)row[Names.Column_PrevFlickNoteID];
                    var nextFlick = (int)(long)row[Names.Column_NextFlickNoteID];
                    var hold = (int)(long)row[Names.Column_HoldTargetID];

                    EnsureBarIndex(score, barIndex);
                    var bar = score.Bars[barIndex];
                    var note = bar.AddNoteWithoutUpdatingGlobalNotes(id);
                    if (note != null) {
                        note.IndexInGrid = grid;
                        note.StartPosition = start;
                        note.FinishPosition = finish;
                        note.FlickType = flick;
                        note.PrevFlickNoteID = prevFlick;
                        note.NextFlickNoteID = nextFlick;
                        note.HoldTargetID = hold;
                    } else {
                        Debug.Print("Note with id {0} already exists.", id);
                    }
                }
            }
        }

        private static void ReadBarParams(SQLiteConnection connection, Score score) {
            using (var table = new DataTable()) {
                SQLiteHelper.ReadBarParamsTable(connection, score.Difficulty, table);
                foreach (DataRow row in table.Rows) {
                    var index = (int)(long)row[Names.Column_BarIndex];
                    var grid = (int?)(long?)row[Names.Column_GridPerSignature];
                    var signature = (int?)(long?)row[Names.Column_Signature];
                    if (index < score.Bars.Count) {
                        score.Bars[index].Params = new BarParams {
                            UserDefinedGridPerSignature = grid,
                            UserDefinedSignature = signature
                        };
                    }
                }
            }
        }

        private static void ReadSpecialNotes(SQLiteConnection connection, Score score) {
            using (var table = new DataTable()) {
                SQLiteHelper.ReadSpecialNotesTable(connection, score.Difficulty, table);
                foreach (DataRow row in table.Rows) {
                    var id = (int)(long)row[Names.Column_ID];
                    var barIndex = (int)(long)row[Names.Column_BarIndex];
                    var grid = (int)(long)row[Names.Column_IndexInGrid];
                    var type = (int)(long)row[Names.Column_NoteType];
                    var paramsString = (string)row[Names.Column_ParamValues];
                    if (barIndex < score.Bars.Count) {
                        var bar = score.Bars[barIndex];
                        // Special notes are not added during the ReadScores() process, so we call AddNote() rather than AddNoteWithoutUpdatingGlobalNotes().
                        var note = bar.Notes.FirstOrDefault(n => n.Type == (NoteType)type && n.IndexInGrid == grid);
                        if (note == null) {
                            note = bar.AddNote(id);
                            note.SetSpecialType((NoteType)type);
                            note.IndexInGrid = grid;
                            note.ExtraParams = NoteExtraParams.FromDataString(paramsString, note);
                        } else {
                            note.ExtraParams.UpdateByDataString(paramsString);
                        }
                    }
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
