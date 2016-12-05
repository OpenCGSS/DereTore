using System.Linq;
using System.Windows.Input;
using DereTore.Applications.StarlightDirector.Entities;

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
                var score = Project.Scores[Project.Difficulty];

                // find start time, accurate even if BPM is variant
                if (!PreviewFromStart && ScrollViewer.ExtentHeight > 0)
                {
                    /*
                     * First, we find the percentage of scroll
                     * Since bar margin is constant, we can compute targetGrid = total #grids * percentage, which is the grid we should start at
                     * Then, we find where this grid is and set startTime to be its time
                     */
                    var perc = (ScrollViewer.ExtentHeight - ScrollViewer.VerticalOffset - ScrollViewer.ViewportHeight)/ScrollViewer.ExtentHeight;
                    var lastBar = Editor.ScoreBars.LastOrDefault();

                    var totalGrids = 0;
                    foreach (var bar in score.Bars)
                    {
                        totalGrids += bar.TotalGridCount;
                    }

                    if (perc > 0 && lastBar != null)
                    {
                        var projectOffset = Project.Settings.StartTimeOffset;
                        var targetGrid = totalGrids * perc;
                        var bar = score.Bars[0];
                        var gridSum = 0;
                        for (int i = 1; i < score.Bars.Count; ++i)
                        {
                            gridSum += score.Bars[i].TotalGridCount;
                            if (gridSum > targetGrid)
                            {
                                gridSum -= score.Bars[i].TotalGridCount;
                                bar = score.Bars[i - 1];
                                break;
                            }
                        }

                        for (int i = 1; i < bar.TotalGridCount; ++i)
                        {
                            ++gridSum;
                            if (gridSum > targetGrid)
                            {
                                startTime = (int) (1000*(bar.TimeAtGrid(i - 1) - projectOffset) + 1000*projectOffset);
                                break;
                            }
                        }
                    }
                }

                startTime += (int)(PreviewStartOffset*1000);
                if (startTime < 0)
                    startTime = 0;

                double approachTime = ArTable[PreviewSpeed];
                ScorePreviewer.BeginPreview(score, fps, startTime, approachTime);
            } else {
                ScorePreviewer.EndPreview();
            }
        }

        private void CmdPreviewToggle_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = Editor.Score != null;
        }

    }
}
