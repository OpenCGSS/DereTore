using System.Collections.Generic;
using System.IO;
using System.Text;
using DereTore.Applications.StarlightDirector.Conversion.Formats.Deleste;
using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector.Conversion {
    public static class ScoreIO {

        public static Score LoadFromDelesteBeatmap(Project project, Difficulty difficulty, string fileName, out string[] warnings) {
            warnings = null;
            var encoding = TryDetectDelesteBeatmapEncoding(fileName);
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read)) {
                using (var streamReader = new StreamReader(fileStream, encoding, true)) {
                    if (streamReader.EndOfStream) {
                        return null;
                    }
                    var score = new Score(project, difficulty);
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
                        var entry = DelesteHelper.ReadEntry(score, line, entryCounter, noteCache, warningList);
                        if (entry != null) {
                            entryCache.Add(entry);
                        }
                    } while (!streamReader.EndOfStream);
                    DelesteHelper.AnalyzeDelesteBeatmap(score, entryCache, warningList);
                    if (warningList.Count > 0) {
                        warnings = warningList.ToArray();
                    }
                    return score;
                }
            }
        }

        private static Encoding TryDetectDelesteBeatmapEncoding(string fileName) {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read)) {
                // Fallback to default platform encoding.
                using (var streamReader = new StreamReader(fileStream, Encoding.Default)) {
                    string line = string.Empty;
                    if (!streamReader.EndOfStream) {
                        do {
                            line = streamReader.ReadLine();
                        } while (line.Length > 0 && line[0] != '#' && !streamReader.EndOfStream);
                    }
                    line = line.ToLowerInvariant();
                    if (!string.IsNullOrEmpty(line)) {
                        if (line == "#utf8" || line == "#utf-8") {
                            return Encoding.UTF8;
                        } else {
                            // According to the help of Deleste:
                            // 
                            // > 譜面ファイルの文字コードは原則「Shift-JIS」を使用してください。
                            // > 例外的に「UTF-8」のみ使用できます。
                            // > 使用する場合、テキストファイルの先頭に「#utf8」又は「#utf-8」と記述してください。
                            return Encoding.GetEncoding("Shift-JIS");
                        }
                    } else {
                        return Encoding.GetEncoding("Shift-JIS");
                    }
                }
            }
        }

    }
}
