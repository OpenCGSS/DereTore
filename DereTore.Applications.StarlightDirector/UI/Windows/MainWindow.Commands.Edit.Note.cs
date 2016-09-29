using System.Diagnostics;
using System.Windows.Input;
using DereTore.Applications.StarlightDirector.Components;

namespace DereTore.Applications.StarlightDirector.UI.Windows {
    partial class MainWindow {

        public static readonly ICommand CmdEditNoteAdd = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdEditNoteEdit = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdEditNoteDelete = CommandHelper.RegisterCommand("Delete");

        private void CmdEditNoteAdd_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSingleSelectedScoreBar;
        }

        private void CmdEditNoteAdd_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.Print("Not implemented: add note");
            NotifyProjectChanged();
        }

        private void CmdEditNoteEdit_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.HasSingleSelectedScoreNote;
        }

        private void CmdEditNoteEdit_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.Print("Not implemented: edit note");
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

    }
}
