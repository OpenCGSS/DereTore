using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace DereTore.Applications.StarlightDirector.UI.Converters {
    // http://stackoverflow.com/questions/1350598/passing-two-command-parameters-using-a-wpf-binding
    public sealed class EquityConverter : IMultiValueConverter {

        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture) {
            if (value.Length != 2) {
                throw new ArgumentException("There should be 2 objects for EquityConverter.");
            }
            var b = EqualityComparer<object>.Default.Equals(value[0], value[1]);
            var negate = (bool?)parameter;
            if (negate.HasValue && negate.Value) {
                b = !b;
            }
            return b;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

        public static readonly bool Negate = true;

    }
}
