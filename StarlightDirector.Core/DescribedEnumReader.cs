using System;
using System.Collections.Generic;
using System.ComponentModel;
using DereTore;

namespace StarlightDirector {
    internal static class DescribedEnumReader {

        public static string Read(Enum value, Type enumType) {
            var flagsAttribute = enumType.GetCustomAttributes(typeof(FlagsAttribute), false);
            if (flagsAttribute.Length == 0) {
                var fi = enumType.GetField(Enum.GetName(enumType, value));
                var dna = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));
                return dna != null ? dna.Description : value.ToString();
            } else {
                var enumValues = Enum.GetValues(enumType);
                var names = new List<string>();
                foreach (var enumValue in enumValues) {
                    var v = (Enum)enumValue;
                    if (!value.HasFlag(v)) {
                        continue;
                    }
                    var fi = enumType.GetField(Enum.GetName(enumType, v));
                    var dna = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));
                    names.Add(dna != null ? dna.Description : v.ToString());
                }
                return names.Count > 0 ? names.BuildString(", ") : string.Empty;
            }
        }

    }
}
