using System;

namespace DereTore.Exchange.Archive.ACB.Serialization {
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class UtfTableAttribute : Attribute {

        public UtfTableAttribute(string name) {
            Name = name;
        }

        public string Name { get; }

    }
}
