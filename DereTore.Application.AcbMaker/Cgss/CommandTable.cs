using DereTore.ACB.Serialization;

namespace DereTore.Application.AcbMaker.Cgss {
    public sealed class CommandTable : UtfRowBase {

        [UtfField(0)]
        public byte[] Command;

    }
}
