using System;

namespace DereTore.ACB.Serialization {
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class UtfTableAttribute : Attribute {

        public UtfTableAttribute(string name) {
            Name = name;
        }

        public string Name { get; }

    }
}
