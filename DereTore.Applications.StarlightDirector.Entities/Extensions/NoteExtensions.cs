using System.Collections.Generic;

namespace DereTore.Applications.StarlightDirector.Entities.Extensions {
    public static class NoteExtensions {

        public static double GetHitTiming(this Note note) {
            var bar = note.Bar;
            var barStartTime = bar.GetStartTime();
            var signature = bar.GetActualSignature();
            var gridCountInBar = bar.GetActualGridPerSignature();
            var barLength = bar.GetLength();
            return barStartTime + barLength * (note.IndexInGrid / (double)(signature * gridCountInBar));
        }

        public static Note GetFirstNoteBetween(this IEnumerable<Note> notes, Note n1, Note n2) {
            var first = n1 < n2 ? n1 : n2;
            var second = first.Equals(n1) ? n2 : n1;
            foreach (var n in notes) {
                if (n.Equals(first) || n.Equals(second)) {
                    continue;
                }
                if (n.FinishPosition != first.FinishPosition || first.Bar.Index > n.Bar.Index || n.Bar.Index > second.Bar.Index) {
                    continue;
                }
                if (first.Bar.Index == second.Bar.Index) {
                    if (first.IndexInGrid <= n.IndexInGrid && n.IndexInGrid <= second.IndexInGrid) {
                        return n;
                    }
                } else {
                    if (first.Bar.Index == n.Bar.Index) {
                        if (first.IndexInGrid <= n.IndexInGrid) {
                            return n;
                        }
                    } else if (second.Bar.Index == n.Bar.Index) {
                        if (n.IndexInGrid <= second.IndexInGrid) {
                            return n;
                        }
                    } else {
                        return n;
                    }
                }
            }
            return null;
        }

        public static bool AnyNoteBetween(this IEnumerable<Note> notes, Note start, Note end) {
            return GetFirstNoteBetween(notes, start, end) != null;
        }

    }
}
