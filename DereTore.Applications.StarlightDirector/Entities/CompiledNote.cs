using System;

namespace DereTore.Applications.StarlightDirector.Entities {
    public sealed class CompiledNote {

        public int ID { get; set; }

        public double HitTiming { get; set; }

        public NoteType Type { get; set; }

        public NotePosition StartPosition { get; set; }

        public NotePosition FinishPosition { get; set; }

        // The type is Int32 here because this field ('status') will serve other usages.
        // See note type 100 (score info).
        public int FlickType { get; set; }

        public bool IsSync { get; set; }

        public int FlickGroupID { get; set; }

        internal static readonly Comparison<CompiledNote> IDComparison = (n1, n2) => n1.ID.CompareTo(n2.ID);
        internal static readonly Comparison<CompiledNote> TimingComparison = (n1, n2) => n1.HitTiming.CompareTo(n2.HitTiming);

    }
}
