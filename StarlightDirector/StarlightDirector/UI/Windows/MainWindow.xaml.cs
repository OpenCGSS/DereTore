using System;
using System.Timers;
using DereTore.Interop.OS;

namespace StarlightDirector.UI.Windows {
    public partial class MainWindow {

        public MainWindow() {
            InitializeComponent();
            CommandHelper.InitializeCommandBindings(this);
            _temporaryMessageTimer = new Timer(TemporaryMessageTimeout) {
                AutoReset = true
            };
            _temporaryMessageTimer.Elapsed += TemporaryMessageTimer_OnElapsed;
            _autoSaveTimer = new Timer(AutoSaveInterval);
            _autoSaveTimer.Elapsed += AutoSaveTimer_OnElapsed;
        }

        public void ShowTemporaryMessage(string message) {
            if (string.IsNullOrEmpty(message)) {
                return;
            }
            TemporaryMessage = message;
            if (IsTemporaryMessageVisible) {
                _temporaryMessageTimer.Stop();
                _temporaryMessageTimer.Start();
            } else {
                IsTemporaryMessageVisible = true;
                _temporaryMessageTimer.Start();
            }
        }

        internal void NotifyProjectChanged() {
            if (Project != null) {
                Project.IsChanged = true;
            }
        }

        private void OnDwmColorizationColorChanged(object sender, EventArgs e) {
            AccentColorBrush = UIHelper.GetWindowColorizationBrush();
        }

        private IntPtr WndProc(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            switch (uMsg) {
                case NativeConstants.WM_DWMCOLORIZATIONCOLORCHANGED:
                    OnDwmColorizationColorChanged(this, EventArgs.Empty);
                    return IntPtr.Zero;
                default:
                    return IntPtr.Zero;
            }
        }

        private bool ShouldPromptSaving => Project != null && Project.IsChanged;

        private Timer _temporaryMessageTimer;
        private Timer _autoSaveTimer;

        private static readonly double TemporaryMessageTimeout = TimeSpan.FromSeconds(6).TotalMilliseconds;
        private static readonly double AutoSaveInterval = TimeSpan.FromMinutes(3).TotalMilliseconds;

    }
}
