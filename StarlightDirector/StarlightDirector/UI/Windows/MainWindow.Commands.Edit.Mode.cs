using System.Diagnostics;
using System.Windows.Input;

namespace StarlightDirector.UI.Windows {
    partial class MainWindow {

        public static readonly ICommand CmdEditModeCreateRelations = CommandHelper.RegisterCommand("Alt+1", "Alt+NumPad1");
        public static readonly ICommand CmdEditModeResetNote = CommandHelper.RegisterCommand("Alt+2", "Alt+NumPad2");

        private void CmdEditModeCreateRelations_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.EditMode != EditMode.CreateRelations;
        }

        private void CmdEditModeCreateRelations_Executed(object sender, ExecutedRoutedEventArgs e) {
            Editor.EditMode = EditMode.CreateRelations;
        }

        private void CmdEditModeResetNote_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.EditMode != EditMode.ResetNote;
        }

        private void CmdEditModeResetNote_Executed(object sender, ExecutedRoutedEventArgs e) {
            Editor.EditMode = EditMode.ResetNote;
        }

    }
}
