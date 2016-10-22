using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace DereTore.Applications.StarlightDirector.UI.Converters {
    // http://stackoverflow.com/questions/2607490/is-there-a-way-to-chain-multiple-value-converters-in-xaml
    public sealed class ValueConverterGroup : List<IValueConverter>, IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!(parameter is ConverterGroupParameters)) {
                throw new ArgumentException("Converter parameter should be ConverterGroupParameter.", nameof(parameter));
            }
            var param = (ConverterGroupParameters)parameter;
            if (param.Count != Count) {
                throw new ArgumentException($"The number of parameters ({param.Count}) does not equal to the number of converters ({Count}).");
            }
            var current = value;
            var i = -1;
            foreach (var converter in this) {
                ++i;
                current = converter.Convert(value, targetType, param[i], culture);
            }
            return current;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!(parameter is ConverterGroupParameters)) {
                throw new ArgumentException("Converter parameter should be ConverterGroupParameter.", nameof(parameter));
            }
            var param = (ConverterGroupParameters)parameter;
            if (param.Count != Count) {
                throw new ArgumentException($"The number of parameters ({param.Count}) does not equal to the number of converters ({Count}).");
            }
            var current = value;
            var i = -1;
            foreach (var converter in this) {
                ++i;
                current = converter.ConvertBack(value, targetType, param[i], culture);
            }
            return current;
        }

    }
}
