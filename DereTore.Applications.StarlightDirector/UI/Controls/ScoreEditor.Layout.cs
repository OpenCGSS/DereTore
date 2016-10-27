using System.Linq;
using System.Windows.Controls;
using DereTore.Applications.StarlightDirector.Entities.Extensions;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    partial class ScoreEditor {

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
                height += ScoreBar.GridStrokeThickness;
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

    }
}
