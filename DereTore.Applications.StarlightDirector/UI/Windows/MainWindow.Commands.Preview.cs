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
                var offset = Project.Settings.StartTimeOffset;
                var fps = PreviewFps;
                var startTime = 0;

                // compute current time
                // find the ScoreNote that appears at the bottom of screen
                // TODO: not very accurate (maybe acceptable?)
                if (!PreviewFromStart) {
                    var minY = ScrollViewer.ExtentHeight - ScrollViewer.VerticalOffset - ScrollViewer.ViewportHeight;
                    var notes = Editor.ScoreNotes.OrderBy(sn => sn.Note.HitTiming);
                    var note = notes.FirstOrDefault(sn => sn.Y > minY);

                    if (note != null) {
                        startTime = (int)(1000 * (note.Note.HitTiming - offset));
                    }
                }

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
