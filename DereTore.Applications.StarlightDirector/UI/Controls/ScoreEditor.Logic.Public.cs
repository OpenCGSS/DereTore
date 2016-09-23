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
            Relations = new NoteRelationCollection();

            InitializeComponent();
            InitializeControls();
        }

        public ScoreBar AppendBar() {
            return AddBar(null);
        }

        public ScoreBar InsertBar(ScoreBar before) {
            return AddBar(before);
        }

        public void RemoveBar(ScoreBar scoreBar) {
            if (!ScoreBars.Contains(scoreBar)) {
                throw new ArgumentException("Invalid ScoreBar.", nameof(scoreBar));
            }
            scoreBar.ScoreBarHitTest -= ScoreBar_ScoreBarHitTest;
            scoreBar.MouseDoubleClick -= ScoreBar_MouseDoubleClick;
            scoreBar.MouseDown -= ScoreBar_MouseDown;
            Score.RemoveBarAt(scoreBar.Bar.Index);
            EditableScoreBars.Remove(scoreBar);
            BarLayer.Children.Remove(scoreBar);
            TrimScoreNotes(scoreBar);
            UpdateBarTexts();
            RecalcEditorLayout();
            UpdateMaximumScrollOffset();
        }

        public ScoreNote AddNote(ScoreBar scoreBar, int row, NotePosition position) {
            return AddNote(scoreBar, row, (int)position - 1);
        }

        public ScoreNote AddNote(ScoreBar scoreBar, int row, int column) {
            if (row < 0 || column < 0 || row >= 5) {
                return null;
            }
            if (row >= scoreBar.Bar.GetTotalGridCount()) {
                return null;
            }
            var bar = scoreBar.Bar;
            var scoreNote = AnyNoteExistOnPosition(bar.Index, column, row);
            if (scoreNote != null) {
                return scoreNote;
            }
            var baseY = ScrollOffset + bar.Index * BarHeight;
            var extraY = BarHeight * row / bar.GetTotalGridCount();
            scoreNote = new ScoreNote();
            scoreNote.Radius = NoteRadius;
            var note = bar.AddNote(MathHelper.NextRandomInt32());
            note.Type = NoteType.TapOrFlick;
            note.StartPosition = note.FinishPosition = (NotePosition)(column + 1);
            note.PositionInGrid = row;
            scoreNote.Note = note;
            EditableScoreNotes.Add(scoreNote);
            NoteLayer.Children.Add(scoreNote);
            scoreNote.X = NoteLayer.ActualWidth * TrackCenterXPositions[column];
            scoreNote.Y = baseY + extraY;
            scoreNote.MouseDown += ScoreNote_MouseDown;
            scoreNote.MouseUp += ScoreNote_MouseUp;
            scoreNote.MouseDoubleClick += ScoreNote_MouseDoubleClick;
            return scoreNote;
        }

        public void RemoveNote(ScoreNote scoreNote) {
            if (!ScoreNotes.Contains(scoreNote)) {
                throw new ArgumentException("Invalid ScoreNote.", nameof(scoreNote));
            }
            scoreNote.MouseDown -= ScoreNote_MouseDown;
            scoreNote.MouseUp -= ScoreNote_MouseUp;
            scoreNote.MouseDoubleClick -= ScoreNote_MouseDoubleClick;
            EditableScoreNotes.Remove(scoreNote);
            var note = scoreNote.Note;
            if (Score.Bars.Contains(note.Bar)) {
                note.Bar.Notes.Remove(note);
                Debug.Print("Note removed.");
            }
            NoteLayer.Children.Remove(scoreNote);
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

        public void SetGlobalBpm(double bpm) {
            foreach (var scoreBar in ScoreBars) {
                scoreBar.SetGlobalBpm(bpm);
            }
        }

        public double ScrollOffset {
            get { return (double)GetValue(ScrollOffsetProperty); }
            set { SetValue(ScrollOffsetProperty, value); }
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
            editor.ReloadScore();
        }

        private static void OnPartyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var editor = obj as ScoreEditor;
            Debug.Assert(editor != null, "editor != null");
            var party = (Party)e.NewValue;

            ScoreNote note;
            foreach (var child in editor.AvatarLayer.Children) {
                note = child as ScoreNote;
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
            CommandManager.InvalidateRequerySuggested();
        }

    }
}
