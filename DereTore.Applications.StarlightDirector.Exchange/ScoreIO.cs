using System.Collections.Generic;
using System.IO;
using System.Text;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Exchange.Deleste;

namespace DereTore.Applications.StarlightDirector.Exchange {
    public static class ScoreIO {

        public static Score LoadFromDelesteBeatmap(Project temporaryProject, Difficulty difficulty, string fileName, out string[] warnings, out bool hasErrors) {
            warnings = null;
            hasErrors = false;
            var encoding = DelesteHelper.TryDetectBeatmapEncoding(fileName);
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read)) {
                using (var streamReader = new StreamReader(fileStream, encoding, true)) {
                    if (streamReader.EndOfStream) {
                        return null;
                    }
                    var score = new Score(temporaryProject, difficulty);
                    var noteCache = new List<DelesteBasicNote>();
                    var entryCache = new List<DelesteBeatmapEntry>();
                    var warningList = new List<string>();
                    var entryCounter = 0;
                    do {
                        var line = streamReader.ReadLine();
                        if (line.Length == 0 || line[0] != '#') {
                            continue;
                        }
                        ++entryCounter;
                        var entry = DelesteHelper.ReadEntry(temporaryProject, line, entryCounter, noteCache, warningList, ref hasErrors);
                        if (hasErrors) {
                            warnings = warningList.ToArray();
                            return null;
                        }
                        if (entry != null) {
                            entryCache.Add(entry);
                        }
                    } while (!streamReader.EndOfStream);
                    DelesteHelper.AnalyzeBeatmap(score, entryCache, warningList);
                    if (warningList.Count > 0) {
                        warnings = warningList.ToArray();
                    }
                    return score;
                }
            }
        }

        public static void ExportToDelesteBeatmap(Score score, string fileName) {
            using (var fileStream = File.Open(fileName, FileMode.Create, FileAccess.Write)) {
                // Is there any way to achieve UTF-8 w/o BOM?
                using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8)) {
                    streamWriter.NewLine = Deleste.Constants.NewLine;
                    DelesteHelper.WriteBeatmapHeader(score, streamWriter);
                    DelesteHelper.WriteEntries(score, streamWriter);
                }
            }
        }

    }
}
