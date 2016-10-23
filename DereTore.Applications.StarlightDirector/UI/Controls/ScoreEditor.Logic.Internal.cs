using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Entities.Extensions;
using DereTore.Applications.StarlightDirector.Extensions;

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
            NoteIDs.ExistingIDs.Clear();
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
                    NoteIDs.ExistingIDs.Add(note.ID);
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

        private void RecalcEditorLayout() {
            ResizeBars();
            RepositionBars();
            RepositionNotes();
            RepositionSpecialNotes();
            RepositionLineLayer();
            ResizeEditorHeight();
        }

        private void ResizeEditorHeight() {
            var height = -MinimumScrollOffset;
            if (ScoreBars.Count > 0) {
                height += ScoreBars.Sum(scoreBar => scoreBar.Height);
                height += ScoreBars[ScoreBars.Count - 1].GridStrokeThickness;
            }
            Height = height;
        }

        private void ResizeBars() {
            var barLayerWidth = BarLayer.ActualWidth;
            foreach (var scoreBar in ScoreBars) {
                scoreBar.BarColumnWidth = barLayerWidth;
            }
        }

        private void RepositionBars() {
            if (ScoreBars.Count == 0) {
                return;
            }
            var currentY = -MinimumScrollOffset;
            foreach (var scoreBar in ScoreBars) {
                Canvas.SetLeft(scoreBar, -scoreBar.TextColumnWidth - scoreBar.SpaceColumnWidth);
                Canvas.SetTop(scoreBar, currentY);
                currentY += scoreBar.Height;
            }
        }

        private void RepositionNotes() {
            if (ScoreNotes.Count == 0) {
                return;
            }
            var scrollOffset = -MinimumScrollOffset;
            var noteLayerWidth = NoteLayer.ActualWidth;
            var barHeight = ScoreBars[0].Height;
            foreach (var scoreNote in ScoreNotes) {
                var note = scoreNote.Note;
                var bar = note.Bar;
                var baseY = scrollOffset + bar.Index * barHeight;
                var extraY = barHeight * note.IndexInGrid / bar.GetTotalGridCount();
                scoreNote.X = noteLayerWidth * (TrackCenterXPositions[note.IndexInTrack] - TrackCenterXPositions[0]) / (TrackCenterXPositions[4] - TrackCenterXPositions[0]);
                scoreNote.Y = baseY + extraY;
            }
        }

        private void RepositionSpecialNotes() {
            if (SpecialScoreNotes.Count == 0 || ScoreBars.Count == 0) {
                return;
            }
            var scrollOffset = -MinimumScrollOffset;
            var barHeight = ScoreBars[0].Height;
            foreach (var specialNotePointer in SpecialScoreNotes) {
                var note = specialNotePointer.Note;
                var bar = note.Bar;
                var baseY = scrollOffset + bar.Index * barHeight;
                var extraY = barHeight * note.IndexInGrid / bar.GetTotalGridCount();
                specialNotePointer.Y = baseY + extraY;
            }
        }

        private void RepositionLineLayer() {
            LineLayer.Width = NoteLayer.ActualWidth;
            LineLayer.Height = NoteLayer.ActualHeight;
            LineLayer.InvalidateVisual();
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
                    NoteIDs.ExistingIDs.Remove(note.ID);
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
            NoteIDs.ExistingIDs.Remove(note.ID);
            SpecialNoteLayer.Children.Remove(specialNotePointer);
            Project.IsChanged = true;
            var b = EditableSpecialScoreNotes.Remove(specialNotePointer);
            if (updateBarText) {
                UpdateBarTexts();
            }
            return b;
        }

        private ScoreNote FindScoreNote(Note note) {
            return (from sn in ScoreNotes
                    where sn.Note == note
                    select sn).FirstOrDefault();
        }

        private ScoreNote AnyNoteExistOnPosition(int barIndex, int column, int row) {
            return (from scoreNote in ScoreNotes
                    let note = scoreNote.Note
                    where note.Bar.Index == barIndex && (int)note.FinishPosition == column + 1 && note.IndexInGrid == row
                    select scoreNote
                    ).FirstOrDefault();
        }

        private ScoreBar GetScoreBarGeomInfoForZooming(Point pointRelativeToThis, out double heightPercentage, out double height) {
            heightPercentage = 0;
            height = 0;
            var pt = pointRelativeToThis;
            var hit = VisualTreeHelper.HitTest(BarLayer, pt);
            var scoreBar = (hit?.VisualHit as FrameworkElement)?.FindVisualParent<ScoreBar>();
            if (scoreBar == null) {
                return null;
            }
            pt = TranslatePoint(pt, scoreBar);
            height = scoreBar.Height;
            heightPercentage = pt.Y / height;
            return scoreBar;
        }

        private void TrimScoreNotes(ScoreBar willBeDeleted, bool modifiesModel) {
            // Reposition after calling this function.
            var bar = willBeDeleted.Bar;
            Func<ScoreNote, bool> matchFunc = scoreNote => scoreNote.Note.Bar == bar;
            var processing = ScoreNotes.Where(matchFunc).ToArray();
            foreach (var scoreNote in processing) {
                RemoveScoreNote(scoreNote, modifiesModel, false);
            }
        }

        private void ZoomOut(Point? specifiedPoint) {
            double heightPercentage, scoreBarHeight;
            var point = specifiedPoint ?? Mouse.GetPosition(this);
            var originalScoreBar = GetScoreBarGeomInfoForZooming(point, out heightPercentage, out scoreBarHeight);
            foreach (var scoreBar in ScoreBars) {
                scoreBar.ZoomOut();
            }
            RecalcEditorLayout();
            if (originalScoreBar != null && ScrollViewer != null) {
                point = TranslatePoint(point, ScrollViewer);
                var newVertical = (ScoreBars.Count - originalScoreBar.Bar.Index - 1) * originalScoreBar.Height + originalScoreBar.Height * (1 - heightPercentage) - point.Y;
                ScrollViewer.ScrollToVerticalOffset(newVertical);
            }
        }

        private void ZoomIn(Point? specifiedPoint) {
            double heightPercentage, scoreBarHeight;
            var point = specifiedPoint ?? Mouse.GetPosition(this);
            var originalScoreBar = GetScoreBarGeomInfoForZooming(point, out heightPercentage, out scoreBarHeight);
            foreach (var scoreBar in ScoreBars) {
                scoreBar.ZoomIn();
            }
            RecalcEditorLayout();
            if (originalScoreBar != null && ScrollViewer != null) {
                point = TranslatePoint(point, ScrollViewer);
                var newVertical = (ScoreBars.Count - originalScoreBar.Bar.Index - 1) * originalScoreBar.Height + originalScoreBar.Height * (1 - heightPercentage) - point.Y;
                ScrollViewer.ScrollToVerticalOffset(newVertical);
            }
        }

        private void OnScoreGlobalSettingsChanged(object sender, EventArgs e) {
            UpdateBarTexts();
        }

        private List<ScoreBar> EditableScoreBars { get; }

        private List<ScoreNote> EditableScoreNotes { get; }

        private List<SpecialNotePointer> EditableSpecialScoreNotes { get; }

        private ScoreNote DraggingStartNote { get; set; }

        private ScoreNote DraggingEndNote { get; set; }

        private static readonly double[] TrackCenterXPositions = { 0.2, 0.35, 0.5, 0.65, 0.8 };

    }
}
