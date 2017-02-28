using System.Windows.Input;

namespace StarlightDirector.UI.Windows {
    partial class MainWindow {

        public static readonly ICommand CmdBgScrollUpSmall = CommandHelper.RegisterCommand("Up");
        public static readonly ICommand CmdBgScrollDownSmall = CommandHelper.RegisterCommand("Down");
        public static readonly ICommand CmdBgScrollUpLarge = CommandHelper.RegisterCommand("PageUp");
        public static readonly ICommand CmdBgScrollDownLarge = CommandHelper.RegisterCommand("PageDown");
        public static readonly ICommand CmdBgScrollToStart = CommandHelper.RegisterCommand("Home");
        public static readonly ICommand CmdBgScrollToEnd = CommandHelper.RegisterCommand("End");

        private void CmdBgScrollUpSmall_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdBgScrollUpSmall_Executed(object sender, ExecutedRoutedEventArgs e) {
            ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset - CustomScroll.GetScrollSpeed(ScrollViewer));
        }

        private void CmdBgScrollDownSmall_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdBgScrollDownSmall_Executed(object sender, ExecutedRoutedEventArgs e) {
            ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset + CustomScroll.GetScrollSpeed(ScrollViewer));
        }

        private void CmdBgScrollUpLarge_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdBgScrollUpLarge_Executed(object sender, ExecutedRoutedEventArgs e) {
            ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset - CustomScroll.GetScrollSpeed(ScrollViewer) * 10);
        }

        private void CmdBgScrollDownLarge_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdBgScrollDownLarge_Executed(object sender, ExecutedRoutedEventArgs e) {
            ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset + CustomScroll.GetScrollSpeed(ScrollViewer) * 10);
        }

        private void CmdBgScrollToStart_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdBgScrollToStart_Executed(object sender, ExecutedRoutedEventArgs e) {
            ScrollViewer.ScrollToHome();
        }

        private void CmdBgScrollToEnd_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

        private void CmdBgScrollToEnd_Executed(object sender, ExecutedRoutedEventArgs e) {
            ScrollViewer.ScrollToEnd();
        }

    }
}
