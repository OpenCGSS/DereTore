using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DereTore.Applications.StarlightDirector.UI.Converters {
    public sealed class DoubleToStringConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((double)value).ToString("F3");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            string stringValue;
            try {
                stringValue = (string)value;
            } catch (InvalidCastException) {
                return DependencyProperty.UnsetValue;
            }
            double d;
            var b = double.TryParse(stringValue, out d);
            if (!b) {
                Debug.Print($"Object '{value}' is not a double.");
                return DependencyProperty.UnsetValue;
            }
            return d;
        }

    }
}
