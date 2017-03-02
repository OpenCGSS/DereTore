using DereTore.Exchange.Archive.ACB.Serialization;

namespace DereTore.Apps.AcbMaker.Cgss {
    public sealed class CommandTable : UtfRowBase {

        [UtfField(0)]
        public byte[] Command;

    }
}
