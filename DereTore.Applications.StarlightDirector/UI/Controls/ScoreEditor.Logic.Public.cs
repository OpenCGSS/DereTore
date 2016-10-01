using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.Components;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;

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

        public double ScrollOffset {
            get { return (double)GetValue(ScrollOffsetProperty); }
            set {
                value = -MathHelper.Clamp(-value, MinimumScrollOffset, MaximumScrollOffset);
                SetValue(ScrollOffsetProperty, value);
            }
        }

        public double MinimumScrollOffset {
            get { return (double)GetValue(MinimumScrollOffsetProperty); }
            private set { SetValue(MinimumScrollOffsetProperty, value); }
        }

        public double MaximumScrollOffset {
            get { return (double)GetValue(MaximumScrollOffsetProperty); }
            private set { SetValue(MaximumScrollOffsetProperty, value); }
        }

        public Score Score {
            get { return (Score)GetValue(ScoreProperty); }
            set { SetValue(ScoreProperty, value); }
        }

        public Party Party {
            get { return (Party)GetValue(PartyProperty); }
            set { SetValue(PartyProperty, value); }
        }

        public EditMode EditMode {
            get { return (EditMode)GetValue(EditModeProperty); }
            set { SetValue(EditModeProperty, value); }
        }

        public Project Project {
            get { return (Project)GetValue(ProjectProperty); }
            set { SetValue(ProjectProperty, value); }
        }

        public double SmallChange {
            get { return (double)GetValue(SmallChangeProperty); }
            set { SetValue(SmallChangeProperty, value); }
        }

        public double LargeChange {
            get { return (double)GetValue(LargeChangeProperty); }
            set { SetValue(LargeChangeProperty, value); }
        }

        public static readonly DependencyProperty ScrollOffsetProperty = DependencyProperty.Register(nameof(ScrollOffset), typeof(double), typeof(ScoreEditor),
            new PropertyMetadata(0d, OnScrollOffsetChanged));

        public static readonly DependencyProperty MinimumScrollOffsetProperty = DependencyProperty.Register(nameof(MinimumScrollOffset), typeof(double), typeof(ScoreEditor),
            new PropertyMetadata(-45d, OnMinimumScrollOffsetChanged));

        public static readonly DependencyProperty MaximumScrollOffsetProperty = DependencyProperty.Register(nameof(MaximumScrollOffset), typeof(double), typeof(ScoreEditor),
            new PropertyMetadata(0d, OnMaximumScrollOffsetChanged));

        public static readonly DependencyProperty ScoreProperty = DependencyProperty.Register(nameof(Score), typeof(Score), typeof(ScoreEditor),
            new PropertyMetadata(null, OnScoreChanged));

        public static readonly DependencyProperty PartyProperty = DependencyProperty.Register(nameof(Party), typeof(Party), typeof(ScoreEditor),
            new PropertyMetadata(Party.Neutral, OnPartyChanged));

        public static readonly DependencyProperty EditModeProperty = DependencyProperty.Register(nameof(EditMode), typeof(EditMode), typeof(ScoreEditor),
            new PropertyMetadata(EditMode.Select, OnEditModeChanged));

        public static readonly DependencyProperty ProjectProperty = DependencyProperty.Register(nameof(Project), typeof(Project), typeof(ScoreEditor),
            new PropertyMetadata(null, OnProjectChanged));

        public static readonly DependencyProperty SmallChangeProperty = DependencyProperty.Register(nameof(SmallChange), typeof(double), typeof(ScoreEditor),
            new PropertyMetadata(10d));

        public static readonly DependencyProperty LargeChangeProperty = DependencyProperty.Register(nameof(LargeChange), typeof(double), typeof(ScoreEditor),
            new PropertyMetadata(50d));

        private static void OnScrollOffsetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var editor = obj as ScoreEditor;
            Debug.Assert(editor != null, "editor != null");
            editor.RecalcEditorLayout();
        }

        private static void OnMinimumScrollOffsetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var editor = obj as ScoreEditor;
            Debug.Assert(editor != null, "editor != null");
            var newValue = (double)e.NewValue;
            if (editor.ScrollOffset < newValue) {
                editor.ScrollOffset = newValue;
            }
        }

        private static void OnMaximumScrollOffsetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var editor = obj as ScoreEditor;
            Debug.Assert(editor != null, "editor != null");
            var newValue = (double)e.NewValue;
            if (editor.ScrollOffset > newValue) {
                editor.ScrollOffset = newValue;
            }
        }

        private static void OnScoreChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var editor = obj as ScoreEditor;
            Debug.Assert(editor != null, "editor != null");
            var newScore = (Score)e.NewValue;
            editor.ReloadScore(newScore);
        }

        private static void OnPartyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var editor = obj as ScoreEditor;
            Debug.Assert(editor != null, "editor != null");
            var party = (Party)e.NewValue;

            foreach (var child in editor.AvatarLayer.Children) {
                var note = child as ScoreNote;
                if (note != null) {
                    note.Party = party;
                }
            }
            var app = Application.Current;
            switch (party) {
                case Party.Neutral:
                    editor.AvatarLine.Stroke = app.FindResource<Brush>(App.ResourceKeys.NeutralStrokeBrush);
                    break;
                case Party.Cute:
                    editor.AvatarLine.Stroke = app.FindResource<Brush>(App.ResourceKeys.CuteStrokeBrush);
                    break;
                case Party.Cool:
                    editor.AvatarLine.Stroke = app.FindResource<Brush>(App.ResourceKeys.CoolStrokeBrush);
                    break;
                case Party.Passion:
                    editor.AvatarLine.Stroke = app.FindResource<Brush>(App.ResourceKeys.PassionStrokeBrush);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(party), party, null);
            }
        }

        private static void OnEditModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
        }

        private static void OnProjectChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var editor = (ScoreEditor)obj;
            var oldproject = (Project)e.OldValue;
            var newProject = (Project)e.NewValue;
            if (oldproject != null) {
                oldproject.GlobalSettingsChanged -= editor.OnScoreGlobalSettingsChanged;
            }
            if (newProject != null) {
                newProject.GlobalSettingsChanged += editor.OnScoreGlobalSettingsChanged;
            }
            CommandManager.InvalidateRequerySuggested();
        }

        private void OnScoreGlobalSettingsChanged(object sender, EventArgs e) {
            UpdateBarTexts();
        }

    }
}
