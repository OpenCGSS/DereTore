using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DereTore;
using StarlightDirector.Extensions;

namespace StarlightDirector.UI.Controls.Pages {
    partial class AboutPage {

        public static readonly ICommand CmdOpenLink = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdEasterEgg = CommandHelper.RegisterCommand();

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

        private void CmdEasterEgg_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void CmdEasterEgg_Executed(object sender, ExecutedRoutedEventArgs e) {
            // #01-#05 are "normal" facial expressions, #06 is yandere. (yeah my favorite!)
            // So let's play a mini gacha game. I set the probability to see #06 as 1.5%, which is the CGSS SSR drop rate.
            var n = MathHelper.NextRandomInt32(1000);
            string iconResourceName;
            if (n >= 985) {
                iconResourceName = "Mayu-06";
            } else {
                n = n % 5 + 1;
                iconResourceName = "Mayu-0" + n;
            }
            var newIcon = Application.Current.FindResource<BitmapImage>(iconResourceName);
            IconPlaceholder.Source = newIcon;
        }

    }
}
