using System;
using System.Globalization;

namespace DereTore.Applications.StarlightDirector.UI.Converters.MathOp {
    public sealed class MathConstantConverter : MathConverterBase {

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return GetObjectValue(parameter);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

    }
}
