using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Entities.Extensions;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    partial class ScoreEditor {

        public ScoreEditor() {
            EditableScoreNotes = new List<ScoreNote>();
            EditableScoreBars = new List<ScoreBar>();
            EditableSpecialScoreNotes = new List<SpecialNotePointer>();
            ScoreNotes = EditableScoreNotes.AsReadOnly();
            ScoreBars = EditableScoreBars.AsReadOnly();
            SpecialScoreNotes = EditableSpecialScoreNotes.AsReadOnly();

            InitializeComponent();
        }

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
            return AddSpecialNote(scoreBar, row, type, null);
        }

        public bool RemoveSpecialNote(SpecialNotePointer specialNotePointer) {
            var exists = SpecialScoreNotes.Contains(specialNotePointer);
            if (!exists) {
                return false;
            }
            var note = specialNotePointer.Note;
            note.Bar.RemoveNote(note);
            NoteIDs.ExistingIDs.Remove(note.ID);
            SpecialNoteLayer.Children.Remove(specialNotePointer);
            Project.IsChanged = true;
            return EditableSpecialScoreNotes.Remove(specialNotePointer);
        }

        public ReadOnlyCollection<ScoreNote> ScoreNotes { get; }

        public ReadOnlyCollection<ScoreBar> ScoreBars { get; }

        public ReadOnlyCollection<SpecialNotePointer> SpecialScoreNotes { get; }

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
                var expectedHeight = scoreBar.NoteRadius * 2 * oneNthBeat / 4 * scoreBar.Bar.GetActualSignature();
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

        public ScoreBarHitTestInfo LastHitTestInfo { get; private set; }

    }
}
