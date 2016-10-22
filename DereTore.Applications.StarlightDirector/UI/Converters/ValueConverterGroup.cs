using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace DereTore.Applications.StarlightDirector.UI.Converters {
    // http://stackoverflow.com/questions/2607490/is-there-a-way-to-chain-multiple-value-converters-in-xaml
    public sealed class ValueConverterGroup : List<IValueConverter>, IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var i = -1;
            var param = (ConverterGroupParameters)parameter;
            var current = value;
            foreach (var converter in this) {
                ++i;
                current = converter.Convert(value, targetType, param[i], culture);
            }
            return current;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var i = -1;
            var param = (ConverterGroupParameters)parameter;
            var current = value;
            foreach (var converter in this) {
                ++i;
                current = converter.ConvertBack(value, targetType, param[i], culture);
            }
            return current;
        }

    }
}
