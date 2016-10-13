using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;

namespace DereTore.Applications.StarlightDirector.Conversion.Formats.Deleste {
    internal static class DelesteHelper {

        public static void AnalyzeBeatmap(Score score, List<DelesteBeatmapEntry> entries, List<string> warnings) {
            var currentHoldingNotes = new Note[5];
            // key: Deleste Group Number
            // value: (value)
            var lastFlickNotes = new Dictionary<int, Note>();
            var lastBasicFlickNotes = new Dictionary<int, DelesteBasicNote>();

            foreach (var entry in entries) {
                if (entry.MeasureIndex + 1 > score.Bars.Count) {
                    for (var i = score.Bars.Count; i < entry.MeasureIndex + 1; ++i) {
                        score.Bars.Add(new Bar(score, i));
                    }
                }
                var bar = score.Bars[entry.MeasureIndex];
                var bpm = bar.GetActualBpm();
                var timePerBeat = DirectorUtilities.BpmToSeconds(bpm);
                var totalBarGridCount = bar.GetTotalGridCount();

                if (entry.FullLength > 0 && !totalBarGridCount.IsMultipleOf(entry.FullLength)) {
                    // Try to fit in the grid. E.g. Yumeiro Harmony measure #041.
                    var lengthFactors = new List<uint> {
                        (uint)entry.FullLength
                    };
                    lengthFactors.AddRange(entry.Notes.Select(note => (uint)note.IndexInMeasure));
                    var factor = (int)MathHelper.GreatestCommonFactor(lengthFactors.Where(n => n != 0).ToArray());
                    var originalEntryFullLength = entry.FullLength;
                    if (factor > 1) {
                        entry.FullLength /= factor;
                        foreach (var basicNote in entry.Notes) {
                            basicNote.IndexInMeasure /= factor;
                        }
                    }
                    if (!totalBarGridCount.IsMultipleOf(entry.FullLength)) {
                        var warning = string.Format(Application.Current.FindResource<string>(App.ResourceKeys.DelesteUnfitGridSizePromptTemplate),
                            entry.MeasureIndex, originalEntryFullLength, totalBarGridCount);
                        warnings.Add(warning);
                        continue;
                    }
                }

                foreach (var basicNote in entry.Notes) {
                    if (!basicNote.IsFlick) {
                        lastFlickNotes[entry.GroupID] = null;
                    }
                    var note = bar.AddNote();
                    note.IndexInGrid = (int)(totalBarGridCount * ((float)basicNote.IndexInMeasure / entry.FullLength));
                    note.StartPosition = basicNote.StartPosition;
                    note.FinishPosition = basicNote.FinishPosition;
                    var positionInTrack = basicNote.IndexInTrack;

                    if (basicNote.IsHoldStart) {
                        // The '<' part fixes this example: (credits to @inosuke01)
                        // #0,043:330000002020*4*000:42511:12132
                        // #1,043:4000202040002000:11211:44544
                        // #4,043:0001*1*00000000000:32
                        // Without this judgement, the two notes marked with '*' will be wrongly connected.
                        if (currentHoldingNotes[positionInTrack] != null && currentHoldingNotes[positionInTrack] < note) {
                            // Invalid holding notes: I haven't released my finger yet!
                            // OK, accept it, set the newly added note as hold end.
                            Note.ConnectHold(currentHoldingNotes[positionInTrack], note);
                            currentHoldingNotes[positionInTrack] = null;
                        } else {
                            currentHoldingNotes[positionInTrack] = note;
                        }
                        lastFlickNotes[entry.GroupID] = null;
                        lastBasicFlickNotes[entry.GroupID] = null;
                        continue;
                    }
                    if (currentHoldingNotes[positionInTrack] != null && currentHoldingNotes[positionInTrack] < note) {
                        Note.ConnectHold(currentHoldingNotes[positionInTrack], note);
                        currentHoldingNotes[positionInTrack] = null;
                        if (basicNote.IsFlick) {
                            note.FlickType = basicNote.IsFlickLeft ? NoteFlickType.FlickLeft : NoteFlickType.FlickRight;
                        }
                    }
                    if (basicNote.IsTap) {
                        lastFlickNotes[entry.GroupID] = null;
                        lastBasicFlickNotes[entry.GroupID] = null;
                        continue;
                    }
                    if (basicNote.IsFlick) {
                        if (!lastFlickNotes.ContainsKey(entry.GroupID) || !lastBasicFlickNotes.ContainsKey(entry.GroupID) ||
                            lastFlickNotes[entry.GroupID] == null || lastBasicFlickNotes[entry.GroupID] == null) {
                            lastFlickNotes[entry.GroupID] = note;
                            lastBasicFlickNotes[entry.GroupID] = basicNote;
                            continue;
                        }
                        // Whether the next note is a traditional flick or not, set it.
                        lastFlickNotes[entry.GroupID].FlickType = lastBasicFlickNotes[entry.GroupID].IsFlickLeft ? NoteFlickType.FlickLeft : NoteFlickType.FlickRight;
                        // We haven implemented Free Flick Mode so we assume all the notes are in Restricted Flick Mode (as in real gaming).
                        var requestedFlickDirection = lastBasicFlickNotes[entry.GroupID].IsFlickLeft ? -1 : 1;
                        var actualFlickDirection = (int)basicNote.FinishPosition - (int)lastBasicFlickNotes[entry.GroupID].FinishPosition;
                        // <0: Current note is not in the requested trail. (e.g. right vs 5->1)
                        // =0: Current note is not in the requested trail. (e.g. right vs 3->3)
                        // >0: Current note is in the requested trail. (e.g. right vs 1->2)
                        if (actualFlickDirection * requestedFlickDirection <= 0) {
                            lastFlickNotes[entry.GroupID] = note;
                            lastBasicFlickNotes[entry.GroupID] = basicNote;
                            continue;
                        }
                        // TODO: Here I use a simple way to calculate time difference. It only applies to constant BPM.
                        double timeDiff;
                        if (basicNote.Entry.MeasureIndex != lastBasicFlickNotes[entry.GroupID].Entry.MeasureIndex) {
                            timeDiff = timePerBeat * Deleste.DefaultSignature * (basicNote.Entry.MeasureIndex - lastBasicFlickNotes[entry.GroupID].Entry.MeasureIndex - 1);
                            timeDiff += timePerBeat * Deleste.DefaultSignature * ((float)basicNote.IndexInMeasure / entry.FullLength);
                            timeDiff += timePerBeat * Deleste.DefaultSignature * (1 - (float)lastBasicFlickNotes[entry.GroupID].IndexInMeasure / lastBasicFlickNotes[entry.GroupID].Entry.FullLength);
                        } else {
                            timeDiff = timePerBeat * Deleste.DefaultSignature * ((float)basicNote.IndexInMeasure / entry.FullLength - (float)lastBasicFlickNotes[entry.GroupID].IndexInMeasure / lastBasicFlickNotes[entry.GroupID].Entry.FullLength);
                        }
                        // > ※ただし、1000msを超えると接続されません
                        if (timeDiff > 1) {
                            lastFlickNotes[entry.GroupID] = note;
                            lastBasicFlickNotes[entry.GroupID] = basicNote;
                            continue;
                        }
                        Note.ConnectFlick(lastFlickNotes[entry.GroupID], note);
                        lastFlickNotes[entry.GroupID] = note;
                        lastBasicFlickNotes[entry.GroupID] = basicNote;
                    }
                }
            }

            // Fix sync notes.
            foreach (var bar in score.Bars) {
                var notes = bar.Notes;
                var distinctGridLineIndices = notes.Select(note => note.IndexInGrid).Distinct().ToArray();
                if (distinctGridLineIndices.Length == notes.Count) {
                    continue;
                }
                // There are sync notes in this bar.
                notes.Sort(Note.TimingComparison);
                for (var i = 0; i < bar.Notes.Count - 1; ++i) {
                    if (notes[i].IndexInGrid == notes[i + 1].IndexInGrid) {
                        Note.ConnectSync(notes[i], notes[i + 1]);
                        ++i;
                    }
                }
            }

            // Fix hold notes: if any line crosses other note(s). (Is it necessary? Deleste files seem to be organized well.)
            foreach (var note in score.Notes) {
                if (note.IsHoldStart) {
                    var between = score.Notes.GetFirstNoteBetween(note, note.HoldTarget);
                    if (between != null) {
                        note.HoldTarget = between;
                    }
                }
            }
        }

