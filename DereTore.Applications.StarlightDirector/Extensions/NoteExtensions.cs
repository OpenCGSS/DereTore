using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector.Extensions {
    public static class NoteExtensions {

        public static double GetHitTiming(this Note note) {
            var bar = note.Bar;
            var barStartTime = bar.GetStartTime();
            var signature = bar.GetActualSignature();
            var gridCountInBar = bar.GetActualGridPerSignature();
            var barLength = bar.GetLength();
            return barStartTime + barLength * (note.PositionInGrid / (double)(signature * gridCountInBar));
        }

    }
}
