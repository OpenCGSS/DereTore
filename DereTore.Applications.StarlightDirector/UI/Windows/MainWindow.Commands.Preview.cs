using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using DereTore.Applications.StarlightDirector.UI.Controls.Primitives;

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
                var offset = Project.Settings.StartTimeOffset;
                var fps = PreviewFps;
                var startTime = 0;

                // compute current time
                // find the ScoreNote that appears at the bottom of screen
                // TODO: not very accurate (maybe acceptable?)
                if (!PreviewFromStart)
                {
                    var minY = ScrollViewer.ExtentHeight - ScrollViewer.VerticalOffset - ScrollViewer.ViewportHeight;
                    var notes = Editor.ScoreNotes.OrderBy(sn => sn.Note.HitTiming);
                    ScoreNote note = null;
                    foreach (var sn in notes)
                    {
                        if (sn.Y > minY)
                        {
                            note = sn;
                            break;
                        }
                    }

                    if (note != null)
                    {
                        startTime = (int)(1000 * (note.Note.HitTiming - offset));
                    }
                }

                // delay a second before preview
                // so that the view is switched and the previewer can correctly get dimensions
                var delayThread = new Thread(() =>
                {
                    Thread.Sleep(1000);
                    Dispatcher.Invoke(new Action(() => ScorePreviewer.BeginPreview(Project.Scores[Project.Difficulty], offset, fps, startTime)));
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
