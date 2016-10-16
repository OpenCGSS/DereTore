using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;

namespace DereTore.Applications.StarlightDirector {
    public partial class App {

        public static readonly ResourceKeys ResourceKeys = new ResourceKeys();

        public static string Title => "Starlight Director";

        private void App_OnStartup(object sender, StartupEventArgs e) {
            bool createdNewMutex;
            var mutex = new Mutex(true, DirectorMutexName, out createdNewMutex);
            if (!createdNewMutex) {
                MessageBox.Show(Application.Current.FindResource<string>(ResourceKeys.ApplicationIsAlreadyRunningPrompt), Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Shutdown();
            }
            _singleInstanceMutex = mutex;
            Project.Current = new Project();
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            var message = e.Exception.Message + Environment.NewLine + e.Exception.StackTrace;
            MessageBox.Show(message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            // No need to set e.Handled.
        }

        private void App_OnExit(object sender, ExitEventArgs e) {
            if (_singleInstanceMutex != null) {
                _singleInstanceMutex.Dispose();
                _singleInstanceMutex = null;
            }
        }

        private Mutex _singleInstanceMutex;

        private static readonly string DirectorMutexName = "StarlightDirector";

    }
}
