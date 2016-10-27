using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;
using DereTore.Applications.StarlightDirector.UI.Controls.Primitives;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    partial class ScoreEditor {

        public bool HasSelectedScoreNotes => ScoreNotes.Any(scoreNote => scoreNote.IsSelected);

        public bool HasSingleSelectedScoreNote {
            get {
                var i = 0;
                foreach (var scoreNote in ScoreNotes) {
                    if (!scoreNote.IsSelected) {
                        continue;
                    }
                    ++i;
                    if (i > 1) {
                        return false;
                    }
                }
                return i == 1;
            }
        }

        public int GetSelectedScoreNoteCount() {
            return ScoreNotes.Count(scoreNote => scoreNote.IsSelected);
        }

        public ScoreNote GetSelectedScoreNote() {
            return ScoreNotes.FirstOrDefault(scoreNote => scoreNote.IsSelected);
        }

        public IEnumerable<ScoreNote> GetSelectedScoreNotes() {
            return ScoreNotes.Where(scoreNote => scoreNote.IsSelected);
        }

        public IEnumerable<ScoreNote> SelectAllScoreNotes() {
            foreach (var scoreNote in ScoreNotes) {
                scoreNote.IsSelected = true;
            }
            return ScoreNotes;
        }

        public IEnumerable<ScoreNote> UnselectAllScoreNotes() {
            foreach (var scoreNote in ScoreNotes) {
                scoreNote.IsSelected = false;
            }
            return Enumerable.Empty<ScoreNote>();
        }

        public IEnumerable<ScoreNote> UnselectAllScoreBars() {
            foreach (var scoreBar in ScoreBars) {
                scoreBar.IsSelected = false;
            }
            return Enumerable.Empty<ScoreNote>();
        }

        public bool HasSelectedScoreBars => ScoreBars.Any(scoreBar => scoreBar.IsSelected);

        public bool HasSingleSelectedScoreBar {
            get {
                var i = 0;
                foreach (var scoreBar in ScoreBars) {
                    if (!scoreBar.IsSelected) {
                        continue;
                    }
                    ++i;
                    if (i > 1) {
                        return false;
                    }
                }
                return i == 1;
            }
        }

        public ScoreBar GetSelectedScoreBar() {
            return ScoreBars.FirstOrDefault(scoreBar => scoreBar.IsSelected);
        }

        public IEnumerable<ScoreBar> GetSelectedScoreBars() {
            return ScoreBars.Where(scoreBar => scoreBar.IsSelected);
        }

        public void ScrollToScoreBar(ScoreBar scoreBar) {
            var point = scoreBar.TranslatePoint(new Point(scoreBar.Width / 2, scoreBar.Height / 2), ScrollViewer);
            var newVertical = (ScoreBars.Count - scoreBar.Bar.Index - 1) * scoreBar.Height + scoreBar.Height * 0.5 - point.Y;
            ScrollViewer.ScrollToVerticalOffset(newVertical);
        }

        public double GetBarsTotalHeight() {
            var height = ScoreBars.Sum(scoreBar => scoreBar.ActualHeight);
            return height;
        }

        public void SelectScoreBar(ScoreBar scoreBar) {
            var previousSelected = GetSelectedScoreBar();
            if (previousSelected != null) {
                previousSelected.IsSelected = false;
            }
            var current = scoreBar;
            if (current != null) {
                current.IsSelected = true;
            }
        }

        public ScoreBar GetScoreBarAtPosition(Point position, UIElement sourceElement) {
            var hitPosition = sourceElement.TranslatePoint(position, BarLayer);
            var hitResult = VisualTreeHelper.HitTest(BarLayer, hitPosition);
            var visual = hitResult.VisualHit as FrameworkElement;
            ScoreBar hitScoreBar = null;
            while (visual != null) {
                if (visual is ScoreBar) {
                    hitScoreBar = visual as ScoreBar;
                    break;
                }
                visual = visual.Parent as FrameworkElement;
            }
            return hitScoreBar;
        }

        private ScoreNote FindScoreNote(Note note) {
            return (from sn in ScoreNotes
                    where sn.Note.Equals(note)
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

    }
}
