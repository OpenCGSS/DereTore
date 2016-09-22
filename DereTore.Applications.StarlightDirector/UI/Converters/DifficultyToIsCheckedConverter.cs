using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector.UI.Converters {
    public sealed class DifficultyToIsCheckedConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            Difficulty e, p;
            e = (Difficulty)value;
            p = (Difficulty)parameter;
            return e == p;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

    }
}