        public static DelesteBeatmapEntry ReadEntry(Project temporaryProject, string line, int entryCounter, List<DelesteBasicNote> noteCache, List<string> warnings, ref bool hasErrors) {
            line = line.ToLowerInvariant();
            if (line.StartsWithOfGroup(Deleste.BeatmapCommands)) {
                line = line.Substring(0, line.IndexOf(' '));
                var warning = string.Format(Application.Current.FindResource<string>(App.ResourceKeys.DelesteCommandIsNotYetSupportedPromptTemplate),
                    entryCounter, line);
                warnings.Add(warning);
                return null;
            }

            if (line.StartsWith(Deleste.BpmCommand)) {
                var dataText = line.Substring(Deleste.BpmCommand.Length + 1);
                var bpm = double.Parse(dataText);
                temporaryProject.Settings.GlobalBpm = bpm;
                return null;
            }
            if (line.StartsWith(Deleste.OffsetCommand)) {
                var dataText = line.Substring(Deleste.OffsetCommand.Length + 1);
                var offset = int.Parse(dataText);
                // msec -> sec
                temporaryProject.Settings.StartTimeOffset = (double)offset / 1000;
                return null;
            }
            if (line.Length < 2 || !char.IsNumber(line, 1)) {
                // Not a beatmap entry. May be metadata.
                return null;
            }

            if (line.IndexOf('.') >= 0) {
                hasErrors = true;
                var warning = Application.Current.FindResource<string>(App.ResourceKeys.DelesteTxtFormat2IsNotSupportedPrompt);
                warnings.Add(warning);
                return null;
            }

            // #gid,mid:types&indices:sp[:fp]
            var colonStringValues = line.Substring(1).Split(':');
            var commaStringValues = colonStringValues[0].Split(',');
            var noteDistribution = colonStringValues[1];
            var standardNoteCount = noteDistribution.Count(ch => ch != '0');

            // Abbreviated format (1, 2) & full format
            // #0,000:2
            // #0,000:2222:1234
            // #0,001:2222:3333:3333
            var groupNumberString = commaStringValues[0];
            var measureIndexString = commaStringValues[1];
            string finishPositions, startPositions;
            switch (colonStringValues.Length) {
                case 2:
                    startPositions = finishPositions = new string('3', standardNoteCount);
                    break;
                case 3:
                    startPositions = finishPositions = colonStringValues[2];
                    break;
                default:
                    startPositions = colonStringValues[2];
                    finishPositions = colonStringValues[3];
                    break;
            }
            noteCache.Clear();

            var measureIndex = Convert.ToInt32(measureIndexString);
            if (standardNoteCount != startPositions.Length || startPositions.Length != finishPositions.Length) {
                var warning = string.Format(Application.Current.FindResource<string>(App.ResourceKeys.DelesteNoteCountInconsistentPromptTemplate),
                    entryCounter, measureIndex, standardNoteCount, startPositions.Length, finishPositions.Length);
                warnings.Add(warning);
                return null;
            }
            var entry = new DelesteBeatmapEntry();
            entry.GroupID = Convert.ToInt32(groupNumberString);
            entry.MeasureIndex = measureIndex;
            entry.FullLength = noteDistribution.Length;
            int i = -1, j = -1;
            foreach (var ch in noteDistribution) {
                ++j;
                if (ch == '0') {
                    continue;
                }
                ++i;
                var note = new DelesteBasicNote(entry);
                note.IndexInMeasure = j;
                note.Type = (DelesteNoteType)(ch - '0');
                note.StartPosition = (NotePosition)(startPositions[i] - '0');
                note.FinishPosition = (NotePosition)(finishPositions[i] - '0');
                noteCache.Add(note);
            }
            entry.Notes = noteCache.ToArray();
            return entry;
        }

