namespace DereTore.ACB {
    public sealed class CriAfs2File {

        public ushort CueId { get; internal set; }
        public long FileOffsetRaw { get; internal set; }
        public long FileOffsetByteAligned { get; internal set; }
        public long FileLength { get; internal set; }

        // Unused for now.
        public string FileName { get; internal set; }

    }
}