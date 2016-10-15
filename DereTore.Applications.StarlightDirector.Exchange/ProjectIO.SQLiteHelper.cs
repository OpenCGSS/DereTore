using System.Collections.Specialized;
using System.Data;
using System.Data.SQLite;
using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector.Exchange {
    static partial class ProjectIO {

        private static class SQLiteHelper {

            public static void SetValue(SQLiteTransaction transaction, string tableName, string key, string value, bool creatingNewDatabase, ref SQLiteCommand command) {
                SetValue(transaction.Connection, tableName, key, value, creatingNewDatabase, ref command);
            }

            public static void SetValue(SQLiteConnection connection, string tableName, string key, string value, bool creatingNewDatabase, ref SQLiteCommand command) {
                if (creatingNewDatabase) {
                    InsertValue(connection, tableName, key, value, ref command);
                } else {
                    UpdateValue(connection, tableName, key, value, ref command);
                }
            }

            public static void InsertValue(SQLiteTransaction transaction, string tableName, string key, string value, ref SQLiteCommand command) {
                InsertValue(transaction.Connection, tableName, key, value, ref command);
            }

            public static void InsertValue(SQLiteConnection connection, string tableName, string key, string value, ref SQLiteCommand command) {
                if (command == null) {
                    command = connection.CreateCommand();
                    command.CommandText = $"INSERT INTO {tableName} (key, value) VALUES (@key, @value);";
                    command.Parameters.Add("key", DbType.AnsiString).Value = key;
                    command.Parameters.Add("value", DbType.AnsiString).Value = value;
                } else {
                    command.CommandText = $"INSERT INTO {tableName} (key, value) VALUES (@key, @value);";
                    command.Parameters["key"].Value = key;
                    command.Parameters["value"].Value = value;
                }
                command.ExecuteNonQuery();
            }

            public static void UpdateValue(SQLiteTransaction transaction, string tableName, string key, string value, ref SQLiteCommand command) {
                UpdateValue(transaction.Connection, tableName, key, value, ref command);
            }

            public static void UpdateValue(SQLiteConnection connection, string tableName, string key, string value, ref SQLiteCommand command) {
                if (command == null) {
                    command = connection.CreateCommand();
                    command.CommandText = $"UPDATE {tableName} SET value = @value WHERE key = @key;";
                    command.Parameters.Add("key", DbType.AnsiString).Value = key;
                    command.Parameters.Add("value", DbType.AnsiString).Value = value;
                } else {
                    command.CommandText = $"UPDATE {tableName} SET value = @value WHERE key = @key;";
                    command.Parameters["key"].Value = key;
                    command.Parameters["value"].Value = value;
                }
                command.ExecuteNonQuery();
            }

            public static string GetValue(SQLiteTransaction transaction, string tableName, string key, ref SQLiteCommand command) {
                return GetValue(transaction.Connection, tableName, key, ref command);
            }

            public static string GetValue(SQLiteConnection connection, string tableName, string key, ref SQLiteCommand command) {
                if (command == null) {
                    command = connection.CreateCommand();
                    command.CommandText = $"SELECT value FROM {tableName} WHERE key = @key;";
                    command.Parameters.Add("key", DbType.AnsiString).Value = key;
                } else {
                    command.CommandText = $"SELECT value FROM {tableName} WHERE key = @key;";
                    command.Parameters["key"].Value = key;
                }
                var value = command.ExecuteScalar();
                return (string)value;
            }

            public static StringDictionary GetValues(SQLiteTransaction transaction, string tableName, ref SQLiteCommand command) {
                return GetValues(transaction.Connection, tableName, ref command);
            }

            public static StringDictionary GetValues(SQLiteConnection connection, string tableName, ref SQLiteCommand command) {
                if (command == null) {
                    command = connection.CreateCommand();
                }
                command.CommandText = $"SELECT key, value FROM {tableName};";
                var result = new StringDictionary();
                using (var reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        var row = reader.GetValues();
                        result.Add(row[0], row[1]);
                    }
                }
                return result;
            }

            public static void CreateKeyValueTable(SQLiteTransaction transaction, string tableName, ref SQLiteCommand command) {
                CreateKeyValueTable(transaction.Connection, tableName, ref command);
            }

            public static void CreateKeyValueTable(SQLiteConnection connection, string tableName, ref SQLiteCommand command) {
                if (command == null) {
                    command = connection.CreateCommand();
                }
                // Have to use LONGTEXT (2^31-1) rather than TEXT (32768).
                command.CommandText = $"CREATE TABLE {tableName} (key LONGTEXT PRIMARY KEY NOT NULL, value LONGTEXT NOT NULL);";
                command.ExecuteNonQuery();
            }

            public static void CreateScoresTables(SQLiteTransaction transaction, ref SQLiteCommand command) {
                CreateScoresTables(transaction.Connection, ref command);
            }

            public static void CreateScoresTables(SQLiteConnection connection, ref SQLiteCommand command) {
                if (command == null) {
                    command = connection.CreateCommand();
                }
                command.CommandText = $"CREATE TABLE {Names.Table_NoteIDs} ({Names.Column_ID} INTEGER NOT NULL PRIMARY KEY);";
                command.ExecuteNonQuery();
                command.CommandText = $@"CREATE TABLE {Names.Table_Notes} (
{Names.Column_ID} INTEGER PRIMARY KEY NOT NULL, {Names.Column_Difficulty} INTEGER NOT NULL, {Names.Column_BarIndex} INTEGER NOT NULL, {Names.Column_IndexInGrid} INTEGER NOT NULL,
{Names.Column_StartPosition} INTEGER NOT NULL, {Names.Column_FinishPosition} INTEGER NOT NULL, {Names.Column_FlickType} INTEGER NOT NULL,
{Names.Column_PrevFlickNoteID} INTEGER NOT NULL, {Names.Column_NextFlickNoteID} NOT NULL, {Names.Column_SyncTargetID} INTEGER NOT NULL, {Names.Column_HoldTargetID} INTEGER NOT NULL,
FOREIGN KEY ({Names.Column_ID}) REFERENCES {Names.Table_NoteIDs}({Names.Column_ID}), FOREIGN KEY ({Names.Column_PrevFlickNoteID}) REFERENCES {Names.Table_NoteIDs}({Names.Column_ID}),
FOREIGN KEY ({Names.Column_NextFlickNoteID}) REFERENCES {Names.Table_NoteIDs}({Names.Column_ID}), FOREIGN KEY ({Names.Column_SyncTargetID}) REFERENCES {Names.Table_NoteIDs}({Names.Column_ID}),
FOREIGN KEY ({Names.Column_HoldTargetID}) REFERENCES {Names.Table_NoteIDs}({Names.Column_ID}));";
                command.ExecuteNonQuery();
            }

            public static void InsertNoteID(SQLiteTransaction transaction, int index, ref SQLiteCommand command) {
                InsertNoteID(transaction.Connection, index, ref command);
            }

            public static void InsertNoteID(SQLiteConnection connection, int index, ref SQLiteCommand command) {
                if (command == null) {
                    command = connection.CreateCommand();
                    command.CommandText = $"INSERT INTO {Names.Table_NoteIDs} ({Names.Column_ID}) VALUES (@id);";
                    command.Parameters.Add("id", DbType.Int32);
                }
                // Have to use LONGTEXT (2^31-1) rather than TEXT (32768).
                command.Parameters["id"].Value = index;
                command.ExecuteNonQuery();
            }

            public static void InsertNote(SQLiteTransaction transaction, Note note, ref SQLiteCommand command) {
                InsertNote(transaction.Connection, note, ref command);
            }

            public static void InsertNote(SQLiteConnection connection, Note note, ref SQLiteCommand command) {
                if (command == null) {
                    command = connection.CreateCommand();
                    command.CommandText = $@"INSERT INTO {Names.Table_Notes} (
{Names.Column_ID}, {Names.Column_Difficulty}, {Names.Column_BarIndex}, {Names.Column_IndexInGrid}, {Names.Column_StartPosition}, {Names.Column_FinishPosition},
{Names.Column_FlickType}, {Names.Column_PrevFlickNoteID}, {Names.Column_NextFlickNoteID}, {Names.Column_SyncTargetID}, {Names.Column_HoldTargetID}
) VALUES (@id, @difficulty, @bar, @grid, @start, @finish, @flick, @prev_flick, @next_flick, @sync, @hold);";
                    command.Parameters.Add("id", DbType.Int32);
                    command.Parameters.Add("difficulty", DbType.Int32);
                    command.Parameters.Add("bar", DbType.Int32);
                    command.Parameters.Add("grid", DbType.Int32);
                    command.Parameters.Add("start", DbType.Int32);
                    command.Parameters.Add("finish", DbType.Int32);
                    command.Parameters.Add("flick", DbType.Int32);
                    command.Parameters.Add("prev_flick", DbType.Int32);
                    command.Parameters.Add("next_flick", DbType.Int32);
                    command.Parameters.Add("sync", DbType.Int32);
                    command.Parameters.Add("hold", DbType.Int32);
                }
                command.Parameters["id"].Value = note.ID;
                command.Parameters["difficulty"].Value = note.Bar.Score.Difficulty;
                command.Parameters["bar"].Value = note.Bar.Index;
                command.Parameters["grid"].Value = note.IndexInGrid;
                command.Parameters["start"].Value = (int)note.StartPosition;
                command.Parameters["finish"].Value = (int)note.FinishPosition;
                command.Parameters["flick"].Value = (int)note.FlickType;
                command.Parameters["prev_flick"].Value = note.PrevFlickNoteID;
                command.Parameters["next_flick"].Value = note.NextFlickNoteID;
                command.Parameters["sync"].Value = note.SyncTargetID;
                command.Parameters["hold"].Value = note.HoldTargetID;
                command.ExecuteNonQuery();
            }

            public static void ReadNotesTable(SQLiteConnection connection, Difficulty difficulty, DataTable table) {
                using (var command = connection.CreateCommand()) {
                    command.CommandText = $"SELECT * FROM {Names.Table_Notes} WHERE {Names.Column_Difficulty} = @difficulty;";
                    command.Parameters.Add("difficulty", DbType.Int32).Value = (int)difficulty;
                    using (var adapter = new SQLiteDataAdapter(command)) {
                        adapter.Fill(table);
                    }
                }
            }

        }

    }
}
