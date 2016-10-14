using System.Diagnostics;
using System.Windows.Input;

namespace DereTore.Applications.StarlightDirector.UI.Windows {
    partial class MainWindow {

        public static readonly ICommand CmdEditModeSelect = CommandHelper.RegisterCommand("Alt+1", "Alt+NumPad1");
        public static readonly ICommand CmdEditModeEditSync = CommandHelper.RegisterCommand("Alt+2", "Alt+NumPad2");
        public static readonly ICommand CmdEditModeEditFlick = CommandHelper.RegisterCommand("Alt+3", "Alt+NumPad3");
        public static readonly ICommand CmdEditModeEditHold = CommandHelper.RegisterCommand("Alt+4", "Alt+NumPad4");
        public static readonly ICommand CmdEditModeClear = CommandHelper.RegisterCommand("Alt+0", "Alt+NumPad0");

        private void CmdEditModeSelect_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.EditMode != EditMode.Select;
        }

        private void CmdEditModeSelect_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.Print("Edit mode: select");
            Editor.EditMode = EditMode.Select;
        }

        private void CmdEditModeEditSync_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.EditMode != EditMode.Sync;
        }

        private void CmdEditModeEditSync_Executed(object sender, ExecutedRoutedEventArgs e) {
            if (Editor.EditMode == EditMode.Sync) {
                Editor.EditMode = EditMode.Select;
                return;
            }
            Editor.EditMode = EditMode.Sync;
            Debug.Print("Edit mode: sync");
        }

        private void CmdEditModeEditFlick_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.EditMode != EditMode.Flick;
        }

        private void CmdEditModeEditFlick_Executed(object sender, ExecutedRoutedEventArgs e) {
            if (Editor.EditMode == EditMode.Flick) {
                Editor.EditMode = EditMode.Select;
                return;
            }
            Editor.EditMode = EditMode.Flick;
            Debug.Print("Edit mode: flick");
        }

        private void CmdEditModeEditHold_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.EditMode != EditMode.Hold;
        }

        private void CmdEditModeEditHold_Executed(object sender, ExecutedRoutedEventArgs e) {
            if (Editor.EditMode == EditMode.Hold) {
                Editor.EditMode = EditMode.Select;
                return;
            }
            Editor.EditMode = EditMode.Hold;
            Debug.Print("Edit mode: hold");
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
