﻿using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using DereTore.Applications.StarlightDirector.Components;

namespace DereTore.Applications.StarlightDirector.UI.Pages {
    /// <summary>
    /// AboutPage.xaml 的交互逻辑
    /// </summary>
    public partial class AboutPage : IDirectorPage {

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

        private void AboutPage_OnLoaded(object sender, RoutedEventArgs e) {
            var mainAssembly = Assembly.GetEntryAssembly();
            var attributes = mainAssembly.GetCustomAttributes(false);
            var fileVersionAttribute = attributes.FirstOrDefault(a => a is AssemblyFileVersionAttribute) as AssemblyFileVersionAttribute;
            VersionText.Text = fileVersionAttribute?.Version;
        }

        public static string VersionExtra => "Alpha";

    }
}
