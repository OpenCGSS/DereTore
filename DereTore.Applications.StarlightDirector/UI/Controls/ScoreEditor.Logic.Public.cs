using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.Components;
using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    partial class ScoreEditor {

        public ScoreEditor() {
            ScoreNotes = new List<ScoreNote>();
            ScoreBars = new List<ScoreBar>();
            Relations = new NoteRelationCollection();

            InitializeComponent();
            InitializeControls();
        }

        public event EventHandler<ScoreChangedEventArgs> CompiledScoreChanged;

        public IEnumerable<ScoreNote> GetSelectedScoreNotes() {
            return from scoreNote in ScoreNotes
                   where scoreNote.Selected
                   select scoreNote;
        }

        public IEnumerable<ScoreNote> SelectAllScoreNotes() {
            foreach (var scoreNote in ScoreNotes) {
                scoreNote.Selected = true;
            }
            return ScoreNotes;
        }

        public IEnumerable<ScoreNote> UnselectAllScoreNotes() {
            foreach (var scoreNote in ScoreNotes) {
                scoreNote.Selected = false;
            }
            return Enumerable.Empty<ScoreNote>();
        }

        public CompiledScore CompiledScore {
            get { return (CompiledScore)GetValue(CompiledScoreProperty); }
            set { SetValue(CompiledScoreProperty, value); }
        }

        public double ScrollOffset {
            get { return (double)GetValue(ScrollOffsetProperty); }
            set { SetValue(ScrollOffsetProperty, value); }
        }

        public Party Party {
            get { return (Party)GetValue(PartyProperty); }
            set { SetValue(PartyProperty, value); }
        }

        public EditMode EditMode {
            get { return (EditMode)GetValue(EditModeProperty); }
            set { SetValue(EditModeProperty, value); }
        }

        public static readonly DependencyProperty ScrollOffsetProperty = DependencyProperty.Register(nameof(ScrollOffset), typeof(double), typeof(ScoreEditor),
            new PropertyMetadata(0d, OnScrollOffsetChanged));

        public static readonly DependencyProperty CompiledScoreProperty = DependencyProperty.Register(nameof(CompiledScore), typeof(CompiledScore), typeof(ScoreEditor),
            new PropertyMetadata(null, OnCompiledScoreChanged));

        public static readonly DependencyProperty PartyProperty = DependencyProperty.Register(nameof(Party), typeof(Party), typeof(ScoreEditor),
            new PropertyMetadata(Party.Neutral, OnPartyChanged));

        public static readonly DependencyProperty EditModeProperty = DependencyProperty.Register(nameof(EditMode), typeof(EditMode), typeof(ScoreEditor),
            new PropertyMetadata(EditMode.Normal, OnEditModeChanged));

        private static void OnScrollOffsetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var editor = obj as ScoreEditor;
            Debug.Assert(editor != null, "editor != null");
            editor.RepositionBars();
            editor.RepositionNotes();
        }

        private static void OnCompiledScoreChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var editor = obj as ScoreEditor;
            Debug.Assert(editor != null, "editor != null");
            editor.CompiledScoreChanged?.Invoke(editor, new ScoreChangedEventArgs((Score)e.OldValue, (Score)e.NewValue));
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
                    editor.AvatarLine.Stroke = app.FindResource(App.ResourceKeys.NeutralStrokeBrush) as Brush;
                    break;
                case Party.Cute:
                    editor.AvatarLine.Stroke = app.FindResource(App.ResourceKeys.CuteStrokeBrush) as Brush;
                    break;
                case Party.Cool:
                    editor.AvatarLine.Stroke = app.FindResource(App.ResourceKeys.CoolStrokeBrush) as Brush;
                    break;
                case Party.Passion:
                    editor.AvatarLine.Stroke = app.FindResource(App.ResourceKeys.PassionStrokeBrush) as Brush;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(party), party, null);
            }
        }

        private static void OnEditModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
        }

    }
}
