using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;

namespace DereTore.Applications.ScoreEditor.Model {
    public sealed class Score {

        public static Score FromFile(string fileName, Difficulty difficulty) {
            if (!IsScoreFile(fileName)) {
                throw new FormatException($"'{fileName}' is not a score file.");
            }
            return new Score(fileName, difficulty);
        }

        public event EventHandler<ScoreChangedEventArgs> ScoreChanged;

        public ReadOnlyCollection<Note> Notes => _notes;

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

        public bool Validate(out string[] reasons) {
            var b = true;
            var notes = _editableNotes;
            var flickGroupNoteCount = new Dictionary<int, int>();
            var r = new List<string>();
            foreach (var note in notes) {
                switch (note.Type) {
                    case NoteType.TapOrFlick:
                        if (note.IsSync) {
                            if (note.SyncPairNote == null) {
                                r.Add($"Missing sync pair note at note ID #{note.Id}.");
                                b = false;
                            }
                        }
                        if (note.IsFlick) {
                            if (!flickGroupNoteCount.ContainsKey(note.FlickGroupId)) {
                                flickGroupNoteCount.Add(note.FlickGroupId, 0);
                            }
                            ++flickGroupNoteCount[note.FlickGroupId];
                        }
                        break;
                    case NoteType.Hold:
                        if (note.IsSync) {
                            if (note.SyncPairNote == null) {
                                r.Add($"Missing sync pair note at note ID #{note.Id}.");
                                b = false;
                            }
                        }
                        if (note.NextHoldNote != null && note.PrevHoldNote != null) {
                            r.Add($"Note ${note.Id} has both previous and next hold notes.");
                            b = false;
                        }
                        if (note.NextHoldNote != null) {
                            if (note.NextHoldNote.PrevHoldNote != note) {
                                r.Add($"Broken next hold note detected at note ID #{note.Id}.");
                            }
                        }
                        if (note.PrevHoldNote != null) {
                            if (note.PrevHoldNote.NextHoldNote != note) {
                                r.Add($"Broken previous hold note detected at note ID #{note.Id}.");
                            }
                        }
                        break;
                }
            }
            r.AddRange(from flickGroup in flickGroupNoteCount
                       where flickGroup.Value < 2
                       select $"[WARNING] Flick group ID #{flickGroup.Key} does not contain enough notes (at least 2).");
            reasons = r.ToArray();
            return b;
        }

        public string SaveToCsv() {
            var tempFileName = Path.GetTempFileName();
            using (var stream = File.Open(tempFileName, FileMode.Create, FileAccess.Write)) {
                using (var writer = new StreamWriter(stream, Encoding.UTF8)) {
                    var config = new CsvConfiguration();
                    config.RegisterClassMap<ScoreCsvMap>();
                    config.HasHeaderRecord = true;
                    config.TrimFields = false;
                    using (var csv = new CsvWriter(writer, config)) {
                        var newList = new List<Note>(Notes);
                        newList.Sort((n1, n2) => n1.Id.CompareTo(n2.Id));
                        csv.WriteRecords(newList);
                    }
                }
            }
            // Bug: WTF? Why can't all the data be written into a MemoryStream right after the csv.WriteRecords() call, but a FileStream?
            var text = File.ReadAllText(tempFileName, Encoding.UTF8);
            File.Delete(tempFileName);
            return text;
        }

        internal List<Note> EditableNotes => _editableNotes;

        internal void RaiseScoreChanged(object sender, ScoreChangedEventArgs e) {
            ScoreChanged?.Invoke(sender, e);
        }

        private Score(string fileName, Difficulty difficulty) {
            var sanitizedFileName = SanitizeString(fileName);
            using (var connection = new SQLiteConnection($"Data Source={sanitizedFileName};")) {
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
                                config.RegisterClassMap<ScoreCsvMap>();
                                config.HasHeaderRecord = true;
                                using (var csv = new CsvReader(reader, config)) {
                                    var items = new List<Note>();
                                    while (csv.Read()) {
                                        items.Add(csv.GetRecord<Note>());
                                    }
                                    items.Sort((s1, s2) => s1.HitTiming > s2.HitTiming ? 1 : (s2.HitTiming > s1.HitTiming ? -1 : 0));
                                    _editableNotes = items;
                                    _notes = _editableNotes.AsReadOnly();
                                }
                            }
                        }
                    }
                }
            }
            UpdateNotesInfo();
        }

        private void UpdateNotesInfo() {
            var notes = _editableNotes;
            var holdNotesToBeMatched = new List<Note>();
            var flickGroupNoteCount = new Dictionary<int, int>();
            var i = 0;
            foreach (var note in notes) {
                switch (note.Type) {
                    case NoteType.TapOrFlick:
                        if (note.IsSync) {
                            var syncPairItem = notes.FirstOrDefault(n => n != note && n.HitTiming.Equals(note.HitTiming) && n.IsSync);
                            if (syncPairItem == null) {
                                throw new FormatException($"Missing sync pair note at note ID #{note.Id}.");
                            }
                            note.SyncPairNote = syncPairItem;
                        }
                        if (note.IsFlick) {
                            if (!flickGroupNoteCount.ContainsKey(note.FlickGroupId)) {
                                flickGroupNoteCount.Add(note.FlickGroupId, 0);
                            }
                            ++flickGroupNoteCount[note.FlickGroupId];
                            var nextFlickItem = notes.Skip(i + 1).FirstOrDefault(n => n.IsFlick && n.FlickGroupId != 0 && n.FlickGroupId == note.FlickGroupId);
                            if (nextFlickItem == null) {
                                if (flickGroupNoteCount[note.FlickGroupId] < 2) {
                                    Debug.WriteLine($"[WARNING] No enough flick notes to form a flick group at note ID #{note.Id}, group ID {note.FlickGroupId}.");
                                }
                            } else {
                                note.NextFlickNote = nextFlickItem;
                                nextFlickItem.PrevFlickNote = note;
                            }
                        }
                        break;
                    case NoteType.Hold:
                        if (note.IsSync) {
                            var syncPairItem = notes.FirstOrDefault(n => n != note && n.HitTiming.Equals(note.HitTiming) && n.IsSync);
                            if (syncPairItem == null) {
                                throw new FormatException($"Missing sync pair note at note ID #{note.Id}.");
                            }
                            note.SyncPairNote = syncPairItem;
                        }
                        if (holdNotesToBeMatched.Contains(note)) {
                            holdNotesToBeMatched.Remove(note);
                            break;
                        }
                        var endHoldItem = notes.Skip(i + 1).FirstOrDefault(n => n.FinishPosition == note.FinishPosition);
                        if (endHoldItem == null) {
                            throw new FormatException($"Missing end hold note at note ID #{note.Id}.");
                        }
                        note.NextHoldNote = endHoldItem;
                        endHoldItem.PrevHoldNote = note;
                        // The end hold note always follows the trail of start hold note, so the literal value of its 'start' field is ignored.
                        // See song_1001, 'Master' difficulty, #189-#192, #479-#483.
                        endHoldItem.StartPosition = note.StartPosition;
                        holdNotesToBeMatched.Add(endHoldItem);
                        break;
                }
                ++i;
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

        private readonly List<Note> _editableNotes;
        private readonly ReadOnlyCollection<Note> _notes;

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
