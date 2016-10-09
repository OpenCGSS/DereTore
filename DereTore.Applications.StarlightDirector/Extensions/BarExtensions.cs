using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector.Extensions {
    public static class BarExtensions {

        public static double GetStartTime(this Bar bar) {
            var score = bar.Score;
            var bars = score.Bars;
            var settings = bar.Score.Project.Settings;
            var index = bars.IndexOf(bar);
            var time = settings.StartTimeOffset;
            var i = 0;
            foreach (var b in bars) {
                if (i >= index) {
                    break;
                }
                time += b.GetLength();
                ++i;
            }
            return time;
        }

        public static double GetLength(this Bar bar) {
            var bpm = bar.GetActualBpm();
            var signature = bar.GetActualSignature();
            var seconds = DirectorUtilities.BpmToSeconds(bpm);
            return seconds * signature;
        }

        public static int GetActualSignature(this Bar bar) {
            return bar.Params?.UserDefinedSignature ?? bar.Score.Project.Settings.GlobalSignature;
        }

        public static int GetActualGridPerSignature(this Bar bar) {
            return bar.Params?.UserDefinedGridPerSignature ?? bar.Score.Project.Settings.GlobalGridPerSignature;
        }

        public static int GetTotalGridCount(this Bar bar) {
            return GetActualSignature(bar) * GetActualGridPerSignature(bar);
        }

        public static double GetActualBpm(this Bar bar) {
            return bar.Params?.UserDefinedBpm ?? bar.Score.Project.Settings.GlobalBpm;
        }

    }
}
