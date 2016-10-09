using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;

namespace DereTore.Applications.StarlightDirector.Conversion.Formats.Deleste {
    internal static class DelesteHelper {

        public static void AnalyzeDelesteBeatmap(Score score, List<DelesteBeatmapEntry> entries, List<string> warnings) {
            var currentHoldingNotes = new Note[5];
            Note lastFlickNote = null;
            DelesteBasicNote lastBasicFlickNote = null;

            foreach (var entry in entries) {
                if (entry.MeasureIndex + 1 > score.Bars.Count) {
                    for (var i = score.Bars.Count; i < entry.MeasureIndex + 1; ++i) {
                        score.Bars.Add(new Bar(score, entry.MeasureIndex));
                    }
                }
                var bar = score.Bars[entry.MeasureIndex];
                var bpm = bar.GetActualBpm();
                var timePerBeat = DirectorUtilities.BpmToSeconds(bpm);
                var totalBarGridCount = bar.GetTotalGridCount();

                if (!totalBarGridCount.IsMultipleOf(entry.FullLength)) {
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
                        lastFlickNote = null;
                    }
                    var note = bar.AddNote();
                    note.PositionInGrid = (int)(totalBarGridCount * ((float)basicNote.IndexInMeasure / entry.FullLength));
                    note.StartPosition = basicNote.StartPosition;
                    note.FinishPosition = basicNote.FinishPosition;
                    var positionInTrack = basicNote.PositionInTrack;

                    if (basicNote.IsHoldStart) {
                        if (currentHoldingNotes[positionInTrack] != null) {
                            // Invalid holding notes: I haven't released my finger yet!
                            // OK, accept it, set the newly added note as hold end.
                            Note.ConnectHold(currentHoldingNotes[positionInTrack], note);
                            currentHoldingNotes[positionInTrack] = null;
                        } else {
                            currentHoldingNotes[positionInTrack] = note;
                        }
                        lastFlickNote = null;
                        lastBasicFlickNote = null;
                        continue;
                    }
                    if (currentHoldingNotes[positionInTrack] != null) {
                        Note.ConnectHold(currentHoldingNotes[positionInTrack], note);
                        currentHoldingNotes[positionInTrack] = null;
                        if (basicNote.IsFlick) {
                            note.FlickType = basicNote.IsFlickLeft ? NoteFlickType.FlickLeft : NoteFlickType.FlickRight;
                        }
                    }
                    if (basicNote.IsTap) {
                        lastFlickNote = null;
                        lastBasicFlickNote = null;
                        continue;
                    }
                    if (basicNote.IsFlick) {
                        if (lastFlickNote == null || lastBasicFlickNote == null) {
                            lastFlickNote = note;
                            lastBasicFlickNote = basicNote;
                            continue;
                        }
                        // Whether the next note is a traditional flick or not, set it.
                        lastFlickNote.FlickType = lastBasicFlickNote.IsFlickLeft ? NoteFlickType.FlickLeft : NoteFlickType.FlickRight;
                        if (lastBasicFlickNote.Entry.GroupID != entry.GroupID) {
                            lastFlickNote = note;
                            lastBasicFlickNote = basicNote;
                            continue;
                        }
                        // We haven implemented Free Flick Mode so we assume all the notes are in Restricted Flick Mode (as in real gaming).
                        var requestedFlickDirection = lastBasicFlickNote.IsFlickLeft ? -1 : 1;
                        var actualFlickDirection = (int)basicNote.FinishPosition - (int)lastBasicFlickNote.FinishPosition;
                        // <0: Current note is not in the requested trail. (e.g. right vs 5->1)
                        // =0: Current note is not in the requested trail. (e.g. right vs 3->3)
                        // >0: Current note is in the requested trail. (e.g. right vs 1->2)
                        if (actualFlickDirection * requestedFlickDirection <= 0) {
                            lastFlickNote = note;
                            lastBasicFlickNote = basicNote;
                            continue;
                        }
                        // TODO: Here I use a simple way to calculate time difference. It only applies to constant BPM.
                        double timeDiff;
                        if (basicNote.Entry.MeasureIndex != lastBasicFlickNote.Entry.MeasureIndex) {
                            timeDiff = timePerBeat * Deleste.DefaultSignature * (basicNote.Entry.MeasureIndex - lastBasicFlickNote.Entry.MeasureIndex);
                            timeDiff += timePerBeat * Deleste.DefaultSignature * ((float)basicNote.IndexInMeasure / entry.FullLength);
                            timeDiff += timePerBeat * Deleste.DefaultSignature * (1 - (float)lastBasicFlickNote.IndexInMeasure / lastBasicFlickNote.Entry.FullLength);
                        } else {
                            timeDiff = timePerBeat * Deleste.DefaultSignature * ((float)basicNote.IndexInMeasure / entry.FullLength - (float)lastBasicFlickNote.IndexInMeasure / lastBasicFlickNote.Entry.FullLength);
                        }
                        // > ※ただし、1000msを超えると接続されません
                        if (timeDiff > 1) {
                            lastFlickNote = note;
                            lastBasicFlickNote = basicNote;
                            continue;
                        }
                        Note.ConnectFlick(lastFlickNote, note);
                        lastFlickNote = note;
                        lastBasicFlickNote = basicNote;
                    }
                }
            }

            // Fix sync notes.
            foreach (var bar in score.Bars) {
                var notes = bar.Notes;
                var distinctGridLineIndices = notes.Select(note => note.PositionInGrid).Distinct().ToArray();
                if (distinctGridLineIndices.Length == notes.Count) {
                    continue;
                }
                // There are sync notes in this bar.
                notes.Sort(Note.TimingComparison);
                for (var i = 0; i < bar.Notes.Count - 1; ++i) {
                    if (notes[i].PositionInGrid == notes[i + 1].PositionInGrid) {
                        Note.ConnectSync(notes[i], notes[i + 1]);
                        ++i;
                    }
                }
            }

            // Fix hold notes: if any line crosses other note(s). (Is it necessary? Deleste files seem to be organized well.)
            //foreach (var bar in score.Bars) {
            //    var notes = bar.Notes;
            //}
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
            var groupNumberString = commaStringValues[0];
            var measureIndexString = commaStringValues[1];
            var noteSpreading = colonStringValues[1];
            var finishPositions = colonStringValues.Length > 3 ? colonStringValues[3] : colonStringValues[2];
            // Abbreviated format & full format
            // #0,000:2222:1234
            // #0,001:2222:3333:3333
            var startPositions = colonStringValues[2];
            noteCache.Clear();

            var standardNoteCount = noteSpreading.Count(ch => ch != '0');
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
            entry.FullLength = noteSpreading.Length;
            int i = -1, j = -1;
            foreach (var ch in noteSpreading) {
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

        public static Encoding TryDetectDelesteBeatmapEncoding(string fileName) {
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
