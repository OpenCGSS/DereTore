using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using StarlightDirector.Entities;

namespace StarlightDirector.UI.Converters {
    public sealed class NoteFlickTypeToVisibilityConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (parameter == DependencyProperty.UnsetValue) {
                return Visibility.Hidden;
            }
            var s = (NoteFlickType)value;
            var e = (NoteFlickType)parameter;
            return s == e ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

    }
}
