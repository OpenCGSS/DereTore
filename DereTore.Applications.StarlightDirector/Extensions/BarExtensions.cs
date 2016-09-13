using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector.Extensions {
    public static class BarExtensions {

        public static double GetStartTime(this Bar bar) {
            var score = bar.Score;
            var scoreSettings = score.Settings;
            var bars = score.Bars;
            var index = bars.IndexOf(bar);
            var time = scoreSettings.StartTimeOffset;
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
            var seconds = ComposerUtilities.BpmToSeconds(bpm);
            return seconds * signature;
        }

        public static int GetActualSignature(this Bar bar) {
            return bar.Params?.UserDefinedSignature ?? bar.Score.Settings.GlobalSignature;
        }

        public static int GetActualGridPerSignature(this Bar bar) {
            return bar.Params?.UserDefinedGridPerSignature ?? bar.Score.Settings.GlobalGridPerSignature;
        }

        public static int GetTotalGridCount(this Bar bar) {
            return GetActualSignature(bar) * GetActualGridPerSignature(bar);
        }

        public static double GetActualBpm(this Bar bar) {
            return bar.Params?.UserDefinedBpm ?? bar.Score.Settings.GlobalBpm;
        }

    }
}
