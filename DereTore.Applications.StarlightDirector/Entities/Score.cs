using System.Linq;
using DereTore.Applications.StarlightDirector.Components;
using DereTore.Applications.StarlightDirector.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DereTore.Applications.StarlightDirector.Entities {
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class Score {

        public InternalList<Bar> Bars { get; }

        public ScoreSettings Settings { get; set; }

        [JsonIgnore]
        public Project Project { get; internal set; }

        [JsonIgnore]
        public Difficulty Difficulty { get; internal set; }

        public Bar AddBar() {
            return AddBar(Bars.Count);
        }

        public Bar AddBar(int index) {
            return AddBar(index, null);
        }

        public Bar AddBar(int index, BarParams barParams) {
            var bar = new Bar(this, index) {
                Params = barParams
            };
            if (index == Bars.Count) {
                Bars.Add(bar);
            } else {
                foreach (var b in Bars.Skip(index - 1)) {
                    ++b.Index;
                }
                Bars.Insert(index, bar);
            }
            return bar;
        }

        public Bar[] AddBars(int startIndex, int count) {
            return AddBars(startIndex, count, null);
        }

        public Bar[] AddBars(int startIndex, int count, BarParams barParams) {
            var bars = new Bar[count];
            for (var i = 0; i < count; ++i) {
                bars[i] = new Bar(this, startIndex + i) {
                    Params = barParams
                };
            }
            if (startIndex == Bars.Count - 1) {
                Bars.AddRange(bars);
            } else {
                foreach (var b in Bars.Skip(startIndex - 1)) {
                    ++b.Index;
                }
                Bars.InsertRange(startIndex, bars);
            }
            return bars;
        }

        [JsonConstructor]
        internal Score(Project project, Difficulty difficulty) {
            Bars = new InternalList<Bar>();
            Project = project;
            Difficulty = difficulty;
            Settings = ScoreSettings.CreateDefault();
            IDGenerators = new IDGenerators();
        }

        internal IDGenerators IDGenerators { get; }

        internal void ResolveReferences() {
            if (Bars == null) {
                return;
            }
            foreach (var bar in Bars) {
                if (bar.Notes != null) {
                    foreach (var note in bar.Notes) {
                        note.Bar = bar;
                    }
                }
                bar.Score = this;
            }
        }

        internal CompiledScore Compile() {
            var compiledScore = new CompiledScore();
            CompileTo(compiledScore);
            return compiledScore;
        }

        internal void CompileTo(CompiledScore compiledScore) {
            var settings = Settings;
            IDGenerators.ResetCompiled();
            var compiledNotes = compiledScore.Notes;
            compiledNotes.Clear();
            var endTimeOfLastBar = settings.StartTimeOffset;

            // We can also use Note.GetNoteHitTiming(), but that requires massive loops. Therefore we manually expand them here.
            foreach (var bar in Bars) {
                var bpm = bar.GetActualBpm();
                var signature = bar.GetActualSignature();
                var gridCountInBar = bar.GetActualGridPerSignature();
                var barStartTime = ComposerUtilities.BpmToSeconds(bpm) * signature;
                foreach (var note in bar.Notes) {
                    var compiledNote = new CompiledNote();
                    note.CompilationResult = compiledNote;
                    compiledNote.ID = IDGenerators.CompiledNoteIDGenerator.Next();
                    compiledNote.Type = note.Type;
                    compiledNote.StartPosition = note.StartPosition;
                    compiledNote.FinishPosition = note.FinishPosition;
                    compiledNote.FlickType = note.FlickType;
                    compiledNote.IsSync = note.IsSync;
                    compiledNote.HitTiming = endTimeOfLastBar + barStartTime * (note.PositionInGrid / (double)(signature * gridCountInBar));
                    var groupID = IDGenerators.FlickGroupIDGenerator.Current;
                    if (note.TryGetFlickGroupID(ref groupID)) {
                        compiledNote.FlickGroupID = groupID;
                        if (groupID == IDGenerators.FlickGroupIDGenerator.Current) {
                            IDGenerators.FlickGroupIDGenerator.Next();
                        }
                    }

                    compiledNotes.Add(compiledNote);
                }
                endTimeOfLastBar += barStartTime;
            }

            compiledNotes.Sort(CompiledNote.TimingComparison);
            var i = 1;
            foreach (var compiledNote in compiledNotes) {
                compiledNote.ID = i++;
            }
        }

    }
}
