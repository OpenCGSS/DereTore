using System;
using System.Windows.Forms;
using DereTore.Applications.ScoreEditor.Forms;

namespace DereTore.Applications.ScoreEditor {
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
