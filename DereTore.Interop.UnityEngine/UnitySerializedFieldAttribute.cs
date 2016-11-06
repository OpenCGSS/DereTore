using System;
using System.Reflection;

namespace DereTore.Interop.UnityEngine {
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class UnitySerializedFieldAttribute : Attribute {

        public UnitySerializedFieldAttribute(string unityName) {
            UnityName = unityName;
        }

        public string UnityName { get; }

        public static string GetUnityName(Type thisType, string fieldName) {
            var field = thisType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            if (field == null) {
                return null;
            }
            var attributes = field.GetCustomAttributes(typeof(UnitySerializedFieldAttribute), false);
            if (attributes.Length != 1) {
                return null;
            }
            var attr = (UnitySerializedFieldAttribute)attributes[0];
            return attr.UnityName;
        }

    }
}
