using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;

namespace DereTore.Application.ScoreEditor.Model {
    public sealed class Score {

        public static Score FromFile(string fileName, Difficulty difficulty) {
            if (!IsScoreFile(fileName)) {
                throw new FormatException($"'{fileName}' is not a score file.");
            }
            return new Score(fileName, difficulty);
        }

        public Note[] Items => _items;

        public static bool IsScoreFile(string fileName) {
            string[] dummy;
            return IsScoreFile(fileName, out dummy);
        }

        public static bool IsScoreFile(string fileName, out string[] supportedNames) {
            supportedNames = null;
            var connectionString = $"Data Source={SanitizeString(fileName)};";
            using (var connection = new SQLiteConnection(connectionString)) {
                connection.Open();
                using (var command = connection.CreateCommand()) {
                    command.CommandText = "SELECT name FROM blobs WHERE name LIKE 'musicscores/m___/%.csv';";
                    try {
                        using (var reader = command.ExecuteReader()) {
                            var names = new List<string>();
                            var result = false;
                            while (reader.Read()) {
                                names.Add(reader.GetString(0));
                                result = true;
                            }
                            supportedNames = names.ToArray();
                            if (result) {
                                connection.Close();
                                return true;
                            }
                        }
                    } catch (Exception ex) when (ex is SQLiteException || ex is InvalidOperationException) {
                        connection.Close();
                        return false;
                    }
                }
                connection.Close();
            }
            return false;
        }

        public static bool ContainsDifficulty(string[] names, Difficulty difficulty) {
            if (difficulty == Difficulty.Invalid) {
                throw new IndexOutOfRangeException("Invalid difficulty.");
            }
            var n = (int)difficulty;
            if (n > names.Length) {
                return false;
            }
            return DifficultyRegexes[n - 1].IsMatch(names[n - 1]);
        }

        private Score(string fileName, Difficulty difficulty) {
            using (var connection = new SQLiteConnection($"Data Source={SanitizeString(fileName)};")) {
                using (var adapter = new SQLiteDataAdapter("SELECT name, data FROM blobs WHERE name LIKE 'musicscores/m___/%.csv' ORDER BY name;", connection)) {
                    using (var dataTable = new DataTable()) {
                        adapter.Fill(dataTable);
                        var n = (int)difficulty;
                        if (dataTable.Rows.Count < n) {
                            throw new ArgumentOutOfRangeException(nameof(difficulty));
                        }
                        --n;
                        var data = dataTable.Rows[n]["data"];
                        if (data.GetType() != typeof(byte[])) {
                            throw new InvalidCastException("The 'data' row should be byte arrays.");
                        }
                        using (var stream = new MemoryStream((byte[])data)) {
                            using (var reader = new StreamReader(stream, Encoding.UTF8)) {
                                var config = new CsvConfiguration();
                                config.RegisterClassMap<ScoreMap>();
                                using (var csv = new CsvReader(reader, config)) {
                                    var items = new List<Note>();
                                    while (csv.Read()) {
                                        items.Add(csv.GetRecord<Note>());
                                    }
                                    items.Sort((s1, s2) => s1.Second > s2.Second ? 1 : (s2.Second > s1.Second ? -1 : 0));
                                    _items = items.ToArray();
                                }
                            }
                        }
                    }
                }
            }
            UpdateNotesInfo();
        }

        private void UpdateNotesInfo() {
            var notes = _items;
            var holdingNotesToBeMatched = new List<Note>();
            var swipeGroupNoteCount = new Dictionary<int, int>();
            for (var i = 0; i < notes.Length; ++i) {
                var note = notes[i];
                switch (note.Type) {
                    case NoteType.TapOrSwipe:
                        if (note.Sync) {
                            var syncPairIndex = notes.FirstIndexOf(n => n != note && n.Second.Equals(note.Second) && n.Sync);
                            if (syncPairIndex < 0) {
                                throw new FormatException($"Missing sync pair note at note #{i + 1}.");
                            }
                            note.SyncPairIndex = syncPairIndex;
                        }
                        if (note.IsSwipe) {
                            if (!swipeGroupNoteCount.ContainsKey(note.GroupId)) {
                                swipeGroupNoteCount.Add(note.GroupId, 0);
                            }
                            ++swipeGroupNoteCount[note.GroupId];
                            var nextSwipeIndex = notes.FirstIndexOf(n => n.IsSwipe && n.GroupId != 0 && n.GroupId == note.GroupId, i + 1);
                            if (nextSwipeIndex < 0) {
                                if (swipeGroupNoteCount[note.GroupId] < 2) {
                                    Debug.WriteLine($"[WARNING] No enough swipe notes to form a swipe group at note #{i + 1}, group ID {note.GroupId}.");
                                }
                            } else {
                                note.NextSwipeIndex = nextSwipeIndex;
                                notes[nextSwipeIndex].PrevSwipeIndex = i;
                            }
                        }
                        break;
                    case NoteType.Hold:
                        if (note.Sync) {
                            var syncPairIndex = notes.FirstIndexOf(n => n != note && n.Second.Equals(note.Second) && n.Sync);
                            if (syncPairIndex < 0) {
                                throw new FormatException($"Missing sync pair note at note #{i + 1}.");
                            }
                            note.SyncPairIndex = syncPairIndex;
                        }
                        if (holdingNotesToBeMatched.Contains(note)) {
                            holdingNotesToBeMatched.Remove(note);
                            break;
                        }
                        var endHoldingIndex = notes.FirstIndexOf(n => n.FinishPosition == note.FinishPosition, i + 1);
                        if (endHoldingIndex < 0) {
                            throw new FormatException($"Missing end holding note at note #{i + 1}.");
                        }
                        note.NextHoldingIndex = endHoldingIndex;
                        notes[endHoldingIndex].PrevHoldingIndex = i;
                        // The end holding note always follows the trail of start holding note, so the literal value of its 'start' field is ignored.
                        // See song_1001, 'Master' difficulty, #189-#192, #479-#483.
                        notes[endHoldingIndex].StartPosition = note.StartPosition;
                        holdingNotesToBeMatched.Add(notes[endHoldingIndex]);
                        break;
                }
            }
        }

        private static string SanitizeString(string s) {
            var shouldCoverWithQuotes = false;
            if (s.IndexOf('"') >= 0) {
                s = s.Replace("\"", "\"\"\"");
                shouldCoverWithQuotes = true;
            }
            if (s.IndexOfAny(CommandlineEscapeChars) >= 0) {
                shouldCoverWithQuotes = true;
            }
            if (s.Any(c => c > 127)) {
                shouldCoverWithQuotes = true;
            }
            return shouldCoverWithQuotes ? "\"" + s + "\"" : s;
        }
        
        private readonly Note[] _items;

        private static readonly char[] CommandlineEscapeChars = { ' ', '&', '%', '#', '@', '!', ',', '~', '+', '=', '(', ')' };

        private static readonly Regex[] DifficultyRegexes = {
            new Regex(@"^musicscores/m[\d]{3}/[\d]+_1\.csv$"),
            new Regex(@"^musicscores/m[\d]{3}/[\d]+_2\.csv$"),
            new Regex(@"^musicscores/m[\d]{3}/[\d]+_3\.csv$"),
            new Regex(@"^musicscores/m[\d]{3}/[\d]+_4\.csv$"),
            new Regex(@"^musicscores/m[\d]{3}/[\d]+_5\.csv$")
        };

    }
}
