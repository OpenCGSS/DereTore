using System.Collections.Specialized;
using System.Data;
using System.Data.SQLite;
using StarlightDirector.Entities;

namespace StarlightDirector.Exchange {
    static partial class ProjectIO {

        private static class SQLiteHelper {

            public static bool DoesTableExist(SQLiteTransaction transaction, string tableName) {
                return DoesTableExist(transaction.Connection, tableName);
            }

            public static bool DoesTableExist(SQLiteConnection connection, string tableName) {
                using (var command = connection.CreateCommand()) {
                    command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' AND name = @tableName;";
                    command.Parameters.Add("tableName", DbType.AnsiString).Value = tableName;
                    var value = command.ExecuteScalar();
                    return value != null;
                }
            }

            public static bool DoesColumnExist(SQLiteTransaction transaction, string tableName, string columnName) {
                return DoesColumnExist(transaction.Connection, tableName, columnName);
            }

            public static bool DoesColumnExist(SQLiteConnection connection, string tableName, string columnName) {
                using (var command = connection.CreateCommand()) {
                    command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' AND name = @tableName;";
                    command.Parameters.Add("tableName", DbType.AnsiString).Value = tableName;
                    var value = command.ExecuteScalar();
                    if (value == null) {
                        return false;
                    }
                    // TODO: Is it possible to use command parameters?
                    command.CommandText = $"PRAGMA table_info('{tableName}');";
                    command.Parameters.RemoveAt("tableName");
                    using (var reader = command.ExecuteReader()) {
                        while (reader.NextResult()) {
                            while (reader.Read()) {
                                var ordinal = reader.GetOrdinal("name");
                                value = reader.GetValue(ordinal);
                                if ((string)value == columnName) {
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            }

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

            public static void CreateKeyValueTable(SQLiteTransaction transaction, string tableName) {
                CreateKeyValueTable(transaction.Connection, tableName);
            }

            public static void CreateKeyValueTable(SQLiteConnection connection, string tableName) {
                using (var command = connection.CreateCommand()) {
                    // Have to use LONGTEXT (2^31-1) rather than TEXT (32768).
                    command.CommandText = $"CREATE TABLE {tableName} (key LONGTEXT PRIMARY KEY NOT NULL, value LONGTEXT NOT NULL);";
                    command.ExecuteNonQuery();
                }
            }

            public static void CreateBarParamsTable(SQLiteTransaction transaction) {
                CreateBarParamsTable(transaction.Connection);
            }

            public static void CreateBarParamsTable(SQLiteConnection connection) {
                using (var command = connection.CreateCommand()) {
                    command.CommandText = $@"CREATE TABLE {Names.Table_BarParams} (
{Names.Column_Difficulty} INTEGER NOT NULL, {Names.Column_BarIndex} INTEGER NOT NULL, {Names.Column_GridPerSignature} INTEGER, {Names.Column_Signature} INTEGER,
PRIMARY KEY ({Names.Column_Difficulty}, {Names.Column_BarIndex}));";
                    command.ExecuteNonQuery();
                }
            }

            public static void CreateSpecialNotesTable(SQLiteTransaction transaction) {
                CreateSpecialNotesTable(transaction.Connection);
            }

            public static void CreateSpecialNotesTable(SQLiteConnection connection) {
                using (var command = connection.CreateCommand()) {
                    command.CommandText = $@"CREATE TABLE {Names.Table_SpecialNotes} (
{Names.Column_ID} INTEGER NOT NULL PRIMARY KEY, {Names.Column_Difficulty} INTEGER NOT NULL, {Names.Column_BarIndex} INTEGER NOT NULL, {Names.Column_IndexInGrid} INTEGER NOT NULL,
{Names.Column_NoteType} INTEGER NOT NULL, {Names.Column_ParamValues} TEXT NOT NULL,
FOREIGN KEY ({Names.Column_ID}) REFERENCES {Names.Table_NoteIDs}({Names.Column_ID}));";
                    command.ExecuteNonQuery();
                }
            }

            public static void CreateScoresTable(SQLiteTransaction transaction) {
                CreateScoresTable(transaction.Connection);
            }

            public static void CreateScoresTable(SQLiteConnection connection) {
                using (var command = connection.CreateCommand()) {
                    command.CommandText = $"CREATE TABLE {Names.Table_NoteIDs} ({Names.Column_ID} INTEGER NOT NULL PRIMARY KEY);";
                    command.ExecuteNonQuery();
                    command.CommandText = $@"CREATE TABLE {Names.Table_Notes} (
{Names.Column_ID} INTEGER PRIMARY KEY NOT NULL, {Names.Column_Difficulty} INTEGER NOT NULL, {Names.Column_BarIndex} INTEGER NOT NULL, {Names.Column_IndexInGrid} INTEGER NOT NULL,
{Names.Column_NoteType} INTEGER NOT NULL, {Names.Column_StartPosition} INTEGER NOT NULL, {Names.Column_FinishPosition} INTEGER NOT NULL, {Names.Column_FlickType} INTEGER NOT NULL,
{Names.Column_PrevFlickNoteID} INTEGER NOT NULL, {Names.Column_NextFlickNoteID} NOT NULL, {Names.Column_SyncTargetID} INTEGER NOT NULL, {Names.Column_HoldTargetID} INTEGER NOT NULL,
FOREIGN KEY ({Names.Column_ID}) REFERENCES {Names.Table_NoteIDs}({Names.Column_ID}), FOREIGN KEY ({Names.Column_PrevFlickNoteID}) REFERENCES {Names.Table_NoteIDs}({Names.Column_ID}),
FOREIGN KEY ({Names.Column_NextFlickNoteID}) REFERENCES {Names.Table_NoteIDs}({Names.Column_ID}), FOREIGN KEY ({Names.Column_SyncTargetID}) REFERENCES {Names.Table_NoteIDs}({Names.Column_ID}),
FOREIGN KEY ({Names.Column_HoldTargetID}) REFERENCES {Names.Table_NoteIDs}({Names.Column_ID}));";
                    command.ExecuteNonQuery();
                }
            }

            public static void InsertNoteID(SQLiteTransaction transaction, int id, ref SQLiteCommand command) {
                InsertNoteID(transaction.Connection, id, ref command);
            }

            public static void InsertNoteID(SQLiteConnection connection, int id, ref SQLiteCommand command) {
                if (command == null) {
                    command = connection.CreateCommand();
                    command.CommandText = $"INSERT INTO {Names.Table_NoteIDs} ({Names.Column_ID}) VALUES (@id);";
                    command.Parameters.Add("id", DbType.Int32);
                }
                command.Parameters["id"].Value = id;
                command.ExecuteNonQuery();
            }

            public static void InsertNote(SQLiteTransaction transaction, Note note, ref SQLiteCommand command) {
                InsertNote(transaction.Connection, note, ref command);
            }

            public static void InsertNote(SQLiteConnection connection, Note note, ref SQLiteCommand command) {
                if (command == null) {
                    command = connection.CreateCommand();
                    command.CommandText = $@"INSERT INTO {Names.Table_Notes} (
{Names.Column_ID}, {Names.Column_Difficulty}, {Names.Column_BarIndex}, {Names.Column_IndexInGrid}, {Names.Column_NoteType}, {Names.Column_StartPosition}, {Names.Column_FinishPosition},
{Names.Column_FlickType}, {Names.Column_PrevFlickNoteID}, {Names.Column_NextFlickNoteID}, {Names.Column_SyncTargetID}, {Names.Column_HoldTargetID}
) VALUES (@id, @difficulty, @bar, @grid, @note_type, @start, @finish, @flick, @prev_flick, @next_flick, @sync, @hold);";
                    command.Parameters.Add("id", DbType.Int32);
                    command.Parameters.Add("difficulty", DbType.Int32);
                    command.Parameters.Add("bar", DbType.Int32);
                    command.Parameters.Add("grid", DbType.Int32);
                    command.Parameters.Add("note_type", DbType.Int32);
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
                command.Parameters["note_type"].Value = (int)note.Type;
                command.Parameters["start"].Value = (int)note.StartPosition;
                command.Parameters["finish"].Value = (int)note.FinishPosition;
                command.Parameters["flick"].Value = (int)note.FlickType;
                command.Parameters["prev_flick"].Value = note.PrevFlickOrSlideNoteID;
                command.Parameters["next_flick"].Value = note.NextFlickOrSlideNoteID;
                command.Parameters["sync"].Value = note.SyncTargetID;
                command.Parameters["hold"].Value = note.HoldTargetID;
                command.ExecuteNonQuery();
            }

            public static void InsertBarParams(SQLiteTransaction transaction, Bar bar, ref SQLiteCommand command) {
                InsertBarParams(transaction.Connection, bar, ref command);
            }

            public static void InsertBarParams(SQLiteConnection connection, Bar bar, ref SQLiteCommand command) {
                if (bar.Params == null) {
                    return;
                }
                if (command == null) {
                    command = connection.CreateCommand();
                    command.CommandText = $"INSERT INTO {Names.Table_BarParams} ({Names.Column_Difficulty}, {Names.Column_BarIndex}, {Names.Column_GridPerSignature}, {Names.Column_Signature}) VALUES (@difficulty, @index, @grid, @signature);";
                    command.Parameters.Add("difficulty", DbType.Int32);
                    command.Parameters.Add("index", DbType.Int32);
                    command.Parameters.Add("grid", DbType.Int32);
                    command.Parameters.Add("signature", DbType.Int32);
                }
                command.Parameters["difficulty"].Value = (int)bar.Score.Difficulty;
                command.Parameters["index"].Value = bar.Index;
                command.Parameters["grid"].Value = bar.Params.UserDefinedGridPerSignature;
                command.Parameters["signature"].Value = bar.Params.UserDefinedSignature;
                command.ExecuteNonQuery();
            }

            public static void InsertSpecialNote(SQLiteTransaction transaction, Note note, ref SQLiteCommand command) {
                InsertSpecialNote(transaction.Connection, note, ref command);
            }

            public static void InsertSpecialNote(SQLiteConnection connection, Note note, ref SQLiteCommand command) {
                if (command == null) {
                    command = connection.CreateCommand();
                    command.CommandText = $"INSERT INTO {Names.Table_SpecialNotes} ({Names.Column_ID}, {Names.Column_Difficulty}, {Names.Column_BarIndex}, {Names.Column_IndexInGrid}, {Names.Column_NoteType}, {Names.Column_ParamValues}) VALUES (@id, @diff, @bar, @grid, @type, @pv);";
                    command.Parameters.Add("id", DbType.Int32);
                    command.Parameters.Add("diff", DbType.Int32);
                    command.Parameters.Add("bar", DbType.Int32);
                    command.Parameters.Add("grid", DbType.Int32);
                    command.Parameters.Add("type", DbType.Int32);
                    // Parameter values
                    command.Parameters.Add("pv", DbType.AnsiString);
                }
                command.Parameters["id"].Value = note.ID;
                command.Parameters["diff"].Value = note.Bar.Score.Difficulty;
                command.Parameters["bar"].Value = note.Bar.Index;
                command.Parameters["grid"].Value = note.IndexInGrid;
                command.Parameters["type"].Value = (int)note.Type;
                command.Parameters["pv"].Value = note.ExtraParams.GetDataString();
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

            public static void ReadBarParamsTable(SQLiteConnection connection, Difficulty difficulty, DataTable dataTable) {
                using (var command = connection.CreateCommand()) {
                    command.CommandText = $"SELECT * FROM {Names.Table_BarParams} WHERE {Names.Column_Difficulty} = @difficulty;";
                    command.Parameters.Add("difficulty", DbType.Int32).Value = (int)difficulty;
                    using (var adapter = new SQLiteDataAdapter(command)) {
                        adapter.Fill(dataTable);
                    }
                }
            }

            public static void ReadSpecialNotesTable(SQLiteConnection connection, Difficulty difficulty, DataTable dataTable) {
                using (var command = connection.CreateCommand()) {
                    command.CommandText = $"SELECT * FROM {Names.Table_SpecialNotes} WHERE {Names.Column_Difficulty} = @difficulty;";
                    command.Parameters.Add("difficulty", DbType.Int32).Value = (int)difficulty;
                    using (var adapter = new SQLiteDataAdapter(command)) {
                        adapter.Fill(dataTable);
                    }
                }
            }

        }

    }
}
