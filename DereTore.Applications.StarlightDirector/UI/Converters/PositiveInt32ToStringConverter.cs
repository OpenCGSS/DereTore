using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DereTore.Applications.StarlightDirector.UI.Converters {
    public sealed class PositiveInt32ToStringConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            string stringValue;
            try {
                stringValue = (string)value;
            } catch (InvalidCastException) {
                return DependencyProperty.UnsetValue;
            }
            double d;
            var b = double.TryParse(stringValue, out d);
            if (!b) {
                Debug.Print($"Object '{value}' is not a number.");
                return 0.0;
            }
            var n = (int)d;
            if (n <= 0) {
                Debug.Print($"Error: value {n} is negative.");
                return 0.0;
            }
            return (double)n;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((int)(double)value).ToString();
        }

    }
}
