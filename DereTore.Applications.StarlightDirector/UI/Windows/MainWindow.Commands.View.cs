using System.Windows.Input;

namespace DereTore.Applications.StarlightDirector.UI.Windows {
    partial class MainWindow {

        public static readonly ICommand CmdViewZoomIn = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdViewZoomOut = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdViewZoomToBeat = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdViewMiscToggleRelationIndicatorsVisibility = CommandHelper.RegisterCommand();

        private void CmdViewZoomIn_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdViewZoomIn_Executed(object sender, ExecutedRoutedEventArgs e) {
            Editor.ZoomInByCenter();
        }

        private void CmdViewZoomOut_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdViewZoomOut_Executed(object sender, ExecutedRoutedEventArgs e) {
            Editor.ZoomOutByCenter();
        }

        private void CmdViewZoomToBeat_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdViewZoomToBeat_Executed(object sender, ExecutedRoutedEventArgs e) {
            // 4, 8, 16, 24, etc.
            var oneNthBeat = int.Parse((string)e.Parameter);
            Editor.ZoomTo(oneNthBeat);
        }

        private void CmdViewMiscToggleRelationIndicatorsVisibility_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void CmdViewMiscToggleRelationIndicatorsVisibility_Executed(object sender, ExecutedRoutedEventArgs e) {
            Editor.AreRelationIndicatorsVisible = !Editor.AreRelationIndicatorsVisible;
        }

    }
}
