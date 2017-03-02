using System;
using System.Globalization;

namespace StarlightDirector.UI.Converters.MathOp {
    public sealed class MathAddConverter : BinaryOpConverterBase {

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var v1 = GetObjectValue(value);
            var v2 = GetObjectValue(parameter);
            return v1 + v2;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var v1 = GetObjectValue(value);
            var v2 = GetObjectValue(parameter);
            return v1 - v2;
        }

        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            var v = GetObjectValues(values);
            var result = v[0] + v[1];
            //return ConcatResult(values, result);
            return result;
        }

        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

    }
}
