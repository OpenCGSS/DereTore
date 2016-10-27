using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    partial class ScoreEditor {

        internal void UpdateBarTexts() {
            foreach (var scoreBar in ScoreBars) {
                scoreBar.UpdateBarIndexText();
                scoreBar.UpdateBarTimeText();
            }
        }

        private void ReloadScore(Score toBeSet) {
            // First, clean up the room before inviting guests. XD (You know where this sentense comes from.)
            // These are presentation layer of the program, just clear them, and let the GC do the rest of the work.
            // Clearing these objects will not affect the underlying model.
            RemoveScoreBars(ScoreBars, false, true);
            LineLayer.NoteRelations.Clear();
            while (SpecialScoreNotes.Count > 0) {
                RemoveSpecialNote(SpecialScoreNotes[0], false);
            }
            if (toBeSet == null) {
                return;
            }

            var temporaryMap = new Dictionary<Note, ScoreNote>();
            var allGamingNotes = new List<Note>();
            // OK the fun part is here.
            foreach (var bar in toBeSet.Bars) {
                var scoreBar = AddScoreBar(null, false, bar);
                foreach (var note in bar.Notes) {
                    if (!note.IsGamingNote) {
                        continue;
                    }
                    var scoreNote = AddScoreNote(scoreBar, note.IndexInGrid, note.FinishPosition, note);
                    temporaryMap.Add(note, scoreNote);
                    allGamingNotes.Add(note);
                }
            }
            var processedNotes = new List<Note>();
            foreach (var note in allGamingNotes) {
                if (!processedNotes.Contains(note)) {
                    ProcessNoteRelations(note, processedNotes, temporaryMap, LineLayer.NoteRelations);
                }
            }

            // Variant BPM indicators
            foreach (var note in toBeSet.Notes.Where(n => n.Type == NoteType.VariantBpm)) {
                var specialNote = AddSpecialNote(ScoreBars[note.Bar.Index], note.IndexInGrid, note.Type, note, false);
                specialNote.Note = note;
            }

            UpdateBarTexts();
            RecalcEditorLayout();
            Debug.Print("Done: ScoreEditor.ReloadScore().");
        }

        private static void ProcessNoteRelations(Note root, ICollection<Note> processedNotes, IDictionary<Note, ScoreNote> map, NoteRelationCollection relations) {
            var waitingList = new Queue<Note>();
            waitingList.Enqueue(root);
            while (waitingList.Count > 0) {
                var note = waitingList.Dequeue();
                if (processedNotes.Contains(note)) {
                    continue;
                }
                processedNotes.Add(note);
                if (note.HasPrevSync) {
                    if (!relations.ContainsPair(map[note], map[note.PrevSyncTarget])) {
                        relations.Add(map[note], map[note.PrevSyncTarget], NoteRelation.Sync);
                        waitingList.Enqueue(note.PrevSyncTarget);
                    }
                }
                if (note.HasNextSync) {
                    if (!relations.ContainsPair(map[note], map[note.NextSyncTarget])) {
                        relations.Add(map[note], map[note.NextSyncTarget], NoteRelation.Sync);
                        waitingList.Enqueue(note.NextSyncTarget);
                    }
                }
                if (note.HasNextFlick) {
                    if (!relations.ContainsPair(map[note], map[note.NextFlickNote])) {
                        relations.Add(map[note], map[note.NextFlickNote], NoteRelation.Flick);
                        waitingList.Enqueue(note.NextFlickNote);
                    }
                }
                if (note.HasPrevFlick) {
                    if (!relations.ContainsPair(map[note], map[note.PrevFlickNote])) {
                        relations.Add(map[note], map[note.PrevFlickNote], NoteRelation.Flick);
                        waitingList.Enqueue(note.PrevFlickNote);
                    }
                }
                if (note.IsHoldStart) {
                    if (!relations.ContainsPair(map[note], map[note.HoldTarget])) {
                        relations.Add(map[note], map[note.HoldTarget], NoteRelation.Hold);
                        waitingList.Enqueue(note.HoldTarget);
                    }
                }
            }
        }

        private void OnScoreGlobalSettingsChanged(object sender, EventArgs e) {
            UpdateBarTexts();
        }

        private static readonly double[] TrackCenterXPositions = { 0.2, 0.35, 0.5, 0.65, 0.8 };

    }
}
