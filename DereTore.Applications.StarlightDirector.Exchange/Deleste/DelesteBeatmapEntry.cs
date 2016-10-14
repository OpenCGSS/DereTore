namespace DereTore.Applications.StarlightDirector.Exchange.Deleste {
    internal sealed class DelesteBeatmapEntry {

        public int GroupID { get; set; }

        public int MeasureIndex { get; set; }

        public int FullLength { get; set; }

        public DelesteBasicNote[] Notes { get; set; }

    }
}