        public static Encoding TryDetectBeatmapEncoding(string fileName) {
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

        public static void WriteBeatmapHeader(Score score, StreamWriter writer) {
            writer.WriteLine("#utf8");
            writer.WriteLine("#Title (Title Here)");
            writer.WriteLine("#Lyricist (Lyricist)");
            writer.WriteLine("#Composer (Composer)");
            writer.WriteLine("#Background background.jpg");
            writer.WriteLine("#Song song.ogg");
            writer.WriteLine("#Lyrics lyrics.lyr");
            // Using the 240/0 preset as discussed at 2016-10-09. May be updated when a new version of Deleste Viewer is released.
            //writer.WriteLine("#BPM {0:F2}", score.Project.Settings.GlobalBpm);
            //writer.WriteLine("#Offset {0}", (int)Math.Round(score.Project.Settings.StartTimeOffset * 1000));
            writer.WriteLine("#BPM 240");
            writer.WriteLine("#Offset 0");
            string s;
            switch (score.Difficulty) {
                case Difficulty.Debut:
                case Difficulty.Regular:
                case Difficulty.Pro:
                case Difficulty.Master:
                    s = score.Difficulty.ToString();
                    break;
                case Difficulty.MasterPlus:
                    s = "Master+";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            writer.WriteLine("#Difficulty {0}", s);
            switch (score.Difficulty) {
                case Difficulty.Debut:
                    s = "7";
                    break;
                case Difficulty.Regular:
                    s = "13";
                    break;
                case Difficulty.Pro:
                    s = "17";
                    break;
                case Difficulty.Master:
                    s = "23";
                    break;
                case Difficulty.MasterPlus:
                    s = "30";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            writer.WriteLine("#Level {0}", s);
            writer.WriteLine("#BGMVolume 100");
            writer.WriteLine("#SEVolume 100");
            writer.WriteLine("#Attribute All");
            // Use the undocumented "Convert" format for CSV-converted beatmaps.
            writer.WriteLine("#Format Convert");
            writer.WriteLine();
        }

        public static void WriteEntries(Score score, StreamWriter writer) {
            var compiledScore = score.Compile();
            // group_id,measure_index:deleste_note_type:start_position:finish_position
            // * measure_index can be floating point numbers: 000.000000
            foreach (var compiledNote in compiledScore.Notes) {
                if (!(compiledNote.Type == NoteType.TapOrFlick || compiledNote.Type == NoteType.Hold)) {
                    continue;
                }
                writer.WriteLine("#{0},{1:000.000000}:{2}:{3}:{4}", compiledNote.FlickGroupID, compiledNote.HitTiming, (int)TranslateNoteType(compiledNote), (int)compiledNote.StartPosition, (int)compiledNote.FinishPosition);
            }
        }

        private static DelesteNoteType TranslateNoteType(CompiledNote note) {
            switch (note.Type) {
                case NoteType.TapOrFlick:
                    switch (note.FlickType) {
                        case (int)NoteFlickType.Tap:
                            return DelesteNoteType.Tap;
                        case (int)NoteFlickType.FlickLeft:
                            return DelesteNoteType.FlickLeft;
                        case (int)NoteFlickType.FlickRight:
                            return DelesteNoteType.FlickRight;
                        default:
                            // Should have thrown an exception.
                            return DelesteNoteType.Tap;
                    }
                case NoteType.Hold:
                    return DelesteNoteType.Hold;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }
}
