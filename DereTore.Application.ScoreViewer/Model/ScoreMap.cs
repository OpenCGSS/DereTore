using System;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace DereTore.Application.ScoreViewer.Model {
    public sealed class ScoreMap : CsvClassMap<Note> {

        public ScoreMap() {
            Map(m => m.Id).Name("id");
            Map(m => m.Second).Name("sec");
            Map(m => m.Type).Name("type");
            // See song_3034 (m063), master level score. These fields are empty, so we need a custom type converter.
            Map(m => m.StartPosition).Name("startPos").TypeConverter<IntConverter>();
            Map(m => m.FinishPosition).Name("finishPos").TypeConverter<IntConverter>();
            Map(m => m.SwipeType).Name("status");
            Map(m => m.Sync).Name("sync").TypeConverter<IntToBoolConverter>();
            Map(m => m.GroupId).Name("groupId");
        }

        private sealed class IntToBoolConverter : ITypeConverter {

            public string ConvertToString(TypeConverterOptions options, object value) {
                return value.ToString();
            }

            public object ConvertFromString(TypeConverterOptions options, string text) {
                if (string.IsNullOrEmpty(text)) {
                    return false;
                }
                var value = int.Parse(text);
                return value != 0;
            }

            public bool CanConvertFrom(Type type) {
                return true;
            }

            public bool CanConvertTo(Type type) {
                return true;
            }

        }

        private sealed class IntConverter : ITypeConverter {

            public string ConvertToString(TypeConverterOptions options, object value) {
                return value.ToString();
            }

            public object ConvertFromString(TypeConverterOptions options, string text) {
                if (string.IsNullOrEmpty(text)) {
                    return 0;
                }
                var value = int.Parse(text);
                return value;
            }

            public bool CanConvertFrom(Type type) {
                return true;
            }

            public bool CanConvertTo(Type type) {
                return true;
            }

        }

    }
}
