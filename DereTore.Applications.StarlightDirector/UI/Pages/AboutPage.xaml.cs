using System.Diagnostics;
using System.Windows.Input;
using DereTore.Applications.StarlightDirector.Components;

namespace DereTore.Applications.StarlightDirector.UI.Pages {
    /// <summary>
    /// AboutPage.xaml 的交互逻辑
    /// </summary>
    partial class AboutPage : IDirectorPage {

        public AboutPage() {
            InitializeComponent();
            CommandHelper.InitializeCommandBindings(this);
        }

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
