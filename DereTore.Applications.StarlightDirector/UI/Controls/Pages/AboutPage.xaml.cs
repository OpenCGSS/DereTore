using System.Linq;
using System.Reflection;
using System.Windows;
using DereTore.Applications.StarlightDirector.Components;

namespace DereTore.Applications.StarlightDirector.UI.Controls.Pages {
    public partial class AboutPage : IDirectorPage {

        public AboutPage() {
            InitializeComponent();
            CommandHelper.InitializeCommandBindings(this);
        }

        private void AboutPage_OnLoaded(object sender, RoutedEventArgs e) {
            var mainAssembly = Assembly.GetEntryAssembly();
            var attributes = mainAssembly.GetCustomAttributes(false);
            var fileVersionAttribute = attributes.FirstOrDefault(a => a is AssemblyFileVersionAttribute) as AssemblyFileVersionAttribute;
            VersionText.Text = fileVersionAttribute?.Version;
        }

        public static string VersionPrerelease => "alpha";
        public static string CodeName => "Uzuki";

    }
}
