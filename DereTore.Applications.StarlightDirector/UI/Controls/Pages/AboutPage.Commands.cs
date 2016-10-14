using System.Diagnostics;
using System.Windows.Input;

namespace DereTore.Applications.StarlightDirector.UI.Controls.Pages {
    partial class AboutPage {

        public static readonly ICommand CmdOpenLink = CommandHelper.RegisterCommand();

        private void CmdOpenLink_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void CmdOpenLink_Executed(object sender, ExecutedRoutedEventArgs e) {
            var link = e.Parameter as string;
            if (link != null) {
                var startInfo = new ProcessStartInfo(link);
                Process.Start(startInfo);
            }
        }

    }
}
