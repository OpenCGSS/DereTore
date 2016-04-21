namespace DereTore.ACB {
    public sealed class CriField {

        public byte Type { get; internal set; }
        public string Name { get; internal set; }
        public object Value { get; internal set; }
        public ulong Offset { get; internal set; }
        public ulong Size { get; internal set; }

    }
}