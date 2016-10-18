using System.Diagnostics;
using System.Windows.Input;

namespace DereTore.Applications.StarlightDirector.UI.Windows {
    partial class MainWindow {

        public static readonly ICommand CmdEditModeSelect = CommandHelper.RegisterCommand("Alt+1", "Alt+NumPad1");
        public static readonly ICommand CmdEditModeEditRelations = CommandHelper.RegisterCommand("Alt+2", "Alt+NumPad2");
        public static readonly ICommand CmdEditModeClear = CommandHelper.RegisterCommand("Alt+0", "Alt+NumPad0");

        private void CmdEditModeSelect_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.EditMode != EditMode.Select;
        }

        private void CmdEditModeSelect_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.Print("Edit mode: select");
            Editor.EditMode = EditMode.Select;
        }

        private void CmdEditModeEditRelations_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.EditMode != EditMode.Relations;
        }

        private void CmdEditModeEditRelations_Executed(object sender, ExecutedRoutedEventArgs e) {
            if (Editor.EditMode == EditMode.Relations) {
                Editor.EditMode = EditMode.Select;
                return;
            }
            Editor.EditMode = EditMode.Relations;
            Debug.Print("Edit mode: relations");
        }

        private void CmdEditModeClear_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.EditMode != EditMode.Clear;
        }

        private void CmdEditModeClear_Executed(object sender, ExecutedRoutedEventArgs e) {
            if (Editor.EditMode == EditMode.Clear) {
                Editor.EditMode = EditMode.Select;
                return;
            }
            Editor.EditMode = EditMode.Clear;
            Debug.Print("Edit mode: clear");
        }

    }
}
