using System.Windows;
using System.Windows.Input;
using DereTore.Applications.StarlightDirector.Entities.Extensions;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    partial class ScoreEditor {

        public void ZoomOutByCenter() {
            var pt = new Point(ActualWidth / 2, ActualHeight / 2);
            ZoomOut(pt);
        }

        public void ZoomInByCenter() {
            var pt = new Point(ActualWidth / 2, ActualHeight / 2);
            ZoomIn(pt);
        }

        public void ZoomOut() {
            ZoomOut(null);
        }

        public void ZoomIn() {
            ZoomIn(null);
        }

        public void ZoomTo(int oneNthBeat) {
            var centerPoint = new Point(ActualWidth / 2, ActualHeight / 2);
            double heightPercentage, scoreBarHeight;
            var originalScoreBar = GetScoreBarGeomInfoForZooming(centerPoint, out heightPercentage, out scoreBarHeight);
            const double conflictAvoidingLevel = 1.05;
            foreach (var scoreBar in ScoreBars) {
                var expectedHeight = scoreBar.NoteRadius * 2 * oneNthBeat / 4 * scoreBar.Bar.Signature;
                expectedHeight *= conflictAvoidingLevel;
                scoreBar.ZoomToHeight(expectedHeight);
            }
            RecalcEditorLayout();
            if (originalScoreBar != null && ScrollViewer != null) {
                var point = TranslatePoint(centerPoint, ScrollViewer);
                var newVertical = (ScoreBars.Count - originalScoreBar.Bar.Index - 1) * originalScoreBar.Height + originalScoreBar.Height * (1 - heightPercentage) - point.Y;
                ScrollViewer.ScrollToVerticalOffset(newVertical);
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

    }
}
