using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.Components;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    partial class ScoreEditor {

        private void ReloadScore() {
            Debug.Print("Not implemented: ScoreEditor.ReloadScore().");
        }

        private void RecalcEditorLayout() {
            ResizeBars();
            ResizeNotes();
            RepositionBars();
            RepositionNotes();
            RepositionLines();
        }

        private void ResizeBars() {
            foreach (var scoreBar in ScoreBars) {
                scoreBar.BarColumnWidth = BarLayer.ActualWidth * (TrackCenterXPositions[4] - TrackCenterXPositions[0]);
            }
        }

        private void RepositionBars() {
            var barLayer = BarLayer;
            //barLayer.Children.Clear();
            if (ScoreBars.Count == 0) {
                return;
            }
            foreach (var scoreBar in ScoreBars) {
                Canvas.SetLeft(scoreBar, BarLayer.ActualWidth * TrackCenterXPositions[0] - scoreBar.TextColumnWidth - scoreBar.SpaceColumnWidth);
                Canvas.SetTop(scoreBar, ScrollOffset + scoreBar.Bar.Index * BarHeight);
            }
            var score = Score;
            if (score == null) {
                return;
            }
            //throw new NotImplementedException();
        }

        private void ResizeNotes() {
        }

        private void RepositionNotes() {
            if (ScoreNotes.Count == 0) {
                return;
            }
            foreach (var scoreNote in ScoreNotes) {
                var note = scoreNote.Note;
                var bar = note.Bar;
                var baseY = ScrollOffset + bar.Index * BarHeight;
                var extraY = BarHeight * note.PositionInGrid / bar.GetTotalGridCount();
                //Canvas.SetTop(scoreNote, baseY + extraY - scoreNote.Radius);
                //Canvas.SetLeft(scoreNote, NoteLayer.ActualWidth * TrackCenterXPositions[(int)note.FinishPosition - 1] - scoreNote.Radius);
                scoreNote.X = NoteLayer.ActualWidth * TrackCenterXPositions[(int)note.FinishPosition - 1];
                scoreNote.Y = baseY + extraY;
            }
            //throw new NotImplementedException();
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
                avatar.Radius = NoteRadius;
                avatar.Image = image;
                avatars[i] = avatar;
                AvatarLayer.Children.Add(avatar);
            }
            Avatars = avatars;
        }

        private ScoreBar AddBar(ScoreBar before) {
            var project = Project;
            Debug.Assert(project != null, "project != null");
            var score = project.GetScore(project.Difficulty);
            var bar = before == null ? score.AddBar() : score.InsertBar(before.Bar.Index);
            if (bar == null) {
                return null;
            }
            var scoreBar = new ScoreBar();
            scoreBar.Bar = bar;
            scoreBar.Height = BarHeight;
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
            UpdateBarTexts();
            RecalcEditorLayout();
            UpdateMaximumScrollOffset();
            return scoreBar;
        }

        private ScoreNote AddNote(ScoreBar scoreBar, ScoreBarHitTestInfo info) {
            if (!info.IsValid || info.Row < 0 || info.Column < 0) {
                if (!info.IsInNextBar) {
                    return null;
                }
                var nextBar = ScoreBars.FirstOrDefault(b => b.Bar.Index > scoreBar.Bar.Index);
                if (nextBar == null) {
                    return null;
                }
                var point = scoreBar.TranslatePoint(info.HitPoint, nextBar);
                return AddNote(nextBar, nextBar.HitTest(point));
            }
            var bar = scoreBar.Bar;
            var scoreNote = AnyNoteExistOnPosition(bar.Index, info.Column, info.Row);
            if (scoreNote != null) {
                return scoreNote;
            }
            var baseY = ScrollOffset + bar.Index * BarHeight;
            var extraY = BarHeight * info.Row / bar.GetTotalGridCount();
            scoreNote = new ScoreNote();
            scoreNote.Radius = NoteRadius;
            var note = bar.AddNote(MathHelper.NextRandomInt32());
            note.Type = NoteType.TapOrFlick;
            note.StartPosition = note.FinishPosition = (NotePosition)(info.Column + 1);
            note.PositionInGrid = info.Row;
            scoreNote.Note = note;
            EditableScoreNotes.Add(scoreNote);
            NoteLayer.Children.Add(scoreNote);
            scoreNote.X = NoteLayer.ActualWidth * TrackCenterXPositions[info.Column];
            scoreNote.Y = baseY + extraY;
            scoreNote.MouseDown += ScoreNote_MouseDown;
            scoreNote.MouseUp += ScoreNote_MouseUp;
            scoreNote.MouseDoubleClick += ScoreNote_MouseDoubleClick;
            return scoreNote;
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
                scoreBar.UpdateBarTimeText();
            }
        }

        private void UpdateMaximumScrollOffset() {
            MaximumScrollOffset = BarHeight * ScoreBars.Count;
        }

        private void TrimScoreNotes(ScoreBar willBeDeleted) {
            // Reposition after calling this function.
            var bar = willBeDeleted.Bar;
            Func<ScoreNote, bool> matchFunc = scoreNote => scoreNote.Note.Bar == bar;
            // Damn C#, their signatures are excatly the same!
            Predicate<ScoreNote> matchPredicate = scoreNote => scoreNote.Note.Bar == bar;
            var processing = ScoreNotes.Where(matchFunc).ToArray();
            foreach (var scoreNote in processing) {
                NoteLayer.Children.Remove(scoreNote);
            }
            EditableScoreNotes.RemoveAll(matchPredicate);
        }

        private List<ScoreBar> EditableScoreBars { get; }

        private List<ScoreNote> EditableScoreNotes { get; }

        private NoteRelationCollection Relations { get; }

        private ScoreNote[] Avatars { get; set; }

        private static readonly double[] TrackCenterXPositions = { 0.2, 0.35, 0.5, 0.65, 0.8 };
        //private static readonly double BaseLineYPosition = 1d / 6;
        private static readonly double BaseLineYPosition = 0.1;
        private static readonly double WorkingAreaPadding = 2;
        private static readonly double BarHeight = 550;
        private static readonly double NoteDiameter = 30;
        private static readonly double NoteRadius = NoteDiameter / 2;
        private static readonly double FutureTimeWindow = 1;
        private static readonly double PastTimeWindow = 0.2;
        // Then we know the bottom is <AvatarCenterY + (PastWindow / FutureWindow) * (AvatarCenterY - Ceiling))>.
        private static readonly double FutureNoteCeiling = 5d / 6;

    }
}
