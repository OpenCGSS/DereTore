using System.Windows.Forms;

namespace DereTore.Apps.ScoreViewer.Forms {
    internal static class FormExtensions {

        public static DialogResult ShowMessageBox(this Form form, string text) {
            return MessageBox.Show(form, text, Application.ProductName);
        }

        public static DialogResult ShowMessageBox(this Form form, string text, string caption, MessageBoxButtons buttons) {
            return MessageBox.Show(form, text, caption, buttons);
        }

        public static DialogResult ShowMessageBox(this Form form, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon) {
            return MessageBox.Show(form, text, caption, buttons, icon);
        }

        public static DialogResult ShowMessageBox(this Form form, string text, MessageBoxButtons buttons) {
            return MessageBox.Show(form, text, Application.ProductName, buttons);
        }

        public static DialogResult ShowMessageBox(this Form form, string text, MessageBoxButtons buttons, MessageBoxIcon icon) {
            return MessageBox.Show(form, text, Application.ProductName, buttons, icon);
        }

    }
}
