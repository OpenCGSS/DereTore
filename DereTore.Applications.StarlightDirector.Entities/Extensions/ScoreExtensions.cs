using System.Linq;

namespace DereTore.Applications.StarlightDirector.Entities.Extensions {
    public static class ScoreExtensions {

        public static CompiledScore Compile(this Score score) {
            var compiledScore = new CompiledScore();
            CompileTo(score, compiledScore);
            return compiledScore;
        }

        public static void CompileTo(this Score score, CompiledScore compiledScore) {
            FlickGroupIDGenerator.Reset();
            var compiledNotes = compiledScore.Notes;
            compiledNotes.Clear();

            // Clear the GroupID caches.
            foreach (var bar in score.Bars) {
                foreach (var note in bar.Notes) {
                    note.GroupID = EntityID.Invalid;

                    var compiledNote = new CompiledNote();
                    SetCommonNoteProperties(note, compiledNote);
                    CalculateGroupID(note, compiledNote);
                    compiledNote.HitTiming = note.HitTiming;

                    compiledNotes.Add(compiledNote);
                }
            }
            
            // The normal gaming notes.
            var noteId = 3;
            foreach (var compiledNote in compiledNotes)
            {
                compiledNote.ID = noteId++;
            }

            // Special notes are added to their destined positions.
            var totalNoteCount = compiledNotes.Count;
            var scoreInfoNote = new CompiledNote {
                ID = 1,
                Type = NoteType.NoteCount,
                FlickType = totalNoteCount
            };
            var songStartNote = new CompiledNote {
                ID = 2,
                Type = NoteType.MusicStart
            };
            compiledNotes.Insert(0, scoreInfoNote);
            compiledNotes.Insert(1, songStartNote);

            var lastBar = score.Bars.Last();
            var songEndNote = new CompiledNote {
                ID = noteId,
                Type = NoteType.MusicEnd,
                HitTiming = lastBar.StartTime + lastBar.TimeLength
            };
            compiledNotes.Add(songEndNote);
        }

        private static void SetCommonNoteProperties(Note note, CompiledNote compiledNote) {
            compiledNote.Type = note.Type;
            compiledNote.StartPosition = note.StartPosition;
            compiledNote.FinishPosition = note.FinishPosition;
            compiledNote.FlickType = (int)note.FlickType;
            compiledNote.IsSync = note.IsSync;
        }

        private static void CalculateGroupID(Note note, CompiledNote compiledNote) {
            if (note.GroupID != EntityID.Invalid) {
                compiledNote.FlickGroupID = note.GroupID;
            } else {
                int groupID;
                FlickGroupModificationResult result;
                Note groupStart;
                if (!note.TryGetFlickGroupID(out result, out groupID, out groupStart)) {
                    // No need to set a group ID. E.g. the note is not a flick note.
                    return;
                }
                switch (result) {
                    case FlickGroupModificationResult.Reused:
                        note.GroupID = compiledNote.FlickGroupID = groupID;
                        break;
                    case FlickGroupModificationResult.CreationPending:
                        groupID = FlickGroupIDGenerator.Next();
                        groupStart.GroupID = note.GroupID = compiledNote.FlickGroupID = groupID;
                        break;
                }
            }
        }

        private static readonly IntegerIDGenerator FlickGroupIDGenerator = new IntegerIDGenerator(1);

    }
}
