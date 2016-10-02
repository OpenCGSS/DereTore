using System.Windows.Input;
using DereTore.Applications.StarlightDirector.Components;

namespace DereTore.Applications.StarlightDirector.UI.Windows {
    partial class MainWindow {

        public static readonly ICommand CmdViewZoomIn = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdViewZoomOut = CommandHelper.RegisterCommand();

        private void CmdViewZoomIn_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdViewZoomIn_Executed(object sender, ExecutedRoutedEventArgs e) {
            Editor.ZoomIn();
        }

        private void CmdViewZoomOut_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdViewZoomOut_Executed(object sender, ExecutedRoutedEventArgs e) {
            Editor.ZoomOut();
        }

    }
}
