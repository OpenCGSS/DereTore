using DereTore.ACB.Serialization;

namespace DereTore.Applications.AcbMaker.Cgss {
    [UtfTable("CueName")] // Optional. The 'Table' in 'CueNameTable' is ignored by default.
    public sealed class CueNameTable : UtfRowBase {

        [UtfField(0)]
        public string CueName;
        [UtfField(1)]
        public ushort CueIndex;

    }
}
