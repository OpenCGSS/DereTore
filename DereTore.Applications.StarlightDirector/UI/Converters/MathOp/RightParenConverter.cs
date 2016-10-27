using System;
using System.Globalization;
using System.Windows;

namespace DereTore.Applications.StarlightDirector.UI.Converters.MathOp {
    public sealed class RightParenConverter : OpSymbolConverterBase {

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return DependencyProperty.UnsetValue;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return DependencyProperty.UnsetValue;
        }

    }
}
