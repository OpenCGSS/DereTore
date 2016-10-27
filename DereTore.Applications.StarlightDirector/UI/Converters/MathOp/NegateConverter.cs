using System;
using System.Globalization;
using System.Windows;

namespace DereTore.Applications.StarlightDirector.UI.Converters.MathOp {
    public sealed class NegateConverter : UnaryOpConverterBase {

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is double) {
                return -(double)value;
            } else if (value is bool) {
                return !(bool)value;
            } else {
                return DependencyProperty.UnsetValue;
            }
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is double) {
                return -(double)value;
            } else if (value is bool) {
                return !(bool)value;
            } else {
                return DependencyProperty.UnsetValue;
            }
        }

    }
}
