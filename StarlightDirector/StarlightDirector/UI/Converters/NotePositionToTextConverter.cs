using System;
using System.Globalization;
using System.Windows.Data;
using StarlightDirector.Entities;

namespace StarlightDirector.UI.Converters {
    public sealed class NotePositionToTextConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var v = (int)(NotePosition)value;
            return v != 0 ? v.ToString() : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

    }
}
