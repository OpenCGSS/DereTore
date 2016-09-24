using System.Windows;
using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector {
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    partial class App {

        public static readonly ResourceKeys ResourceKeys = new ResourceKeys();

        private void App_OnStartup(object sender, StartupEventArgs e) {
            Project.Current = new Project();
        }

        public static string Title => "Starlight Director";

    }
}
