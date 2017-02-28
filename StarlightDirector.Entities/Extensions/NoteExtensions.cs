using System.Collections.Generic;

namespace StarlightDirector.Entities.Extensions {
    public static class NoteExtensions {

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

        internal static bool TryGetFlickGroupID(this Note note, out FlickGroupModificationResult modificationResult, out int knownGroupID, out Note groupStart) {
            if ((!note.IsFlick && !note.IsSlide) || (note.IsHoldEnd && !note.HasNextFlickOrSlide)) {
                knownGroupID = EntityID.Invalid;
                modificationResult = FlickGroupModificationResult.Declined;
                groupStart = null;
                return false;
            }
            var groupItemCount = 0;
            var temp = note;
            // Compiler trick.
            groupStart = temp;
            while (temp != null) {
                groupStart = temp;
                temp = temp.PrevFlickOrSlideNote;
                ++groupItemCount;
            }
            temp = note;
            ++groupItemCount;
            while (temp.HasNextFlickOrSlide) {
                temp = temp.NextFlickOrSlideNote;
                ++groupItemCount;
            }
            if (groupItemCount < 2) {
                // Actually, the flick group is not fully filled. We should throw an exception.
                knownGroupID = EntityID.Invalid;
                modificationResult = FlickGroupModificationResult.Declined;
                return false;
            }
            if (groupStart.GroupID != EntityID.Invalid) {
                knownGroupID = groupStart.GroupID;
                modificationResult = FlickGroupModificationResult.Reused;
            } else {
                knownGroupID = EntityID.Invalid;
                modificationResult = FlickGroupModificationResult.CreationPending;
            }
            return true;
        }

    }
}
