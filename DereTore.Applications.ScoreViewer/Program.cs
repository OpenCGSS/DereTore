using System;
using System.Windows.Forms;
using DereTore.Applications.ScoreViewer.Forms;

namespace DereTore.Applications.ScoreViewer {
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
