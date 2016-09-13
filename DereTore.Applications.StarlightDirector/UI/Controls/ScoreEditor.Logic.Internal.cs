using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.Components;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    partial class ScoreEditor {

        internal ScoreNote[] Avatars { get; private set; }

        private void ReloadScore() {
            // ...

            var compiledScore = CompiledScore;

            RepositionBars();
            RepositionNotes();
            throw new NotImplementedException();
        }

        private void ResizeBars() {
            foreach (var scoreBar in ScoreBars) {
                scoreBar.BarColumnWidth = BarLayer.ActualWidth * (TrackCenterXPositions[4] - TrackCenterXPositions[0]) * (5 + 1) / (5 - 1);
                Canvas.SetLeft(scoreBar, BarLayer.ActualWidth * TrackCenterXPositions[0] - scoreBar.TextColumnWidth - scoreBar.BarColumnWidth / (5 + 1));
            }
        }

        private void RepositionBars() {
            var barLayer = BarLayer;
            //barLayer.Children.Clear();
            if (ScoreBars == null || ScoreBars.Count == 0) {
                return;
            }
            foreach (var scoreBar in ScoreBars) {
                Canvas.SetTop(scoreBar, ScrollOffset + scoreBar.Bar.Index * BarHeight);
            }
            var score = CompiledScore?.Original;
            if (score == null) {
                return;
            }
            //throw new NotImplementedException();
        }

        private void ResizeNotes() {
            //throw new NotImplementedException();
        }

        private void RepositionNotes() {
            if (ScoreNotes == null || ScoreNotes.Count == 0) {
                return;
            }
            foreach (var scoreNote in ScoreNotes) {
                var note = scoreNote.Note;
                var bar = note.Bar;
                var baseY = ScrollOffset + bar.Index * BarHeight;
                var extraY = BarHeight * note.PositionInGrid / bar.GetTotalGridCount();
                Canvas.SetTop(scoreNote, baseY + extraY - scoreNote.Radius);
                Canvas.SetLeft(scoreNote, NoteLayer.ActualWidth * TrackCenterXPositions[(int)note.FinishPosition - 1] - scoreNote.Radius);
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

        private Rect GetWorkAreaRect() {
            var width = WorkingAreaClip.ActualWidth;
            var height = WorkingAreaClip.ActualHeight;
            return new Rect(WorkingAreaPadding, WorkingAreaPadding, width - WorkingAreaPadding * 2, height - WorkingAreaPadding * 2);
        }

        private void InitializeControls() {
            var avatars = new ScoreNote[5];
            for (var i = 0; i < 5; ++i) {
                var image = Application.Current.FindResource($"CardAvatar{i + 1}") as ImageSource;
                var avatar = new ScoreNote();
                avatar.Radius = NoteRadius;
                avatar.Image = image;
                avatars[i] = avatar;
                AvatarLayer.Children.Add(avatar);
            }
            AddBar();
            AddBar();
            Avatars = avatars;
        }

        private ScoreBar AddBar() {
            var bar = Project.CurrentProject?.GetScore(Difficulty.Master)?.AddBar();
            if (bar == null) {
                return null;
            }
            var scoreBar = new ScoreBar();
            scoreBar.Bar = bar;
            scoreBar.Height = BarHeight;
            scoreBar.ScoreBarHitTest += ScoreBar_ScoreBarHitTest;
            scoreBar.PreviewMouseDoubleClick += ScoreBar_PreviewMouseDoubleClick;
            BarLayer.Children.Add(scoreBar);
            ScoreBars.Add(scoreBar);
            return scoreBar;
        }

        private void RemoveBar(ScoreBar scoreBar) {
            if (!ScoreBars.Contains(scoreBar)) {
                throw new ArgumentException("Invalid ScoreBar.", nameof(scoreBar));
            }
            scoreBar.ScoreBarHitTest -= ScoreBar_ScoreBarHitTest;
            scoreBar.PreviewMouseDoubleClick -= ScoreBar_PreviewMouseDoubleClick;
            ScoreBars.Remove(scoreBar);
            BarLayer.Children.Remove(scoreBar);
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
            ScoreNote scoreNote;
            if (DoesNoteExistOnPosition(bar.Index, info.Column, info.Row, out scoreNote)) {
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
            Canvas.SetTop(scoreNote, baseY + extraY - scoreNote.Radius);
            Canvas.SetLeft(scoreNote, NoteLayer.ActualWidth * TrackCenterXPositions[info.Column] - scoreNote.Radius);
            ScoreNotes.Add(scoreNote);
            NoteLayer.Children.Add(scoreNote);
            scoreNote.MouseDown += ScoreNote_MouseDown;
            scoreNote.MouseUp += ScoreNote_MouseUp;
            scoreNote.MouseDoubleClick += ScoreNote_MouseDoubleClick;
            return scoreNote;
        }

        private void RemoveNote(ScoreNote scoreNote) {
            if (!ScoreNotes.Contains(scoreNote)) {
                throw new ArgumentException("Invalid ScoreNote.", nameof(scoreNote));
            }
            scoreNote.MouseDown -= ScoreNote_MouseDown;
            scoreNote.MouseUp -= ScoreNote_MouseUp;
            scoreNote.MouseDoubleClick -= ScoreNote_MouseDoubleClick;
            ScoreNotes.Remove(scoreNote);
            NoteLayer.Children.Remove(scoreNote);
        }

        private bool DoesNoteExistOnPosition(int barIndex, int column, int row, out ScoreNote existNote) {
            foreach (var scoreNote in ScoreNotes) {
                var note = scoreNote.Note;
                if (note.Bar.Index == barIndex && (int)note.FinishPosition == column + 1 && note.PositionInGrid == row) {
                    existNote = scoreNote;
                    return true;
                }
            }
            existNote = null;
            return false;
        }

        private List<ScoreBar> ScoreBars { get; }

        private List<ScoreNote> ScoreNotes { get; }

        private NoteRelationCollection Relations { get; }

        private static readonly double[] TrackCenterXPositions = { 0.2, 0.35, 0.5, 0.65, 0.8 };
        private static readonly double BaseLineYPosition = 1d / 6;
        private static readonly double WorkingAreaPadding = 2;
        private static readonly double BarHeight = 550;
        private static readonly double NoteDiameter = 30;
        public static readonly double NoteRadius = NoteDiameter / 2;
        public static readonly double FutureTimeWindow = 1;
        public static readonly double PastTimeWindow = 0.2;
        // Then we know the bottom is <AvatarCenterY + (PastWindow / FutureWindow) * (AvatarCenterY - Ceiling))>.
        public static readonly double FutureNoteCeiling = 5d / 6;
        public static readonly double MinimumScrollOffset = -45;
        public static readonly double SingleScrollDistance = 15;

    }
}
