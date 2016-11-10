using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
                // delay a second before preview
                // so that the view is switched and the previewer can correctly get dimensions
                var delayThread = new Thread(() =>
                {
                    Thread.Sleep(1000);
                    // TODO: play music
                    Dispatcher.Invoke(new Action(() => ScorePreviewer.BeginPreview(Project.Scores[Project.Difficulty])));
                });
                delayThread.Start();
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
