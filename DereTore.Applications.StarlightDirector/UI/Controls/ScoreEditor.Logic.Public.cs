using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    partial class ScoreEditor {

        public ScoreEditor() {
            EditableScoreNotes = new List<ScoreNote>();
            EditableScoreBars = new List<ScoreBar>();
            ScoreNotes = EditableScoreNotes.AsReadOnly();
            ScoreBars = EditableScoreBars.AsReadOnly();

            InitializeComponent();
            InitializeControls();
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
            UpdateMaximumScrollOffset();
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
            UpdateMaximumScrollOffset();
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

        public ReadOnlyCollection<ScoreNote> ScoreNotes { get; }

        public ReadOnlyCollection<ScoreBar> ScoreBars { get; }

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
            var barHeight = ScoreBars[0].Height;
            var y = Math.Abs(MinimumScrollOffset) + scoreBar.Bar.Index * barHeight;
            ScrollOffset = y;
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

        public void SetGlobalBpm(double bpm) {
            foreach (var scoreBar in ScoreBars) {
                scoreBar.SetGlobalBpm(bpm);
            }
        }

        public void ScrollUpSmall() {
            var targetOffset = ScrollOffset + SmallChange;
            targetOffset = -MathHelper.Clamp(-targetOffset, MinimumScrollOffset, MaximumScrollOffset);
            if (!targetOffset.Equals(ScrollOffset)) {
                ScrollOffset = targetOffset;
            }
        }

        public void ScrollUpLarge() {
            var targetOffset = ScrollOffset + LargeChange;
            targetOffset = -MathHelper.Clamp(-targetOffset, MinimumScrollOffset, MaximumScrollOffset);
            if (!targetOffset.Equals(ScrollOffset)) {
                ScrollOffset = targetOffset;
            }
        }

        public void ScrollDownSmall() {
            var targetOffset = ScrollOffset - SmallChange;
            targetOffset = -MathHelper.Clamp(-targetOffset, MinimumScrollOffset, MaximumScrollOffset);
            if (!targetOffset.Equals(ScrollOffset)) {
                ScrollOffset = targetOffset;
            }
        }

        public void ScrollDownLarge() {
            var targetOffset = ScrollOffset - LargeChange;
            targetOffset = -MathHelper.Clamp(-targetOffset, MinimumScrollOffset, MaximumScrollOffset);
            if (!targetOffset.Equals(ScrollOffset)) {
                ScrollOffset = targetOffset;
            }
        }

        public void ScrollToStart() {
            var targetOffset = -MinimumScrollOffset;
            targetOffset = -MathHelper.Clamp(-targetOffset, MinimumScrollOffset, MaximumScrollOffset);
            if (!targetOffset.Equals(ScrollOffset)) {
                ScrollOffset = targetOffset;
            }
        }

        public void ScrollToEnd() {
            var targetOffset = -MaximumScrollOffset;
            targetOffset = -MathHelper.Clamp(-targetOffset, MinimumScrollOffset, MaximumScrollOffset);
            if (!targetOffset.Equals(ScrollOffset)) {
                ScrollOffset = targetOffset;
            }
        }

        public void ZoomOut() {
            foreach (var scoreBar in ScoreBars) {
                scoreBar.ZoomOut();
            }
            UpdateMaximumScrollOffset();
            RecalcEditorLayout();
        }

        public void ZoomIn() {
            foreach (var scoreBar in ScoreBars) {
                scoreBar.ZoomIn();
            }
            UpdateMaximumScrollOffset();
            RecalcEditorLayout();
        }
        
    }
}
