using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace DereTore.Applications.StarlightDirector.UI.Converters.MathOp {
    public abstract class BinaryOpConverterBase : MathConverterBase, IMultiValueConverter {

        public abstract object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);

        public abstract object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture);
        
        public static double[] GetObjectValues(object[] values) {
            if (values.Length < 2) {
                throw new ArgumentException("At least 2 values are required for BinaryOpConverterBase converting without parameter.");
            }
            var v1 = GetObjectValue(values[0]);
            var v2 = GetObjectValue(values[1]);
            return new[] { v1, v2 };
        }

        public static object[] ConcatResult(object[] values, double result) {
            var r = new[] { (object)result };
            return r.Concat(values.Skip(2)).ToArray();
        }

    }
}
