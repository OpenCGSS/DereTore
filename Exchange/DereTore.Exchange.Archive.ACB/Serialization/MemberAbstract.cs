using System.Reflection;

namespace DereTore.Exchange.Archive.ACB.Serialization {
    internal sealed class MemberAbstract {

        internal MemberAbstract(FieldInfo fieldInfo, UtfFieldAttribute fieldAttribute, Afs2ArchiveAttribute archiveAttribute) {
            FieldInfo = fieldInfo;
            FieldAttribute = fieldAttribute;
            ArchiveAttribute = archiveAttribute;
        }

        public FieldInfo FieldInfo { get; }

        public UtfFieldAttribute FieldAttribute { get; }

        public Afs2ArchiveAttribute ArchiveAttribute { get; }

    }
}
