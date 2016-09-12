using DereTore.ACB.Serialization;

namespace DereTore.Applications.AcbMaker.Cgss {
    public sealed class CommandTable : UtfRowBase {

        [UtfField(0)]
        public byte[] Command;

    }
}
