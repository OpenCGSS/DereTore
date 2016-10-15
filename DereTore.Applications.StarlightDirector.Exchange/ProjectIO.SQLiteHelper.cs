using System.Collections.Specialized;
using System.Data;
using System.Data.SQLite;

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

            public static void CreateTable(SQLiteTransaction transaction, string tableName, ref SQLiteCommand command) {
                CreateTable(transaction.Connection, tableName, ref command);
            }

            public static void CreateTable(SQLiteConnection connection, string tableName, ref SQLiteCommand command) {
                if (command == null) {
                    command = connection.CreateCommand();
                }
                // Have to use LONGTEXT (2^31-1) rather than TEXT (32768).
                command.CommandText = $"CREATE TABLE {tableName} (key LONGTEXT PRIMARY KEY, value LONGTEXT);";
                command.ExecuteNonQuery();
            }

        }

    }
}
