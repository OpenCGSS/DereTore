using System;

namespace DereTore.Exchange.Archive.ACB.Serialization {
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class UtfFieldAttribute : Attribute {

        public UtfFieldAttribute(int order, ColumnStorage storage = ColumnStorage.PerRow, string fieldName = null) {
            Order = order;
            Storage = storage;
            FieldName = fieldName;
        }

        public int Order { get; }

        public string FieldName { get; }

        public ColumnStorage Storage { get; }

    }
}
