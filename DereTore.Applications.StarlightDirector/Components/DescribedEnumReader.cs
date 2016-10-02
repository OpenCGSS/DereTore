using System;
using System.ComponentModel;

namespace DereTore.Applications.StarlightDirector.Components {
    internal static class DescribedEnumReader {

        public static string Read(Enum value, Type enumType) {
            var fi = enumType.GetField(Enum.GetName(enumType, value));
            var dna = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));
            return dna != null ? dna.Description : value.ToString();
        }

    }
}
