namespace StarlightDirector.Exchange.Deleste {
    internal sealed class DelesteBeatmapEntry {

        public int GroupID { get; set; }

        public int MeasureIndex { get; set; }

        public int FullLength { get; set; }

        public DelesteBasicNote[] Notes { get; set; }

        public double BPM { get; set; }

        public double Signature { get; set; }

        public bool IsCommand { get; set; }

        public DelesteCommand CommandType { get; set; }

        public object CommandParameter { get; set; }

    }
}
