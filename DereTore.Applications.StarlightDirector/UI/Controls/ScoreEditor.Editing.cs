using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Entities.Extensions;
using DereTore.Applications.StarlightDirector.UI.Controls.Primitives;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    partial class ScoreEditor {

        public ScoreBar AppendScoreBar() {
            return AddScoreBar(null, true, null);
        }

        public ScoreBar[] AppendScoreBars(int count) {
            var added = new List<ScoreBar>();
            for (var i = 0; i < count; ++i) {
                added.Add(AddScoreBar(null, false, null));
            }
            UpdateBarTexts();
            RecalcEditorLayout();
            return added.ToArray();
        }

        public ScoreBar InsertScoreBar(ScoreBar before) {
            return AddScoreBar(before, true, null);
        }

        public ScoreBar[] InsertScoreBars(ScoreBar before, int count) {
            var added = new List<ScoreBar>();
            for (var i = 0; i < count; ++i) {
                added.Add(AddScoreBar(before, false, null));
            }
            UpdateBarTexts();
            RecalcEditorLayout();
            return added.ToArray();
        }

        public void RemoveScoreBar(ScoreBar scoreBar) {
            RemoveScoreBar(scoreBar, true, true);
        }

        public void RemoveScoreBars(IEnumerable<ScoreBar> scoreBars) {
            RemoveScoreBars(scoreBars, true, true);
        }

        public ScoreNote AddScoreNote(ScoreBar scoreBar, int row, NotePosition position) {
            return AddScoreNote(scoreBar, row, (int)position - 1, null);
        }

        public void RemoveScoreNote(ScoreNote scoreNote) {
            RemoveScoreNote(scoreNote, true, true);
        }

        public void RemoveScoreNotes(IEnumerable<ScoreNote> scoreNotes) {
            RemoveScoreNotes(scoreNotes, true, true);
        }

        public SpecialNotePointer AddSpecialNote(ScoreBar scoreBar, ScoreBarHitTestInfo info, NoteType type) {
            if (!info.IsInNextBar) {
                return AddSpecialNote(scoreBar, info.Row, type);
            }
            var nextBar = ScoreBars.FirstOrDefault(b => b.Bar.Index > scoreBar.Bar.Index);
            if (nextBar == null) {
                return null;
            }
            var point = scoreBar.TranslatePoint(info.HitPoint, nextBar);
            return AddSpecialNote(nextBar, nextBar.HitTest(point), type);
        }

        public SpecialNotePointer AddSpecialNote(ScoreBar scoreBar, int row, NoteType type) {
            return AddSpecialNote(scoreBar, row, type, null, true);
        }

        public bool RemoveSpecialNote(SpecialNotePointer specialNotePointer) {
            return RemoveSpecialNote(specialNotePointer, true);
        }

        private ScoreNote AddScoreNote(ScoreBar scoreBar, int row, NotePosition column, Note dataTemplate) {
            return AddScoreNote(scoreBar, row, (int)column - 1, dataTemplate);
        }

        private ScoreNote AddScoreNote(ScoreBar scoreBar, int row, int column, Note dataTemplate) {
            if (row < 0 || column < 0 || column >= 5) {
                return null;
            }
            var gridCount = scoreBar.Bar.GetTotalGridCount();
            if (row >= gridCount) {
                return null;
            }
            var bar = scoreBar.Bar;
            var scoreNote = AnyNoteExistOnPosition(bar.Index, column, row);
            if (scoreNote != null) {
                return null;
            }
            var barHeight = ScoreBars[0].Height;
            var baseY = -MinimumScrollOffset + bar.Index * barHeight;
            var extraY = barHeight * row / bar.GetTotalGridCount();
            scoreNote = new ScoreNote();
            Note note;
            if (dataTemplate != null) {
                note = dataTemplate;
            } else {
                note = bar.AddNote();
                note.StartPosition = note.FinishPosition = (NotePosition)(column + 1);
                note.IndexInGrid = row;
                note.FixSync();
            }
            scoreNote.Note = note;
            EditableScoreNotes.Add(scoreNote);
            NoteLayer.Children.Add(scoreNote);
            scoreNote.X = NoteLayer.ActualWidth * (TrackCenterXPositions[column] - TrackCenterXPositions[0]) / (TrackCenterXPositions[4] - TrackCenterXPositions[0]);
            scoreNote.Y = baseY + extraY;
            scoreNote.MouseDown += ScoreNote_MouseDown;
            scoreNote.MouseUp += ScoreNote_MouseUp;
            scoreNote.MouseDoubleClick += ScoreNote_MouseDoubleClick;
            if (dataTemplate == null) {
                Project.IsChanged = true;
            }
            return scoreNote;
        }

        private ScoreNote AddScoreNote(ScoreBar scoreBar, ScoreBarHitTestInfo info, Note dataTemplate) {
            if (!info.IsValid || info.Row < 0 || info.Column < 0) {
                if (!info.IsInNextBar) {
                    return null;
                }
                var nextBar = ScoreBars.FirstOrDefault(b => b.Bar.Index > scoreBar.Bar.Index);
                if (nextBar == null) {
                    return null;
                }
                var point = scoreBar.TranslatePoint(info.HitPoint, nextBar);
                return AddScoreNote(nextBar, nextBar.HitTest(point), dataTemplate);
            }
            return AddScoreNote(scoreBar, info.Row, info.Column, dataTemplate);
        }

        private void RemoveScoreNote(ScoreNote scoreNote, bool modifiesModel, bool repositionLines) {
            if (!ScoreNotes.Contains(scoreNote)) {
                throw new ArgumentException("Invalid ScoreNote.", nameof(scoreNote));
            }
            scoreNote.MouseDown -= ScoreNote_MouseDown;
            scoreNote.MouseUp -= ScoreNote_MouseUp;
            scoreNote.MouseDoubleClick -= ScoreNote_MouseDoubleClick;
            scoreNote.ContextMenu = null;
            EditableScoreNotes.Remove(scoreNote);
            LineLayer.NoteRelations.RemoveAll(scoreNote);
            if (modifiesModel) {
                var note = scoreNote.Note;
                if (Score.Bars.Contains(note.Bar)) {
                    // Remove note from sync group
                    // See Note.RemoveSync()
                    if (note.HasPrevSync && note.HasNextSync) {
                        var prevNote = note.PrevSyncTarget;
                        var nextNote = note.NextSyncTarget;
                        var prevScoreNote = FindScoreNote(prevNote);
                        var nextScoreNote = FindScoreNote(nextNote);
                        LineLayer.NoteRelations.Add(prevScoreNote, nextScoreNote, NoteRelation.Sync);
                    }
                    note.RemoveSync();
                    // The Reset() call is necessary.
                    note.Reset();
                    note.Bar.RemoveNote(note);
                }
            }
            NoteLayer.Children.Remove(scoreNote);
            // TODO: Query if there is a need to do that.
            if (repositionLines) {
                RepositionLineLayer();
            }
            if (modifiesModel) {
                Project.IsChanged = true;
            }
        }

        private void RemoveScoreNotes(IEnumerable<ScoreNote> scoreNotes, bool modifiesModel, bool recalcLayout) {
            // Avoid 'the collection has been modified' exception.
            var backup = scoreNotes.ToArray();
            foreach (var scoreNote in backup) {
                RemoveScoreNote(scoreNote, modifiesModel, false);
            }
            if (recalcLayout) {
                RepositionLineLayer();
            }
            if (modifiesModel) {
                Project.IsChanged = true;
            }
        }

        private ScoreBar AddScoreBar(ScoreBar before, bool recalculateLayout, Bar dataTemplate) {
            var project = Project;
            Debug.Assert(project != null, "project != null");
            var score = Score;
            var bar = dataTemplate ?? (before == null ? score.AddBar() : score.InsertBar(before.Bar.Index));
            if (bar == null) {
                return null;
            }
            var scoreBar = new ScoreBar();
            scoreBar.Bar = bar;
            if (ScoreBars.Count == 0) {
                scoreBar.Height = ScoreBar.DefaultHeight;
            } else {
                scoreBar.Height = ScoreBars[0].Height;
            }
            scoreBar.MouseUp += ScoreBar_MouseUp;
            scoreBar.MouseDown += ScoreBar_MouseDown;
            if (before == null) {
                BarLayer.Children.Add(scoreBar);
                EditableScoreBars.Add(scoreBar);
            } else {
                BarLayer.Children.Add(scoreBar);
                EditableScoreBars.Insert(ScoreBars.IndexOf(before), scoreBar);
            }
            if (recalculateLayout) {
                UpdateBarTexts();
                RecalcEditorLayout();
            }
            if (dataTemplate == null) {
                Project.IsChanged = true;
            }
            return scoreBar;
        }

        private void RemoveScoreBar(ScoreBar scoreBar, bool modifiesModel, bool recalcLayout) {
            if (!ScoreBars.Contains(scoreBar)) {
                throw new ArgumentException("Invalid ScoreBar.", nameof(scoreBar));
            }
            scoreBar.MouseUp -= ScoreBar_MouseUp;
            scoreBar.MouseDown -= ScoreBar_MouseDown;
            if (modifiesModel) {
                Score.RemoveBarAt(scoreBar.Bar.Index);
            }
            EditableScoreBars.Remove(scoreBar);
            BarLayer.Children.Remove(scoreBar);
            TrimScoreNotes(scoreBar, modifiesModel);
            if (recalcLayout) {
                UpdateBarTexts();
                RecalcEditorLayout();
            }
            if (modifiesModel) {
                Project.IsChanged = true;
            }
        }

        private void RemoveScoreBars(IEnumerable<ScoreBar> scoreBars, bool modifiesModel, bool recalcLayout) {
            var backup = scoreBars.ToArray();
            foreach (var scoreBar in backup) {
                RemoveScoreBar(scoreBar, modifiesModel, false);
            }
            if (recalcLayout) {
                UpdateBarTexts();
                RecalcEditorLayout();
            }
            if (modifiesModel) {
                Project.IsChanged = true;
            }
        }

        private SpecialNotePointer AddSpecialNote(ScoreBar scoreBar, int row, NoteType type, Note dataTemplate, bool updateBarText) {
            if (!Note.IsTypeSpecial(type)) {
                throw new ArgumentOutOfRangeException(nameof(type));
            }
            var existingNote = scoreBar.Bar.Notes.SingleOrDefault(n => n.Type == type && n.IndexInGrid == row);
            if (existingNote != null) {
                if (dataTemplate == null) {
                    // Manual editing, not from a ReloadScore() call.
                    return SpecialScoreNotes.First(sn => sn.Note.Equals(existingNote));
                }
            }
            var specialNotePointer = new SpecialNotePointer();
            EditableSpecialScoreNotes.Add(specialNotePointer);
            SpecialNoteLayer.Children.Add(specialNotePointer);
            var bar = scoreBar.Bar;
            Note note;
            if (dataTemplate == null) {
                note = bar.AddNote();
                note.IndexInGrid = row;
                note.SetSpecialType(type);
                note.ExtraParams = new NoteExtraParams {
                    Note = note
                };
            } else {
                // We assume that this *is* a special note. After all this method is private.
                note = dataTemplate;
            }
            specialNotePointer.Note = note;
            var barHeight = ScoreBars[0].Height;
            var baseY = -MinimumScrollOffset + bar.Index * barHeight;
            var extraY = barHeight * row / bar.GetTotalGridCount();
            specialNotePointer.Y = baseY + extraY;
            Project.IsChanged = true;
            if (updateBarText) {
                UpdateBarTexts();
            }
            return specialNotePointer;
        }

        private bool RemoveSpecialNote(SpecialNotePointer specialNotePointer, bool updateBarText) {
            var exists = SpecialScoreNotes.Contains(specialNotePointer);
            if (!exists) {
                return false;
            }
            var note = specialNotePointer.Note;
            note.Bar.RemoveNote(note);
            SpecialNoteLayer.Children.Remove(specialNotePointer);
            Project.IsChanged = true;
            var b = EditableSpecialScoreNotes.Remove(specialNotePointer);
            if (updateBarText) {
                UpdateBarTexts();
            }
            return b;
        }

    }
}
