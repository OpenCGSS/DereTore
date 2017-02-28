using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using StarlightDirector.Entities;
using StarlightDirector.UI.Controls.Primitives;

namespace StarlightDirector.UI.Windows {
    partial class MainWindow {

        public static readonly ICommand CmdEditNoteAdd = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdEditNoteStartPosition1 = CommandHelper.RegisterCommand("Ctrl+1");
        public static readonly ICommand CmdEditNoteStartPosition2 = CommandHelper.RegisterCommand("Ctrl+2");
        public static readonly ICommand CmdEditNoteStartPosition3 = CommandHelper.RegisterCommand("Ctrl+3");
        public static readonly ICommand CmdEditNoteStartPosition4 = CommandHelper.RegisterCommand("Ctrl+4");
        public static readonly ICommand CmdEditNoteStartPosition5 = CommandHelper.RegisterCommand("Ctrl+5");
        public static readonly ICommand CmdEditNoteDelete = CommandHelper.RegisterCommand("Delete");
        public static readonly ICommand CmdEditNoteSetSlideTypeToFlick = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdEditNoteSetSlideTypeToSlide = CommandHelper.RegisterCommand();

        private void CmdEditNoteAdd_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSingleSelectedScoreBar && false;
        }

        private void CmdEditNoteAdd_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.Print("Not implemented: add note");
            NotifyProjectChanged();
        }

        private void CmdEditNoteStartPosition1_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSelectedScoreNotes;
        }

        private void CmdEditNoteStartPosition1_Executed(object sender, ExecutedRoutedEventArgs e) {
            var scoreNotes = Editor.GetSelectedScoreNotes();
            ChangeNoteStartPositionTo(scoreNotes, NotePosition.Left);
            Editor.UnselectAllScoreNotes();
            NotifyProjectChanged();
        }

        private void CmdEditNoteStartPosition2_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSelectedScoreNotes;
        }

        private void CmdEditNoteStartPosition2_Executed(object sender, ExecutedRoutedEventArgs e) {
            var scoreNotes = Editor.GetSelectedScoreNotes();
            ChangeNoteStartPositionTo(scoreNotes, NotePosition.CenterLeft);
            Editor.UnselectAllScoreNotes();
            NotifyProjectChanged();
        }

        private void CmdEditNoteStartPosition3_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSelectedScoreNotes;
        }

        private void CmdEditNoteStartPosition3_Executed(object sender, ExecutedRoutedEventArgs e) {
            var scoreNotes = Editor.GetSelectedScoreNotes();
            ChangeNoteStartPositionTo(scoreNotes, NotePosition.Center);
            Editor.UnselectAllScoreNotes();
            NotifyProjectChanged();
        }

        private void CmdEditNoteStartPosition4_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSelectedScoreNotes;
        }

        private void CmdEditNoteStartPosition4_Executed(object sender, ExecutedRoutedEventArgs e) {
            var scoreNotes = Editor.GetSelectedScoreNotes();
            ChangeNoteStartPositionTo(scoreNotes, NotePosition.CenterRight);
            Editor.UnselectAllScoreNotes();
            NotifyProjectChanged();
        }

        private void CmdEditNoteStartPosition5_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSelectedScoreNotes;
        }

        private void CmdEditNoteStartPosition5_Executed(object sender, ExecutedRoutedEventArgs e) {
            var scoreNotes = Editor.GetSelectedScoreNotes();
            ChangeNoteStartPositionTo(scoreNotes, NotePosition.Right);
            Editor.UnselectAllScoreNotes();
            NotifyProjectChanged();
        }

        private void CmdEditNoteDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSelectedScoreNotes;
        }

        private void CmdEditNoteDelete_Executed(object sender, ExecutedRoutedEventArgs e) {
            var scoreNotes = Editor.GetSelectedScoreNotes();
            Editor.RemoveScoreNotes(scoreNotes);
            NotifyProjectChanged();
        }

        private static void ChangeNoteStartPositionTo(IEnumerable<ScoreNote> scoreNotes, NotePosition startPosition) {
            foreach (var scoreNote in scoreNotes) {
                var note = scoreNote.Note;
                // A rule: in a hold pair, the latter one always follows the trail of the former one.
                if (note.IsHoldStart) {
                    note.HoldTarget.StartPosition = note.StartPosition = startPosition;
                } else if (note.IsHoldEnd) {
                    note.StartPosition = note.HoldTarget.StartPosition;
                } else {
                    note.StartPosition = startPosition;
                }
            }
        }

        private void CmdEditNoteSetSlideTypeToFlick_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            var notes = Editor.GetSelectedScoreNotes();
            // The problem is that, in Note.UpdateFlickTypeStep2(), if we check n2.IsFlick from the first to the end in temporal
            // order, we will always get 'true'. Therefore we must calculate IsFlick in reversed temporal order.
            var scoreNotes = notes as ScoreNote[] ?? notes.ToArray();
            e.CanExecute = scoreNotes.Any() && scoreNotes.All(t => t.Note.IsFlick || t.Note.IsSlide);
        }

        private void CmdEditNoteSetSlideTypeToFlick_Executed(object sender, ExecutedRoutedEventArgs e) {
            var notes = Editor.GetSelectedScoreNotes();
            var scoreNotes = notes as List<ScoreNote> ?? notes.ToList();
            scoreNotes.Sort((c1, c2) => Note.TimingThenPositionComparison(c1.Note, c2.Note));
            scoreNotes.Reverse();
            foreach (var scoreNote in scoreNotes) {
                scoreNote.Note.Type = NoteType.TapOrFlick;
            }
            NotifyProjectChanged();
        }

        private void CmdEditNoteSetSlideTypeToSlide_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            var notes = Editor.GetSelectedScoreNotes();
            var scoreNotes = notes as ScoreNote[] ?? notes.Reverse().ToArray();
            e.CanExecute = scoreNotes.Any() && scoreNotes.All(t => (t.Note.IsFlick && !t.Note.IsHoldEnd) || t.Note.IsSlide);
        }

        private void CmdEditNoteSetSlideTypeToSlide_Executed(object sender, ExecutedRoutedEventArgs e) {
            var notes = Editor.GetSelectedScoreNotes();
            var scoreNotes = notes as List<ScoreNote> ?? notes.ToList();
            scoreNotes.Sort((c1, c2) => Note.TimingThenPositionComparison(c1.Note, c2.Note));
            scoreNotes.Reverse();
            foreach (var scoreNote in scoreNotes) {
                scoreNote.Note.Type = NoteType.Slide;
            }
            NotifyProjectChanged();
        }

    }
}
