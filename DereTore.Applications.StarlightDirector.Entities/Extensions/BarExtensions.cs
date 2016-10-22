using System;
using System.Collections.Generic;
using System.Linq;

namespace DereTore.Applications.StarlightDirector.Entities.Extensions {
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
                time += b.GetTimeLength();
                ++i;
            }
            return time;
        }

        public static double GetTimeLength(this Bar bar) {
            var startBpm = bar.GetStartBpm();
            var signature = bar.GetActualSignature();
            if (bar.Notes.All(note => note.Type != NoteType.VariantBpm)) {
                // If BPM doesn't change in this measure, things are simple.
                var seconds = DirectorHelper.BpmToSeconds(startBpm);
                return seconds * signature;
            }
            // If not, we have to do some math.
            var bpmPairs = new List<Tuple<int, double>> {
                new Tuple<int, double>(0, startBpm)
            };
            var lastBpm = startBpm;
            foreach (var note in bar.Notes.Where(note => note.Type == NoteType.VariantBpm)) {
                var newBpm = note.ExtraParams.NewBpm;
                bpmPairs.Add(new Tuple<int, double>(note.IndexInGrid, newBpm));
                lastBpm = newBpm;
            }
            var totalGridCount = GetTotalGridCount(bar);
            bpmPairs.Add(new Tuple<int, double>(totalGridCount, lastBpm));
            bpmPairs.Sort((t1, t2) => t1.Item1.CompareTo(t2.Item1));
            var length = 0d;
            Tuple<int, double> lastBpmPair = null;
            foreach (var bpmPair in bpmPairs) {
                if (lastBpmPair == null) {
                    lastBpmPair = bpmPair;
                    continue;
                }
                var deltaGridCount = bpmPair.Item1 - lastBpmPair.Item1;
                var seconds = DirectorHelper.BpmToSeconds(bpmPair.Item2);
                var timeSlice = seconds * signature * deltaGridCount / totalGridCount;
                length += timeSlice;
                lastBpmPair = bpmPair;
            }
            return length;
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

        public static double GetStartBpm(this Bar bar) {
            var b = bar;
            while (b.Index > 0) {
                b = b.Score.Bars[b.Index - 1];
                if (b.Notes.All(note => note.Type != NoteType.VariantBpm)) {
                    continue;
                }
                var list = new List<Note>(b.Notes);
                list.Sort(Note.TimingComparison);
                var bpmNote = list.FindLast(note => note.Type == NoteType.VariantBpm);
                return bpmNote.ExtraParams.NewBpm;
            }
            return bar.Score.Project.Settings.GlobalBpm;
        }

    }
}
