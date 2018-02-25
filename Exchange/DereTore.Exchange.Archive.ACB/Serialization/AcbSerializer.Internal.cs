using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DereTore.Exchange.Archive.ACB.Serialization {
    public partial class AcbSerializer {

        /// <summary>
        /// Get the unaligned byte array image of a table (UTF row).
        /// Returned array should be aligned by <see cref="ArrayExtensions.RoundUpTo(byte[], int)"/>.
        /// </summary>
        /// <param name="tableRows">Rows in this table.</param>
        /// <returns></returns>
        private byte[] GetTableBytes(UtfRowBase[] tableRows) {
            var tableImage = PrepareTable(tableRows);

            byte[] buffer;

            using (var memory = new MemoryStream()) {
                tableImage.WriteTo(memory);
                buffer = memory.ToArray();
            }

            return buffer;
        }

        private UtfTableImage PrepareTable(UtfRowBase[] tableRows) {
            if (tableRows.Length < 1) {
                throw new ArgumentException("There should be at least one row in a table.", nameof(tableRows));
            }

            var tableRowTypes = tableRows.Select(row => row.GetType()).ToArray();

            if (tableRowTypes.Length > 1) {
                for (var i = 1; i < tableRowTypes.Length; ++i) {
                    if (tableRowTypes[i] != tableRowTypes[i - 1]) {
                        throw new ArgumentException("All rows must have the same CLR type.");
                    }
                }
            }

            var tableName = GetTableName(tableRows, tableRowTypes[0]);
            var tableImage = new UtfTableImage(tableName, Alignment);

            foreach (var tableRow in tableRows) {
                var targetMembers = SerializationHelper.GetSearchTargetFieldsAndProperties(tableRow);
                var tableImageRow = new List<UtfFieldImage>();

                tableImage.Rows.Add(tableImageRow);

                // TODO: Save misc data first, then the tables.
                foreach (var member in targetMembers) {
                    var fieldInfo = member.FieldInfo;
                    var fieldType = fieldInfo.FieldType;
                    var fieldAttribute = member.FieldAttribute;

                    CheckFieldType(fieldType);

                    var fieldValue = member.FieldInfo.GetValue(tableRow);

                    var fieldImage = new UtfFieldImage {
                        Order = fieldAttribute.Order,
                        Name = string.IsNullOrEmpty(fieldAttribute.FieldName) ? fieldInfo.Name : fieldAttribute.FieldName,
                        Storage = fieldAttribute.Storage,
                    };

                    // Empty tables are treated as empty data.
                    if (IsTypeRowList(fieldType) && fieldValue != null && ((UtfRowBase[])fieldValue).Length > 0) {
                        var tableBytes = GetTableBytes((UtfRowBase[])fieldValue);

                        fieldImage.SetValue(tableBytes);
                        fieldImage.IsTable = true;
                    } else if (fieldType == typeof(byte[]) && member.ArchiveAttribute != null) {
                        var files = new List<byte[]> {
                            (byte[])fieldValue
                        };

                        var archiveBytes = SerializationHelper.GetAfs2ArchiveBytes(files.AsReadOnly(), Alignment);

                        fieldImage.SetValue(archiveBytes);
                        fieldImage.IsTable = true;
                    } else {
                        if (fieldValue == null) {
                            fieldImage.SetNullValue(MapUtfColumeTypeFromRawType(fieldType));
                        } else {
                            fieldImage.SetValue(fieldValue);
                        }
                    }

                    tableImageRow.Add(fieldImage);
                }
            }
            return tableImage;
        }

        private static void CheckFieldType(Type type) {
            if (!SupportedTypes.Contains(type) && !IsTypeRowList(type)) {
                throw new InvalidCastException($"Unsupported type: '{type.FullName}'");
            }
        }

        private static bool IsTypeRowList(Type type) {
            return type.IsArray && type.HasElementType && type.GetElementType().IsSubclassOf(typeof(UtfRowBase));
        }

        private static ColumnType MapUtfColumeTypeFromRawType(Type rawType) {
            if (IsTypeRowList(rawType)) {
                return ColumnType.Data;
            }

            var index = SupportedTypes.IndexOf(rawType);

            return (ColumnType)index;
        }

        private static string GetTableName(UtfRowBase[] tableRows, Type tableType) {
            if (tableRows == null) {
                throw new ArgumentNullException(nameof(tableRows));
            }

            var tableAttribute = SerializationHelper.GetCustomAttribute<UtfTableAttribute>(tableType);

            if (!string.IsNullOrEmpty(tableAttribute?.Name)) {
                return tableAttribute.Name;
            } else {
                var s = tableType.Name;

                if (s.EndsWith("Table")) {
                    s = s.Substring(0, s.Length - 5);
                }

                return s;
            }
        }

        private static readonly Type[] SupportedTypes = {
            typeof(byte), typeof(sbyte), typeof(ushort), typeof(short), typeof(uint), typeof(int), typeof(ulong), typeof(long),
            typeof(float),typeof(double),
            typeof(string),
            typeof(byte[])
        };
    }
}
