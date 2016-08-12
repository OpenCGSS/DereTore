using System;
using App = System.Windows.Forms.Application;

namespace DereTore.Application.Toolchain {
    internal static class Program {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        private static void Main() {
            App.EnableVisualStyles();
            App.SetCompatibleTextRenderingDefault(false);
            App.Run(new FMain());
        }
    }
}
