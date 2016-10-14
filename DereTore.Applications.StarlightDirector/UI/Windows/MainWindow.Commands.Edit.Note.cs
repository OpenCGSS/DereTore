using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.UI.Controls;

namespace DereTore.Applications.StarlightDirector.UI.Windows {
    partial class MainWindow {

        public static readonly ICommand CmdEditNoteAdd = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdEditNoteStartPosition1 = CommandHelper.RegisterCommand("Ctrl+1");
        public static readonly ICommand CmdEditNoteStartPosition2 = CommandHelper.RegisterCommand("Ctrl+2");
        public static readonly ICommand CmdEditNoteStartPosition3 = CommandHelper.RegisterCommand("Ctrl+3");
        public static readonly ICommand CmdEditNoteStartPosition4 = CommandHelper.RegisterCommand("Ctrl+4");
        public static readonly ICommand CmdEditNoteStartPosition5 = CommandHelper.RegisterCommand("Ctrl+5");
        public static readonly ICommand CmdEditNoteDelete = CommandHelper.RegisterCommand("Delete");

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
            NotifyProjectChanged();
        }

        private void CmdEditNoteStartPosition2_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSelectedScoreNotes;
        }

        private void CmdEditNoteStartPosition2_Executed(object sender, ExecutedRoutedEventArgs e) {
            var scoreNotes = Editor.GetSelectedScoreNotes();
            ChangeNoteStartPositionTo(scoreNotes, NotePosition.CenterLeft);
            NotifyProjectChanged();
        }

        private void CmdEditNoteStartPosition3_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSelectedScoreNotes;
        }

        private void CmdEditNoteStartPosition3_Executed(object sender, ExecutedRoutedEventArgs e) {
            var scoreNotes = Editor.GetSelectedScoreNotes();
            ChangeNoteStartPositionTo(scoreNotes, NotePosition.Center);
            NotifyProjectChanged();
        }

        private void CmdEditNoteStartPosition4_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSelectedScoreNotes;
        }

        private void CmdEditNoteStartPosition4_Executed(object sender, ExecutedRoutedEventArgs e) {
            var scoreNotes = Editor.GetSelectedScoreNotes();
            ChangeNoteStartPositionTo(scoreNotes, NotePosition.CenterRight);
            NotifyProjectChanged();
        }

        private void CmdEditNoteStartPosition5_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSelectedScoreNotes;
        }

        private void CmdEditNoteStartPosition5_Executed(object sender, ExecutedRoutedEventArgs e) {
            var scoreNotes = Editor.GetSelectedScoreNotes();
            ChangeNoteStartPositionTo(scoreNotes, NotePosition.Right);
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

    }
}
