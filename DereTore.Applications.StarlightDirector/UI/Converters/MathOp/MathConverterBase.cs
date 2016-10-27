using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SysConvert = System.Convert;

namespace DereTore.Applications.StarlightDirector.UI.Converters.MathOp {
    public abstract class MathConverterBase : IValueConverter {

        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);

        public static double GetObjectValue(object value) {
            if (value == null || value == DependencyProperty.UnsetValue) {
                return 0d;
            }
            if (value is double) {
                return (double)value;
            } else if (value is string) {
                double d;
                double.TryParse((string)value, out d);
                return d;
            } else {
                return SysConvert.ToDouble(value);
            }
        }

    }
}
