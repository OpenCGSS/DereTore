using System.Linq;

namespace DereTore.Applications.StarlightDirector.Entities.Extensions {
    public static class ScoreExtensions {

        public static CompiledScore Compile(this Score score) {
            var compiledScore = new CompiledScore();
            CompileTo(score, compiledScore);
            return compiledScore;
        }

        public static void CompileTo(this Score score, CompiledScore compiledScore) {
            var settings = score.Project.Settings;
            FlickGroupIDGenerator.Reset();
            var compiledNotes = compiledScore.Notes;
            compiledNotes.Clear();
            var barTimeStart = settings.StartTimeOffset;

            // Clear the GroupID caches.
            foreach (var bar in score.Bars) {
                foreach (var note in bar.Notes) {
                    note.GroupID = EntityID.Invalid;
                }
            }

            // We can also use Note.GetNoteHitTiming(), but that requires massive loops. Therefore we manually expand them here.
            foreach (var bar in score.Bars) {
                var anyVariantBpm = bar.Notes.Any(note => note.Type == NoteType.VariantBpm);
                double barTimeLength;
                if (anyVariantBpm) {
                    barTimeLength = CompileBarConstantBpm(bar, barTimeStart, compiledNotes);
                } else {
                    barTimeLength = CompileBarVariantBpm(bar, barTimeStart, compiledNotes);
                }
                barTimeStart += barTimeLength;
            }

            // The normal gaming notes.
            compiledNotes.Sort(CompiledNote.TimingComparison);
            var i = 3;
            foreach (var compiledNote in compiledNotes) {
                compiledNote.ID = i++;
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
            var songEndNote = new CompiledNote {
                ID = i++,
                Type = NoteType.MusicEnd,
                HitTiming = barTimeStart
            };
            compiledNotes.Add(songEndNote);
        }

        private static double CompileBarConstantBpm(Bar bar, double timeStart, InternalList<CompiledNote> compiledNotes) {
            var barTimeLength = bar.GetTimeLength();
            var totalGridCount = bar.GetTotalGridCount();
            // Sorting is for flick group generation. We have to assure that the start of each group is
            // processed first, at least in the group which it is in.
            bar.Notes.Sort(Note.TimingComparison);
            foreach (var note in bar.Notes) {
                if (note.IsSpecialNote) {
                    continue;
                }
                var compiledNote = new CompiledNote();
                compiledNotes.Add(compiledNote);
                SetCommonNoteProperties(note, compiledNote);
                CalculateGroupID(note, compiledNote);
                // Timing for constant BPM
                compiledNote.HitTiming = timeStart + barTimeLength * note.IndexInGrid / totalGridCount;
            }
            return barTimeLength;
        }

        private static double CompileBarVariantBpm(Bar bar, double timeStart, InternalList<CompiledNote> compiledNotes) {
            var startBpm = bar.GetStartBpm();
            var signature = bar.GetActualSignature();
            var totalGridCount = bar.GetTotalGridCount();
            var barTimeLength = bar.GetTimeLength();
            // This sorting is for flick group generation AND Variant BPM notes. See Note.TimingComparison for more details.
            bar.Notes.Sort(Note.TimingComparison);
            var lastBpm = startBpm;
            var lastVBIndex = 0;
            var lastVBTiming = 0d;

            foreach (var note in bar.Notes) {
                if (note.IsSpecialNote) {
                    continue;
                }
                var deltaGridCount = note.IndexInGrid - lastVBIndex;
                var timePerBeat = DirectorHelper.BpmToSeconds(lastBpm);
                if (note.Type == NoteType.VariantBpm) {
                    lastVBTiming = lastVBTiming + timePerBeat * signature * deltaGridCount / totalGridCount;
                    lastBpm = note.ExtraParams.NewBpm;
                    lastVBIndex = note.IndexInGrid;
                    continue;
                }
                var compiledNote = new CompiledNote();
                compiledNotes.Add(compiledNote);
                SetCommonNoteProperties(note, compiledNote);
                CalculateGroupID(note, compiledNote);

                // Yeah for the maths.
                var noteTimingInBar = lastVBTiming + timePerBeat * signature * deltaGridCount / totalGridCount;
                compiledNote.HitTiming = timeStart + noteTimingInBar;
            }
            return barTimeLength;
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
                    default:
                        break;
                }
            }
        }

        private static readonly IntegerIDGenerator FlickGroupIDGenerator = new IntegerIDGenerator(1);

    }
}
