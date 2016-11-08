using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace DereTore.Applications.StarlightDirector.UI.Windows
{
    partial class MainWindow
    {
        public static readonly ICommand CmdPreviewToggle = CommandHelper.RegisterCommand("F5");

        private void CmdPreviewToggle_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            IsPreviewing = !IsPreviewing;
            if (IsPreviewing)
            {
                // TODO: play music
                ScorePreviewer.BeginPreview(Project.Scores[Project.Difficulty]);
            }
            else
            {
                ScorePreviewer.EndPreview();
            }
        }

        private void CmdPreviewToggle_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Editor.Score != null;
        }
    }
}
