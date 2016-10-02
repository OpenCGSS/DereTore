using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DereTore.Applications.StarlightDirector.Components;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;

using NoteTuple = System.Tuple<DereTore.Applications.StarlightDirector.UI.Controls.ScoreNote, DereTore.Applications.StarlightDirector.UI.Controls.ScoreNote>;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    partial class ScoreEditor {

        private void ReloadScore(Score toBeSet) {
            // First, clean up the room before inviting guests. XD (You know where this sentense comes from.)
            // These are presentation layer of the program, just clear them, and let the GC do the rest of the work.
            // Clearing these objects will not affect the underlying model.
            RemoveScoreBars(ScoreBars, false, true);
            LineLayer.NoteRelations.Clear();
            UpdateMaximumScrollOffset();
            if (toBeSet == null) {
                return;
            }

            var temporaryMap = new Dictionary<Note, ScoreNote>();
            var allNotes = new List<Note>();
            // OK the fun part is here.
            foreach (var bar in toBeSet.Bars) {
                var scoreBar = AddScoreBar(null, false, bar);
                foreach (var note in bar.Notes) {
                    var scoreNote = AddScoreNote(scoreBar, note.PositionInGrid, note.FinishPosition, note);
                    temporaryMap.Add(note, scoreNote);
                    allNotes.Add(note);
                }
            }
            var processedNotes = new List<Note>();
            foreach (var note in allNotes) {
                if (!processedNotes.Contains(note)) {
                    ProcessNoteRelations(note, processedNotes, temporaryMap, LineLayer.NoteRelations);
                }
            }
            UpdateMaximumScrollOffset();
            UpdateBarTexts();
            var originalOffset = ScrollOffset;
            ScrollOffset = 0;
            if (originalOffset.Equals(ScrollOffset)) {
                RecalcEditorLayout();
            }
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
                if (note.IsSync) {
                    relations.Add(map[note], map[note.SyncTarget], NoteRelation.Sync);
                    waitingList.Enqueue(note.SyncTarget);
                }
                if (note.HasNextFlick) {
                    relations.Add(map[note], map[note.NextFlickNote], NoteRelation.Flick);
                    waitingList.Enqueue(note.NextFlickNote);
                }
                if (note.HasPrevFlick) {
                    relations.Add(map[note], map[note.PrevFlickNote], NoteRelation.Flick);
                    waitingList.Enqueue(note.PrevFlickNote);
                }
                if (note.IsHoldStart) {
                    relations.Add(map[note], map[note.HoldTarget], NoteRelation.Hold);
                    waitingList.Enqueue(note.HoldTarget);
                }
            }
        }

        private void RecalcEditorLayout() {
            ResizeBars();
            RepositionBars();
            RepositionNotes();
            RepositionLines();
        }

        private void ResizeBars() {
            var barLayerWidth = BarLayer.ActualWidth;
            foreach (var scoreBar in ScoreBars) {
                scoreBar.BarColumnWidth = barLayerWidth * (TrackCenterXPositions[4] - TrackCenterXPositions[0]);
            }
        }

        private void RepositionBars() {
            var barLayerWidth = BarLayer.ActualWidth;
            if (ScoreBars.Count == 0) {
                return;
            }
            var currentY = ScrollOffset;
            foreach (var scoreBar in ScoreBars) {
                Canvas.SetLeft(scoreBar, barLayerWidth * TrackCenterXPositions[0] - scoreBar.TextColumnWidth - scoreBar.SpaceColumnWidth);
                Canvas.SetTop(scoreBar, currentY);
                currentY += scoreBar.Height;
            }
        }

        private void RepositionNotes() {
            if (ScoreNotes.Count == 0) {
                return;
            }
            var scrollOffset = ScrollOffset;
            var noteLayerWidth = NoteLayer.ActualWidth;
            var barHeight = ScoreBars[0].Height;
            foreach (var scoreNote in ScoreNotes) {
                var note = scoreNote.Note;
                var bar = note.Bar;
                var baseY = scrollOffset + bar.Index * barHeight;
                var extraY = barHeight * note.PositionInGrid / bar.GetTotalGridCount();
                scoreNote.X = noteLayerWidth * TrackCenterXPositions[(int)note.FinishPosition - 1];
                scoreNote.Y = baseY + extraY;
            }
        }

        private void RepositionAvatars() {
            var avatars = Avatars;
            if (avatars == null || avatars.Length == 0) {
                return;
            }
            var width = NoteLayer.ActualWidth;
            var height = AvatarLayer.ActualHeight;
            var xOffset = AvatarLayer.TranslatePoint(new Point(), NoteLayer).X;
            for (var i = 0; i < 5; ++i) {
                var avatar = avatars[i];
                var x = TrackCenterXPositions[i] * width - avatar.ActualWidth / 2 - xOffset;
                var y = BaseLineYPosition * height - avatar.ActualHeight / 2;
                Canvas.SetLeft(avatar, x);
                Canvas.SetTop(avatar, y);
            }
        }

        private void RepositionAvatarLine() {
            var width = NoteLayer.ActualWidth;
            var height = AvatarLayer.ActualHeight;
            double x1, x2, y;
            var xOffset = AvatarLayer.TranslatePoint(new Point(), NoteLayer).X;
            y = height * BaseLineYPosition;
            x1 = width * (TrackCenterXPositions[0] - 0.075) - xOffset;
            x2 = width * (TrackCenterXPositions[4] + 0.075) - xOffset;
            AvatarLine.X1 = x1;
            AvatarLine.X2 = x2;
            AvatarLine.Y1 = AvatarLine.Y2 = y;
        }

        private void RepositionLines() {
            LineLayer.Width = NoteLayer.ActualWidth;
            LineLayer.Height = NoteLayer.ActualHeight;
            Canvas.SetTop(LineLayer, ScrollOffset);
            LineLayer.InvalidateVisual();
        }

        private Rect GetWorkingAreaRect() {
            var width = WorkingAreaClip.ActualWidth;
            var height = WorkingAreaClip.ActualHeight;
            return new Rect(WorkingAreaPadding, WorkingAreaPadding, width - WorkingAreaPadding * 2, height - WorkingAreaPadding * 2);
        }

        private void InitializeControls() {
            var avatars = new ScoreNote[5];
            for (var i = 0; i < 5; ++i) {
                var image = Application.Current.FindResource<ImageSource>($"CardAvatar{i + 1}");
                var avatar = new ScoreNote();
                avatar.Image = image;
                avatars[i] = avatar;
                AvatarLayer.Children.Add(avatar);
            }
            Avatars = avatars;
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
                    // The Reset() call is necessary.
                    note.Reset();
                    note.Bar.Notes.Remove(note);
                }
            }
            NoteLayer.Children.Remove(scoreNote);
            // TODO: Query if there is a need to do that.
            if (repositionLines) {
                RepositionLines();
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
                RepositionLines();
            }
            if (modifiesModel) {
                Project.IsChanged = true;
            }
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
                return scoreNote;
            }
            var barHeight = ScoreBars[0].Height;
            var baseY = ScrollOffset + bar.Index * barHeight;
            var extraY = barHeight * row / bar.GetTotalGridCount();
            scoreNote = new ScoreNote();
            Note note;
            if (dataTemplate != null) {
                note = dataTemplate;
            } else {
                note = bar.AddNote(MathHelper.NextRandomPositiveInt32());
                note.StartPosition = note.FinishPosition = (NotePosition)(column + 1);
                note.PositionInGrid = row;
            }
            scoreNote.Note = note;
            EditableScoreNotes.Add(scoreNote);
            NoteLayer.Children.Add(scoreNote);
            scoreNote.X = NoteLayer.ActualWidth * TrackCenterXPositions[column];
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
            scoreBar.ScoreBarHitTest += ScoreBar_ScoreBarHitTest;
            scoreBar.MouseDoubleClick += ScoreBar_MouseDoubleClick;
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
                UpdateMaximumScrollOffset();
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
            scoreBar.ScoreBarHitTest -= ScoreBar_ScoreBarHitTest;
            scoreBar.MouseDoubleClick -= ScoreBar_MouseDoubleClick;
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
                UpdateMaximumScrollOffset();
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
                UpdateMaximumScrollOffset();
            }
            if (modifiesModel) {
                Project.IsChanged = true;
            }
        }

        private ScoreNote AnyNoteExistOnPosition(int barIndex, int column, int row) {
            foreach (var scoreNote in ScoreNotes) {
                var note = scoreNote.Note;
                if (note.Bar.Index == barIndex && (int)note.FinishPosition == column + 1 && note.PositionInGrid == row) {
                    return scoreNote;
                }
            }
            return null;
        }

        private void UpdateBarTexts() {
            foreach (var scoreBar in ScoreBars) {
                scoreBar.UpdateBarIndexText();
                scoreBar.UpdateBpmText();
                scoreBar.UpdateBarTimeText();
            }
        }

        private void UpdateMaximumScrollOffset() {
            var scoreBars = ScoreBars;
            if (scoreBars.Count == 0) {
                MaximumScrollOffset = 0;
            } else {
                MaximumScrollOffset = scoreBars[0].Height * ScoreBars.Count;
            }
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

        private List<ScoreBar> EditableScoreBars { get; }

        private List<ScoreNote> EditableScoreNotes { get; }

        private ScoreNote[] Avatars { get; set; }

        private ScoreNote DraggingStartNote { get; set; }

        private ScoreNote DraggingEndNote { get; set; }

        private static readonly double[] TrackCenterXPositions = { 0.2, 0.35, 0.5, 0.65, 0.8 };
        //private static readonly double BaseLineYPosition = 1d / 6;
        private static readonly double BaseLineYPosition = 0.1;
        private static readonly double WorkingAreaPadding = 2;
        private static readonly double FutureTimeWindow = 1;
        private static readonly double PastTimeWindow = 0.2;
        // Then we know the bottom is <AvatarCenterY + (PastWindow / FutureWindow) * (AvatarCenterY - Ceiling))>.
        private static readonly double FutureNoteCeiling = 5d / 6;

    }
}
