using System;
using System.Collections.Generic;
using System.Linq;

namespace DereTore.Applications.ScoreViewer.Model {
    public static class NoteManipulator {

        public static void AddNote(this Score score, Note note) {
            int newIndex, newId;
            foreach (var adjustItem in FindNotesAfter(score, note, out newIndex, out newId)) {
                ++adjustItem.Id;
            }
            var specialCase = JudgeSpecialCase(score, newIndex);
            note.Id = newId;
            var notes = score.EditableNotes;
            switch (specialCase) {
                case SpecialCase.ResultIsEmpty:
                    notes.Add(note);
                    break;
                default:
                    notes.Insert(newIndex, note);
                    break;
            }
            score.RaiseScoreChanged(score, new ScoreChangedEventArgs(ScoreChangeReason.Adding, note));
            // No need to sort the flick notes, since the new note is always a tap note.
        }

        public static void EditNote(this Score score, Note note, Note newValue) {
            EnsureNoteInScore(score, note);
            var oldNote = note.Clone();
            // ATTENTION! Changing the time of the note may cause reordering of flicking, if the note is a flick note.
            SortFlickNotes(score, note, oldNote);
            note.CopyFrom(newValue);
            UpdateNoteIDs(score);
            UpdateSyncAndHoldStatus(note, newValue);
            score.RaiseScoreChanged(score, new ScoreChangedEventArgs(ScoreChangeReason.Modifying, note));
        }

        public static void RemoveNote(this Score score, Note note) {
            foreach (var adjustItem in FindNotesAfter(score, note)) {
                --adjustItem.Id;
            }
            var notes = score.EditableNotes;
            notes.Remove(note);
            // No need to sort, calculate group IDs or do anything else. I would say the code from the Cygames guys may also be like this,
            // otherwise what is the explanation for those tons of ungrouped flick notes in song_3034 (Hotel Moonside)?
            SortFlickNotes(score, null, note);
            UpdateSyncAndHoldStatus(note, null);
            score.RaiseScoreChanged(score, new ScoreChangedEventArgs(ScoreChangeReason.Removing, note));
        }

        public static void MakeSync(this Score score, Note note, Note syncWith, MakeSyncBasis syncBasis) {
            EnsureNoteInScore(score, note);
            EnsureNoteInScore(score, syncWith);
            if (note.IsSync || syncWith.IsSync) {
                throw new InvalidOperationException("At least one of the two notes is already synced.");
            }
            if (!note.HitTiming.Equals(syncWith.HitTiming)) {
                if (syncBasis == MakeSyncBasis.None) {
                    throw new InvalidOperationException("The time of notes are not the same.");
                }
                switch (syncBasis) {
                    case MakeSyncBasis.SelectedNote:
                        ResetTimingTo(score, syncWith, note.HitTiming);
                        break;
                    case MakeSyncBasis.SyncPair:
                        ResetTimingTo(score, note, syncWith.HitTiming);
                        break;
                }
            }
            note.IsSync = syncWith.IsSync = true;
            note.SyncPairNote = syncWith;
            syncWith.SyncPairNote = note;
        }

        public static void MakeHolding(this Score score, Note note, Note holdTo) {
            EnsureNoteInScore(score, note);
            EnsureNoteInScore(score, holdTo);
            throw new NotImplementedException();
        }

        public static void MakeFlick(this Score score, Note note, Note groupMember) {
            EnsureNoteInScore(score, note);
            EnsureNoteInScore(score, groupMember);
            // ATTENTION! Changing the time of the note may cause reordering of flicking, if the note is a flick note.
            var oldNote1 = note.Clone();
            var oldNote2 = groupMember.Clone();
            SortFlickNotes(score, note, oldNote1);
            SortFlickNotes(score, groupMember, oldNote2);
            throw new NotImplementedException();
        }

        public static void ResetToTap(this Score score, Note note) {
            EnsureNoteInScore(score, note);
            var oldNote = note.Clone();
            // ATTENTION! Changing the time of the note may cause reordering of flicking, if the note is a flick note.
            SortFlickNotes(score, note, oldNote);
            throw new NotImplementedException();
        }

        public static void ResetTimingTo(this Score score, Note note, double timing) {
            var temp = note.Clone();
            temp.HitTiming = timing;
            EditNote(score, note, temp);
        }

        public static void InitializeAsTap(this Note note) {
            note.Type = NoteType.TapOrFlick;
        }

        public static void AddTiming(this Score score, Note note) {
            ResetTimingTo(score, note, GetAddTimingResult(note));
        }

        public static void SubtractTiming(this Score score, Note note) {
            var newTiming = GetSubtractTimingResult(note);
            if (!newTiming.Equals(note.HitTiming)) {
                ResetTimingTo(score, note, newTiming);
            }
        }

        public static double GetAddTimingResult(this Note note) {
            return note.HitTiming + NoteTimingStep;
        }

