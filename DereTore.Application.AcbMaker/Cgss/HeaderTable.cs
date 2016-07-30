using DereTore.ACB.Serialization;

namespace DereTore.Application.AcbMaker.Cgss {
    public sealed class HeaderTable : UtfRowBase {

        [UtfField(0)]
        public uint FileIdentifier;
        [UtfField(1)]
        public uint Size;
        [UtfField(2)]
        public uint Version;
        [UtfField(3)]
        public byte Type;
        [UtfField(4)]
        public byte Target;
        [UtfField(5)]
        public byte[] AcfMd5Hash;
        [UtfField(6)]
        public byte CategoryExtension;
        [UtfField(7)]
        public CueTable[] CueTable;
        [UtfField(8)]
        public CueNameTable[] CueNameTable;
        [UtfField(9)]
        public WaveformTable[] WaveformTable;
        // These are empty tables. They are treated as empty data fields.
        [UtfField(10)]
        public byte[] AisacTable;
        [UtfField(11)]
        public byte[] GraphTable;
        [UtfField(12)]
        public byte[] GlobalAisacReferenceTable;
        [UtfField(13)]
        public byte[] AisacNameTable;
        [UtfField(14)]
        public SynthTable[] SynthTable;
        [UtfField(15)]
        public CommandTable[] CommandTable;
        [UtfField(16)]
        public TrackTable[] TrackTable;
        [UtfField(17)]
        public SequenceTable[] SequenceTable;
        [UtfField(18)]
        public byte[] AisacControlNameTable;
        [UtfField(19)]
        public byte[] AutoModulationTable;
        [UtfField(20)]
        public byte[] StreamAwbTocWorkOld;
        [UtfField(21)]
        [Afs2Archive]
        public byte[] AwbFile;
        [UtfField(22)]
        public string VersionString;
        [UtfField(23)]
        public byte[] CueLimitWorkTable;
        [UtfField(24)]
        public ushort NumCueLimitListWorks;
        [UtfField(25)]
        public ushort NumCueLimitNodeWorks;
        [UtfField(26)]
        public byte[] AcbGuid;
        [UtfField(27)]
        public byte[] StreamAwbHash;
        [UtfField(28)]
        public byte[] StreamAwbTocWork_Old;
        [UtfField(29)]
        public float AcbVolume;
        [UtfField(30)]
        public byte[] StringValueTable;
        [UtfField(31)]
        public byte[] OutsideLinkTable;
        [UtfField(32)]
        public byte[] BlockSequenceTable;
        [UtfField(33)]
        public byte[] BlockTable;
        [UtfField(34)]
        public string Name;
        [UtfField(35)]
        public byte CharacterEncodingType;
        [UtfField(36)]
        public byte[] EventTable;
        [UtfField(37)]
        public byte[] ActionTrackTable;
        [UtfField(38)]
        public AcfReferenceTable[] AcfReferenceTable;
        [UtfField(39)]
        public byte[] WaveformExtensionDataTable;
        [UtfField(40)]
        public byte[] BeatSyncInfoTable;
        [UtfField(41)]
        public byte CuePriorityType;
        [UtfField(42)]
        public ushort NumCueLimit;
        [UtfField(43)]
        public byte R17;
        [UtfField(44)]
        public byte R16;
        [UtfField(45)]
        public byte R15;
        [UtfField(46)]
        public byte R14;
        [UtfField(47)]
        public byte R13;
        [UtfField(48)]
        public byte R12;
        [UtfField(49)]
        public byte R11;
        [UtfField(50)]
        public byte R10;
        [UtfField(51)]
        public byte R9;
        [UtfField(52)]
        public byte R8;
        [UtfField(53)]
        public byte R7;
        [UtfField(54)]
        public byte R6;
        [UtfField(55)]
        public byte R5;
        [UtfField(56)]
        public byte R4;
        [UtfField(57)]
        public byte R3;
        [UtfField(58)]
        public byte R2;
        [UtfField(59)]
        public byte R1;
        [UtfField(60)]
        public byte R0;
        [UtfField(61)]
        public byte[] PaddingArea;
        [UtfField(62)]
        public byte[] StreamAwbTocWork;
        [UtfField(63)]
        public byte[] StreamAwbAfs2Header;

    }
}
