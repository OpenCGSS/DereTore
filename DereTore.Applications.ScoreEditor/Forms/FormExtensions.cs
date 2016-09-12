using System.Windows.Forms;
using App = System.Windows.Forms.Application;

namespace DereTore.Applications.ScoreEditor.Forms {
    internal static class FormExtensions {

        public static DialogResult ShowMessageBox(this Form form, string text) {
            return MessageBox.Show(form, text, App.ProductName);
        }

        public static DialogResult ShowMessageBox(this Form form, string text, string caption, MessageBoxButtons buttons) {
            return MessageBox.Show(form, text, caption, buttons);
        }

        public static DialogResult ShowMessageBox(this Form form, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon) {
            return MessageBox.Show(form, text, caption, buttons, icon);
        }

        public static DialogResult ShowMessageBox(this Form form, string text, MessageBoxButtons buttons) {
            return MessageBox.Show(form, text, App.ProductName, buttons);
        }

        public static DialogResult ShowMessageBox(this Form form, string text, MessageBoxButtons buttons, MessageBoxIcon icon) {
            return MessageBox.Show(form, text, App.ProductName, buttons, icon);
        }

    }
}
