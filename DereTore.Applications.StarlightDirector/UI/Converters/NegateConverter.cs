using System;
using System.Globalization;
using System.Windows.Data;

namespace DereTore.Applications.StarlightDirector.UI.Converters {
    public sealed class NegateConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return -(double)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return -(double)value;
        }

    }
}
