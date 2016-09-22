using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.UI.Controls;

namespace DereTore.Applications.StarlightDirector.UI.Converters {
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
