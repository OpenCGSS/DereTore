using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DereTore;
using StarlightDirector.Entities;
using StarlightDirector.Entities.Extensions;
using StarlightDirector.Exchange.Deleste.CommandParameters;
using StarlightDirector.Exchange.Properties;

namespace StarlightDirector.Exchange.Deleste {
    internal static class DelesteHelper {

        public static void AnalyzeBeatmap(Score score, List<DelesteBeatmapEntry> entries, DelesteState initialState, List<string> warnings) {
            var currentHoldingNotes = new Note[5];
            // key: Deleste Group Number
            // value: (value)
            var lastFlickNotes = new Dictionary<int, Note>();
            var lastBasicFlickNotes = new Dictionary<int, DelesteBasicNote>();

            // Prepare the entry params. This params will affect timing computing.
            var state = initialState.Clone();
            var changeBpmCommands = new List<ChangeBpmCommandParameters>();
            foreach (var entry in entries) {
                if (entry.IsCommand) {
                    switch (entry.CommandType) {
                        case DelesteCommand.ChangeBpm:
                            var parameters = (ChangeBpmCommandParameters)entry.CommandParameter;
                            var item = changeBpmCommands.Find(p => p.StartMeasureIndex == parameters.StartMeasureIndex);
                            if (item != null) {
                                // Command parameter override
                                item.NewBpm = parameters.NewBpm;
                            } else {
                                changeBpmCommands.Add(parameters.Clone());
                            }
                            state.BPM = parameters.NewBpm;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                } else {
                    // Reset entry state.
                    entry.BPM = state.BPM;
                    entry.Signature = state.Signature;
                }
            }

            // Here is where the main analysis happens.
            foreach (var entry in entries) {
                if (entry.IsCommand) {
                    continue;
                }

                if (entry.MeasureIndex + 1 > score.Bars.Count) {
                    for (var i = score.Bars.Count; i < entry.MeasureIndex + 1; ++i) {
                        score.Bars.Add(new Bar(score, i));
                    }
                }
                var bar = score.Bars[entry.MeasureIndex];
                var totalBarGridCount = bar.TotalGridCount;

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
                        var warning = string.Format(Resources.DelesteUnfitGridSizePromptTemplate, entry.MeasureIndex, originalEntryFullLength, totalBarGridCount);
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
                        // > ※ただし、1000msを超えると接続されません
                        var timeDiff = CalculateTimingPeriodBetweenNotes(entries, basicNote, lastBasicFlickNotes[entry.GroupID], entry.FullLength);
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

            // Add variant bpm notes to the bar.
            changeBpmCommands.Sort((p1, p2) => p1.StartMeasureIndex.CompareTo(p2.StartMeasureIndex));
            foreach (var p in changeBpmCommands) {
                if (p.StartMeasureIndex >= score.Bars.Count) {
                    break;
                }
                var bar = score.Bars[p.StartMeasureIndex];
                var vbNote = bar.AddNote();
                vbNote.IndexInGrid = 0;
                vbNote.SetSpecialType(NoteType.VariantBpm);
                vbNote.ExtraParams = new NoteExtraParams {
                    Note = vbNote,
                    NewBpm = p.NewBpm
                };
            }

            // Fix sync notes.
            score.FixSyncNotes();

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
            if (line.StartsWithOfGroup(Constants.BeatmapCommands)) {
                line = line.Substring(0, line.IndexOf(' '));
                var warning = string.Format(Resources.DelesteCommandIsNotYetSupportedPromptTemplate, entryCounter, line);
                warnings.Add(warning);
                return null;
            }

            DelesteBeatmapEntry entry;
            var isCommand = HandleCommands(line, temporaryProject, out entry);
            if (isCommand) {
                return entry;
            }
            if (line.Length < 2 || !char.IsNumber(line, 1)) {
                // Not a beatmap entry. May be metadata.
                return null;
            }

            if (line.IndexOf('.') >= 0) {
                hasErrors = true;
                var warning = Resources.DelesteTxtFormat2IsNotSupportedPrompt;
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
                var warning = string.Format(Resources.DelesteNoteCountInconsistentPromptTemplate, entryCounter, measureIndex, standardNoteCount, startPositions.Length, finishPositions.Length);
                warnings.Add(warning);
                return null;
            }
            entry = new DelesteBeatmapEntry();
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
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8)) {
                    var line = string.Empty;
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
                if (!Note.IsTypeGaming(compiledNote.Type)) {
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

        private static bool HandleCommands(string line, Project temporaryProject, out DelesteBeatmapEntry entry) {
            entry = null;
            if (line.StartsWith(Constants.BpmCommand)) {
                var dataText = line.Substring(Constants.BpmCommand.Length + 1);
                var bpm = double.Parse(dataText);
                temporaryProject.Settings.GlobalBpm = bpm;
                return true;
            }
            if (line.StartsWith(Constants.OffsetCommand)) {
                var dataText = line.Substring(Constants.OffsetCommand.Length + 1);
                var offset = double.Parse(dataText);
                offset = Math.Abs(offset);
                // msec -> sec
                temporaryProject.Settings.StartTimeOffset = offset / 1000;
                return true;
            }
            if (line.StartsWith(Constants.ChangeBpmCommand)) {
                var dataText = line.Substring(Constants.ChangeBpmCommand.Length + 1);
                var commaSplittedValues = dataText.Split(',');
                var measureIndex = double.Parse(commaSplittedValues[0]);
                var newBpm = double.Parse(commaSplittedValues[1]);
                entry = new DelesteBeatmapEntry {
                    IsCommand = true,
                    CommandType = DelesteCommand.ChangeBpm,
                    CommandParameter = new ChangeBpmCommandParameters {
                        StartMeasureIndex = (int)measureIndex,
                        NewBpm = newBpm
                    }
                };
                return true;
            }
            return false;
        }

        private static double CalculateTimingPeriodBetweenNotes(List<DelesteBeatmapEntry> entries, DelesteBasicNote currentBasicFlickNote, DelesteBasicNote lastBasicFlickNote, int fullLength) {
            double timeDiff;
            double timePerBeat;
            DelesteBeatmapEntry entry;
            if (currentBasicFlickNote.Entry.MeasureIndex != lastBasicFlickNote.Entry.MeasureIndex) {
                var startIndex = entries.IndexOf(lastBasicFlickNote.Entry);
                var endIndex = entries.IndexOf(currentBasicFlickNote.Entry, startIndex + 1);

                entry = lastBasicFlickNote.Entry;
                timePerBeat = DirectorHelper.BpmToSeconds(entry.BPM);
                timeDiff = timePerBeat * entry.Signature * (1 - (float)lastBasicFlickNote.IndexInMeasure / lastBasicFlickNote.Entry.FullLength);
                for (var i = startIndex + 1; i < endIndex; ++i) {
                    entry = entries[i];
                    if (entry.IsCommand) {
                        continue;
                    }
                    timePerBeat = DirectorHelper.BpmToSeconds(entry.BPM);
                    timeDiff += timePerBeat * entry.Signature;
                }
                entry = currentBasicFlickNote.Entry;
                timePerBeat = DirectorHelper.BpmToSeconds(entry.BPM);
                timeDiff += timePerBeat * entry.Signature * ((float)currentBasicFlickNote.IndexInMeasure / fullLength);
            } else {
                entry = currentBasicFlickNote.Entry;
                timePerBeat = DirectorHelper.BpmToSeconds(entry.BPM);
                timeDiff = timePerBeat * entry.Signature * ((float)currentBasicFlickNote.IndexInMeasure / fullLength - (float)lastBasicFlickNote.IndexInMeasure / lastBasicFlickNote.Entry.FullLength);
            }
            return timeDiff;
        }

    }
}
