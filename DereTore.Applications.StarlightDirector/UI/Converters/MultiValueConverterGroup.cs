using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace DereTore.Applications.StarlightDirector.UI.Converters {
    public sealed class MultiValueConverterGroup : List<object>, IMultiValueConverter {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            var current = (object)values;
            var param = (ConverterGroupParameters)parameter;
            var i = -1;
            foreach (var converter in this) {
                ++i;
                var multiValueConverter = converter as IMultiValueConverter;
                if (multiValueConverter != null) {
                    current = multiValueConverter.Convert((object[])current, targetType, param[i], culture);
                    continue;
                }
                var valueConverter = converter as IValueConverter;
                if (valueConverter != null) {
                    current = valueConverter.Convert(current, targetType, param[i], culture);
                    continue;
                }
                throw new ArgumentException("One of the converters is not IValueConverter or IMultiValueConverter.");
            }
            return current;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

    }
}
