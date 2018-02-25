using System;

namespace DereTore.Exchange.Archive.ACB.Serialization {
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class UtfFieldAttribute : Attribute {

        public UtfFieldAttribute(int order = -1, ColumnStorage storage = ColumnStorage.PerRow, string fieldName = null) {
            Order = order;
            Storage = storage;
            FieldName = fieldName;
        }

        public int Order { get; internal set; }

        public string FieldName { get; }

        public ColumnStorage Storage { get; }

    }
}
