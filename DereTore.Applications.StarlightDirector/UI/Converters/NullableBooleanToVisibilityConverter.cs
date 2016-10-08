using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DereTore.Applications.StarlightDirector.UI.Converters {
    public sealed class NullableBooleanToVisibilityConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var b = (bool?)value;
            if (b.HasValue) {
                return b.Value ? Visibility.Visible : Visibility.Hidden;
            } else {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var b = (Visibility)value;
            switch (b) {
                case Visibility.Visible:
                    return true;
                case Visibility.Hidden:
                    return false;
                case Visibility.Collapsed:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }
}
