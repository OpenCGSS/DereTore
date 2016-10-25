using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector.UI.Converters {
    public sealed class NoteFlickTypeToHorizontalMirrorConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var flickType = (NoteFlickType)value;
            NoteFlickType standard;
            if (parameter == null || parameter == DependencyProperty.UnsetValue || !(parameter is NoteFlickType)) {
                standard = Standard;
            } else {
                standard = (NoteFlickType)parameter;
            }
            return flickType == standard ? 1d : -1d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

        public static readonly NoteFlickType Standard = NoteFlickType.FlickRight;

    }
}
