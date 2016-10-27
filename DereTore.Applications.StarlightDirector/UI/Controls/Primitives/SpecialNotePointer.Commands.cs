using System.Globalization;
using System.Windows.Input;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;

namespace DereTore.Applications.StarlightDirector.UI.Controls.Primitives {
    partial class SpecialNotePointer {

        public static readonly ICommand CmdDeleteThis = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdBeginEditBpm = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdConfirmBpm = CommandHelper.RegisterCommand();
        public static readonly ICommand CmdCancelBpm = CommandHelper.RegisterCommand();

        private void CmdDeleteThis_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void CmdDeleteThis_Executed(object sender, ExecutedRoutedEventArgs e) {
            var editor = this.FindVisualParent<ScoreEditor>();
            editor?.RemoveSpecialNote(this);
        }

        private void CmdBeginEditBpm_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = !_isEditing;
        }

        private void CmdBeginEditBpm_Executed(object sender, ExecutedRoutedEventArgs e) {
            SetEditingState(true);
            NewBpmTextBox.Focus();
            NewBpmTextBox.Text = Note.ExtraParams.NewBpm.ToString(CultureInfo.InvariantCulture);
        }

        private void CmdConfirmBpm_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void CmdConfirmBpm_Executed(object sender, ExecutedRoutedEventArgs e) {
            double d;
            var b = double.TryParse(NewBpmTextBox.Text, out d);
            if (!b) {
                return;
            }
            if (d <= 0) {
                d = (double)NoteExtraParams.NewBpmProperty.GetMetadata(typeof(NoteExtraParams)).DefaultValue;
            }
            Note.ExtraParams.NewBpm = d;
            SetEditingState(false);
        }

        private void CmdCancelBpm_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void CmdCancelBpm_Executed(object sender, ExecutedRoutedEventArgs e) {
            SetEditingState(false);
        }

    }
}
