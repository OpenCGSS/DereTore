using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace DereTore.Applications.StarlightDirector.UI.Converters {
    public sealed class DescribedEnumToStringConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var enumType = (Type)parameter;
            var fieldInfo = enumType.GetField(Enum.GetName(enumType, value));
            var descriptionAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute));
            return descriptionAttribute != null ? descriptionAttribute.Description : value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }

    }
}
