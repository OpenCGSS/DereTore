using System.Windows.Input;
using DereTore.Applications.StarlightDirector.Components;

namespace DereTore.Applications.StarlightDirector.UI.Windows {
    partial class MainWindow {

        public static ICommand CmdBgScrollUpSmall = CommandHelper.RegisterCommand("Up");
        public static ICommand CmdBgScrollDownSmall = CommandHelper.RegisterCommand("Down");
        public static ICommand CmdBgScrollUpLarge = CommandHelper.RegisterCommand("PageUp");
        public static ICommand CmdBgScrollDownLarge = CommandHelper.RegisterCommand("PageDown");
        public static ICommand CmdBgScrollToStart = CommandHelper.RegisterCommand("Home");
        public static ICommand CmdBgScrollToEnd = CommandHelper.RegisterCommand("End");

        private void CmdBgScrollUpSmall_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdBgScrollUpSmall_Executed(object sender, ExecutedRoutedEventArgs e) {
            Editor.ScrollOffset += Editor.SmallChange;
        }

        private void CmdBgScrollDownSmall_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdBgScrollDownSmall_Executed(object sender, ExecutedRoutedEventArgs e) {
            Editor.ScrollOffset -= Editor.SmallChange;
        }

        private void CmdBgScrollUpLarge_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdBgScrollUpLarge_Executed(object sender, ExecutedRoutedEventArgs e) {
            Editor.ScrollOffset += Editor.LargeChange;
        }

        private void CmdBgScrollDownLarge_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdBgScrollDownLarge_Executed(object sender, ExecutedRoutedEventArgs e) {
            Editor.ScrollOffset -= Editor.LargeChange;
        }

        private void CmdBgScrollToStart_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdBgScrollToStart_Executed(object sender, ExecutedRoutedEventArgs e) {
            Editor.ScrollOffset = -Editor.MinimumScrollOffset;
        }

        private void CmdBgScrollToEnd_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdBgScrollToEnd_Executed(object sender, ExecutedRoutedEventArgs e) {
            Editor.ScrollOffset = -Editor.MaximumScrollOffset;
        }

    }
}
