using System.Windows.Input;
using StarlightDirector.Entities;

namespace StarlightDirector.UI.Windows {
    partial class MainWindow {

        public static readonly ICommand CmdContextAddSpecialNoteVariantBpm = CommandHelper.RegisterCommand();

        private void CmdContextAddSpecialNoteVariantBpm_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null && Editor.LastHitTestInfo != null && Editor.LastHitTestInfo.ScoreBar != null;
        }

        private void CmdContextAddSpecialNoteVariantBpm_Executed(object sender, ExecutedRoutedEventArgs e) {
            var hitTestInfo = Editor.LastHitTestInfo;
            if (hitTestInfo == null) {
                return;
            }
            Editor.AddSpecialNote(hitTestInfo.ScoreBar, hitTestInfo, NoteType.VariantBpm);
        }

    }
}
