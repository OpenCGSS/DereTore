using System.Windows;
using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector {
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : System.Windows.Application {

        public static readonly ResourceKeys ResourceKeys = new ResourceKeys();

        private void App_OnStartup(object sender, StartupEventArgs e) {
            Project.Current = new Project();
        }

    }
}
