using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DereTore.Applications.StarlightDirector.UI.Converters {
    public sealed class NullableBooleanToVisibilityConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var negate = (bool?)parameter;
            if (negate == null) {
                negate = false;
            }
            var b = (bool?)value;
            if (b.HasValue) {
                return (negate.Value ? !b.Value : b.Value) ? Visibility.Visible : Visibility.Collapsed;
            } else {
                return negate.Value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var b = (Visibility)value;
            switch (b) {
                case Visibility.Visible:
                    return true;
                case Visibility.Hidden:
                    return null;
                case Visibility.Collapsed:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static readonly bool Negate = true;

    }
}
