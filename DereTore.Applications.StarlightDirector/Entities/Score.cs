using System;
using System.Collections.Generic;
using System.Linq;
using DereTore.Applications.StarlightDirector.Components;
using DereTore.Applications.StarlightDirector.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DereTore.Applications.StarlightDirector.Entities {
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class Score {

        public event EventHandler<EventArgs> GlobalSettingsChanged;

        public InternalList<Bar> Bars { get; }

        public ScoreSettings Settings { get; set; }

        [JsonIgnore]
        public Project Project { get; internal set; }

        [JsonIgnore]
        public Difficulty Difficulty { get; internal set; }

        public Bar AddBar() {
            return InsertBar(Bars.Count);
        }

        public Bar InsertBar(int indexBefore) {
            return InsertBar(indexBefore, null);
        }

        public Bar InsertBar(int indexBefore, BarParams barParams) {
            var bar = new Bar(this, indexBefore) {
                Params = barParams
            };
            if (indexBefore == Bars.Count) {
                Bars.Add(bar);
            } else {
                foreach (var b in Bars.Skip(indexBefore)) {
                    ++b.Index;
                }
                Bars.Insert(indexBefore, bar);
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

        public bool RemoveBarAt(int index) {
            if (index < 0 || index >= Bars.Count) {
                return false;
            }
            Bars.RemoveAt(index);
            for (var i = index; i < Bars.Count; ++i) {
                --Bars[i].Index;
            }
            return true;
        }

        [JsonConstructor]
        internal Score(Project project, Difficulty difficulty) {
            Bars = new InternalList<Bar>();
            Project = project;
            Difficulty = difficulty;
            Settings = ScoreSettings.CreateDefault();
            IDGenerators = new IDGenerators();
            Settings.SettingChanged += OnGlobalSettingsChanged;
        }

        ~Score() {
            Settings.SettingChanged -= OnGlobalSettingsChanged;
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
            var allNotes = Bars.SelectMany(bar => bar.Notes).ToArray();
            foreach (var note in allNotes) {
                if (note.SyncTargetID != EntityID.Invalid) {
                    note.SyncTarget = FindNoteByID(note.SyncTargetID);
                }
                if (note.NextFlickNoteID != EntityID.Invalid) {
                    note.NextFlickNote = FindNoteByID(note.NextFlickNoteID);
                }
                if (note.PrevFlickNoteID != EntityID.Invalid) {
                    note.PrevFlickNote = FindNoteByID(note.PrevFlickNoteID);
                }
                if (note.HoldTargetID != EntityID.Invalid) {
                    note.HoldTarget = FindNoteByID(note.HoldTargetID);
                }
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
            IDGenerators.ResetOriginal();
            var compiledNotes = compiledScore.Notes;
            compiledNotes.Clear();
            var endTimeOfLastBar = settings.StartTimeOffset;

            // Clear the GroupID caches.
            foreach (var bar in Bars) {
                foreach (var note in bar.Notes) {
                    note.GroupID = EntityID.Invalid;
                    note.CompilationResult = null;
                }
            }

            // We can also use Note.GetNoteHitTiming(), but that requires massive loops. Therefore we manually expand them here.
            foreach (var bar in Bars) {
                var bpm = bar.GetActualBpm();
                var signature = bar.GetActualSignature();
                var gridCountInBar = bar.GetActualGridPerSignature();
                var barStartTime = ComposerUtilities.BpmToSeconds(bpm) * signature;
                // Sorting is for flick group generation. We have to assure that the start of each group is
                // processed first, at least in the group which it is in.
                bar.Notes.Sort(Note.TimeComparison);
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
                    if (note.GroupID != EntityID.Invalid) {
                        compiledNote.FlickGroupID = note.GroupID;
                    } else {
                        int groupID;
                        FlickGroupModificationResult result;
                        Note groupStart;
                        if (note.TryGetFlickGroupID(out result, out groupID, out groupStart)) {
                            switch (result) {
                                case FlickGroupModificationResult.Reused:
                                    note.GroupID = compiledNote.FlickGroupID = groupID;
                                    break;
                                case FlickGroupModificationResult.CreationPending:
                                    groupID = IDGenerators.FlickGroupIDGenerator.Next();
                                    groupStart.GroupID = note.GroupID = compiledNote.FlickGroupID = groupID;
                                    break;
                                default:
                                    break;
                            }
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

        private Note FindNoteByID(int noteID) {
            foreach (var bar in Bars) {
                foreach (var note in bar.Notes) {
                    if (note.ID == noteID) {
                        return note;
                    }
                }
            }
            return null;
        }

        private void OnGlobalSettingsChanged(object sender, EventArgs e) {
            GlobalSettingsChanged.Raise(sender, e);
        }

    }
}