        public static double GetSubtractTimingResult(this Note note) {
            var timing = note.HitTiming - NoteTimingStep;
            if (timing < 0) {
                timing = 0;
            }
            return timing;
        }

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
                flickGroupId = note.FlickGroupId;
            }
            if (flickGroupId < 0 && oldNote.IsFlick) {
                flickGroupId = oldNote.FlickGroupId;
            }
            // < 0: truly an exception
            // == 0: this is a single flick note
            if (flickGroupId <= 0) {
                return;
            }
            var flickGroupNotes = (from n in score.Notes
                                   where n.IsFlick && n.FlickGroupId == flickGroupId
                                   select n).ToList();
            // Another kind of exception?
            if (flickGroupNotes.Count < 2) {
                if (flickGroupNotes.Count > 0) {
                    var onlyNote = flickGroupNotes[0];
                    onlyNote.PrevFlickNote = onlyNote.NextFlickNote = null;
                    onlyNote.FlickGroupId = 0;
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
                note.Id = i;
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

        private static IEnumerable<Note> FindNotesAfter(this Score score, Note note, out int newIndex, out int newId) {
            var notes = score.Notes;
            // pivot is the first note that will be moved backwards.
            var pivot = notes.FirstOrDefault(n => n.HitTiming >= note.HitTiming);
            if (pivot == null) {
                newIndex = 0;
                newId = 0;
            } else {
                newIndex = notes.IndexOf(pivot);
                newId = pivot.Id;
            }
            var pivotId = newId;
            var notesWillBeAdjusted = notes.Where(n => n.Id >= pivotId);
            return notesWillBeAdjusted;
        }

        private static IEnumerable<Note> FindNotesAfter(this Score score, Note note) {
            int newIndex, newId;
            return FindNotesAfter(score, note, out newIndex, out newId);
        }

        private static SpecialCase JudgeSpecialCase(Score score, int newIndex) {
            SpecialCase specialCase;
            if (newIndex < 0) {
                specialCase = SpecialCase.ResultIsEntireSeries;
            } else if (newIndex == score.Notes.Count - 1) {
                specialCase = SpecialCase.ResultIsEmpty;
            } else {
                specialCase = SpecialCase.None;
            }
            return specialCase;
        }

        private static Note GetNoteItem(Score score, int newIndex) {
            return score.Notes[newIndex];
        }

        private static void UpdateSyncAndHoldStatus(Note note, Note newValue) {
            if (note.IsSync && note.SyncPairNote != null) {
                var syncNote = note.SyncPairNote;
                if (newValue == null) {
                    // Removing
                    syncNote.IsSync = false;
                    syncNote.SyncPairNote = null;
                } else {
                    if (!note.HitTiming.Equals(syncNote.HitTiming)) {
                        note.IsSync = syncNote.IsSync = false;
                        note.SyncPairNote = syncNote.SyncPairNote = null;
                    }
                }
            }
            if (note.IsHold) {
                Note holdNote;
                if (newValue == null) {
                    if (note.HasNextHold) {
                        holdNote = note.NextHoldNote;
                        holdNote.Type = NoteType.TapOrFlick;
                        holdNote.PrevHoldNote = null;
                    } else if (note.HasPrevHold) {
                        holdNote = note.PrevHoldNote;
                        holdNote.Type = NoteType.TapOrFlick;
                        holdNote.NextHoldNote = null;
                    }
                    note.Type = NoteType.TapOrFlick;
                } else {
                    if (note.HasNextHold) {
                        holdNote = note.NextHoldNote;
                        holdNote.StartPosition = newValue.StartPosition;
                        holdNote.FinishPosition = newValue.FinishPosition;
                        if (note.HitTiming > note.NextHoldNote.HitTiming) {
                            note.PrevHoldNote = note.NextHoldNote;
                            note.NextHoldNote = null;
                            note.PrevHoldNote.NextHoldNote = note;
                            note.PrevHoldNote.PrevHoldNote = null;
                        }
                    } else if (note.HasPrevHold) {
                        holdNote = note.PrevHoldNote;
                        newValue.StartPosition = holdNote.StartPosition;
                        newValue.FinishPosition = holdNote.FinishPosition;
                        if (note.HitTiming < note.PrevHoldNote.HitTiming) {
                            note.NextHoldNote = note.PrevHoldNote;
                            note.PrevHoldNote = null;
                            note.NextHoldNote.PrevHoldNote = note;
                            note.NextFlickNote.NextHoldNote = null;
                        }
                    }
                }
            }
        }

        private static readonly Comparison<Note> TimeBasedNoteComparison = (n1, n2) => n1.HitTiming.CompareTo(n2.HitTiming);

        public static readonly double NoteTimingStep = 0.1f;

        private enum SpecialCase {
            None,
            ResultIsEntireSeries,
            ResultIsEmpty
        }
    }
}
