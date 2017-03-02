using System;
using System.Windows.Forms;
using DereTore.Apps.ScoreViewer.Forms;

namespace DereTore.Apps.ScoreViewer {
    internal static class Program {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        private static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FViewer());
        }
    }
}
