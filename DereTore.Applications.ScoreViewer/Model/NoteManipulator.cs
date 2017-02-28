using System;
using System.Linq;

namespace DereTore.Applications.ScoreViewer.Model {
    public static class NoteManipulator {

        private static void SortFlickNotes(this Score score, Note note, Note oldNote) {
            // Note == null means we just removed the note from score, and then requested sorting the notes.
            if (note != null) {
                EnsureNoteInScore(score, note);
            } else {
                note = oldNote;
            }
            if (!note.IsFlick) {
                return;
            }
            // Search for the flick group
            var flickGroupId = -1;
            if (note.IsFlick) {
                flickGroupId = note.GroupID;
            }
            if (flickGroupId < 0 && oldNote.IsFlick) {
                flickGroupId = oldNote.GroupID;
            }
            // < 0: truly an exception
            // == 0: this is a single flick note
            if (flickGroupId <= 0) {
                return;
            }
            var flickGroupNotes = (from n in score.Notes
                                   where n.IsFlick && n.GroupID == flickGroupId
                                   select n).ToList();
            // Another kind of exception?
            if (flickGroupNotes.Count < 2) {
                if (flickGroupNotes.Count > 0) {
                    var onlyNote = flickGroupNotes[0];
                    onlyNote.PrevFlickNote = onlyNote.NextFlickNote = null;
                    onlyNote.GroupID = 0;
                }
                return;
            }
            if (!flickGroupNotes.Contains(note)) {
                note.PrevFlickNote = note.NextFlickNote = null;
            }
            flickGroupNotes.Sort(TimeBasedNoteComparison);
            var index = 0;
            foreach (var flickNote in flickGroupNotes) {
                if (index == 0) {
                    if (flickNote.PrevFlickNote != null) {
                        flickNote.PrevFlickNote = null;
                    }
                }
                if (index == flickGroupNotes.Count - 1) {
                    if (flickNote.NextFlickNote != null) {
                        flickNote.NextFlickNote = null;
                    }
                }
                var nextFlickNote = index < flickGroupNotes.Count - 1 ? flickGroupNotes[index + 1] : null;
                if (flickNote.NextFlickNote != nextFlickNote) {
                    flickNote.NextFlickNote = nextFlickNote;
                    if (nextFlickNote != null) {
                        nextFlickNote.PrevFlickNote = flickNote;
                    }
                }
                ++index;
            }
            UpdateNoteIDs(score);
        }

        private static void UpdateNoteIDs(this Score score) {
            var notes = score.EditableNotes;
            notes.Sort(TimeBasedNoteComparison);
            var i = 1;
            foreach (var note in notes) {
                note.ID = i;
                ++i;
            }
        }

        private static void EnsureNoteInScore(Score score, Note note) {
            if (score == null) {
                throw new ArgumentNullException(nameof(score));
            }
            if (note == null) {
                throw new ArgumentNullException(nameof(note));
            }
            if (!score.Notes.Contains(note)) {
                throw new InvalidOperationException();
            }
        }

        private static readonly Comparison<Note> TimeBasedNoteComparison = (n1, n2) => n1.HitTiming.CompareTo(n2.HitTiming);

        public static readonly double NoteTimingStep = 0.1;

    }
}
