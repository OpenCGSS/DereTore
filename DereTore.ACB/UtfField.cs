using System;

namespace DereTore.ACB {
    public sealed class UtfField {

        internal UtfField() {
        }

        public byte Type { get; set; }
        public string Name { get; set; }
        public ColumnType ConstrainedType { get; set; }
        public NumericUnion NumericValue { get; set; }
        public byte[] DataValue { get; set; }
        public string StringValue { get; set; }
        public long Offset { get; set; }
        public long Size { get; set; }

        public object GetValue() {
            var constrainedType = ConstrainedType;
            object ret;
            switch (constrainedType) {
                case ColumnType.Byte:
                    ret = NumericValue.U8;
                    break;
                case ColumnType.SByte:
                    ret = NumericValue.S8;
                    break;
                case ColumnType.UInt16:
                    ret = NumericValue.U16;
                    break;
                case ColumnType.Int16:
                    ret = NumericValue.S16;
                    break;
                case ColumnType.UInt32:
                    ret = NumericValue.U32;
                    break;
                case ColumnType.Int32:
                    ret = NumericValue.S32;
                    break;
                case ColumnType.UInt64:
                    ret = NumericValue.U64;
                    break;
                case ColumnType.Int64:
                    ret = NumericValue.S64;
                    break;
                case ColumnType.Single:
                    ret = NumericValue.R32;
                    break;
                case ColumnType.Double:
                    ret = NumericValue.R64;
                    break;
                case ColumnType.String:
                    ret = StringValue;
                    break;
                case ColumnType.Data:
                    ret = DataValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(constrainedType));
            }
            return ret;
        }
    }
}
