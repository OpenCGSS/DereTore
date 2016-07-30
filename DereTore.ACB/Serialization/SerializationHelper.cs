using System;
using System.Collections.Generic;
using System.Reflection;

namespace DereTore.ACB.Serialization {
    internal static class SerializationHelper {

        public static MemberAbstract[] GetSearchTargetFieldsAndProperties(UtfRowBase tableObject) {
            var type = tableObject.GetType();
            var objFields = type.GetFields(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic);
            var validDescriptors = new List<MemberAbstract>();
            foreach (var field in objFields) {
                var caField = field.GetCustomAttributes(typeof(UtfFieldAttribute), false);
                // It is a field that needs serialization.
                if (caField.Length == 1) {
                    var caArchive = field.GetCustomAttributes(typeof(Afs2ArchiveAttribute), false);
                    var ca1 = caField[0] as UtfFieldAttribute;
                    var ca2 = caArchive.Length == 1 ? caArchive[0] as Afs2ArchiveAttribute : null;
                    validDescriptors.Add(new MemberAbstract(field, ca1, ca2));
                }
            }
            validDescriptors.Sort((d1, d2) => d1.FieldAttribute.Order - d2.FieldAttribute.Order);
            return validDescriptors.ToArray();
        }

    }
}
