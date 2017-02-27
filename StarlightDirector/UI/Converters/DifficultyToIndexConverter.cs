using System;
using System.Globalization;
using System.Windows.Data;
using StarlightDirector.Entities;

namespace StarlightDirector.UI.Converters {
    public sealed class DifficultyToIndexConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var e = (Difficulty)value;
            return (int)e - 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var n = (int)value;
            return (Difficulty)(n + 1);
        }

    }
}
