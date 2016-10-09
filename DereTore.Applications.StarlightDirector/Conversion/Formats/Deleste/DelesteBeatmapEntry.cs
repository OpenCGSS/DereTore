namespace DereTore.Applications.StarlightDirector.Conversion.Formats.Deleste {
    internal sealed class DelesteBeatmapEntry {

        public int GroupID { get; set; }

        public int MeasureIndex { get; set; }

        public int FullLength { get; set; }

        public DelesteBasicNote[] Notes { get; set; }

    }
}
