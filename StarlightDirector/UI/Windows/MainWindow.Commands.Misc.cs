using System.Windows.Input;
using StarlightDirector.Entities;

namespace StarlightDirector.UI.Windows {
    partial class MainWindow {
        
        public static readonly ICommand CmdScoreSwitchDifficulty = CommandHelper.RegisterCommand();
               
        private void CmdScoreSwitchDifficulty_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            if (Project == null) {
                e.CanExecute = false;
            } else {
                e.CanExecute = Project.Difficulty != (Difficulty)e.Parameter;
            }
        }

        private void CmdScoreSwitchDifficulty_Executed(object sender, ExecutedRoutedEventArgs e) {
            if (Project != null) {
                Project.Difficulty = (Difficulty)e.Parameter;
            }
        }

    }
}
