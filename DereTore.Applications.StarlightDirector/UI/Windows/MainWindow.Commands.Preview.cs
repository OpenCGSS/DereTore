using System;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using DereTore.Applications.StarlightDirector.UI.Controls.Primitives;

namespace DereTore.Applications.StarlightDirector.UI.Windows {
    partial class MainWindow {

        public static readonly ICommand CmdPreviewToggle = CommandHelper.RegisterCommand("F5");

        // Speed (Approach Rate, in [0, 10]) -> Approach Time (ms)
        // Adapted from osu!
        private static readonly int[] ArTable = { 1800, 1680, 1560, 1440, 1320, 1200, 1050, 900, 750, 600, 450 };

        private void CmdPreviewToggle_Executed(object sender, ExecutedRoutedEventArgs e) {
            ScorePreviewer.IsPreviewing = !ScorePreviewer.IsPreviewing;
            if (ScorePreviewer.IsPreviewing) {
                var fps = PreviewFps;
                var startTime = 0;

                if (!PreviewFromStart && ScrollViewer.ExtentHeight > 0)
                {
                    var perc = (ScrollViewer.ExtentHeight - ScrollViewer.VerticalOffset)/ScrollViewer.ExtentHeight;
                    var lastBar = Editor.ScoreBars.LastOrDefault();

                    if (lastBar != null)
                    {
                        startTime = (int) (perc*(lastBar.Bar.StartTime + lastBar.Bar.TimeLength)*1000);
                    }
                }

                startTime += (int)(PreviewStartOffset*1000);
                if (startTime < 0)
                    startTime = 0;

                double approachTime = ArTable[PreviewSpeed];
                ScorePreviewer.BeginPreview(Project.Scores[Project.Difficulty], fps, startTime, approachTime);
            } else {
                ScorePreviewer.EndPreview();
            }
        }

        private void CmdPreviewToggle_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

    }
}
