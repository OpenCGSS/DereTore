using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DereTore.Applications.StarlightDirector.Entities {
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), MemberSerialization = MemberSerialization.OptIn)]
    public sealed class Score {

        [JsonProperty]
        public InternalList<Bar> Bars { get; }

        public Project Project { get; internal set; }

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
            return RemoveBar(Bars[index]);
        }

        public bool RemoveBar(Bar bar) {
            var bars = Bars;
            if (!bars.Contains(bar)) {
                return false;
            }
            var index = bars.IndexOf(bar);
            foreach (var note in bar.Notes) {
                Notes.Remove(note);
            }
            bars.Remove(bar);
            for (var i = index; i < bars.Count; ++i) {
                --bars[i].Index;
            }
            return true;
        }

        public bool HasAnyNote => Notes.Count > 0;

        public InternalList<Note> Notes { get; }

        public bool Validate(out string[] problems) {
            // Rules:
            // 1. [error] Hold lines do not cross on other notes;
            // 2. [warning] Two notes on the same grid line have no sync relation;
            // 3. [warning] Flick group contains only one flick note.
            throw new NotImplementedException();
        }

        public bool Validate() {
            string[] dummy;
            return Validate(out dummy);
        }

        [JsonConstructor]
        internal Score(Project project, Difficulty difficulty) {
            Bars = new InternalList<Bar>();
            Project = project;
            Difficulty = difficulty;
            Notes = new InternalList<Note>();
        }

        internal void ResolveReferences(Project project) {
            if (Bars == null) {
                return;
            }
            Project = project;
            foreach (var bar in Bars) {
                foreach (var note in bar.Notes) {
                    note.Bar = bar;
                }
                bar.Score = this;
            }
            var allNotes = Bars.SelectMany(bar => bar.Notes).ToArray();
            Notes.AddRange(allNotes);
            foreach (var note in allNotes) {
                if (!note.IsGamingNote) {
                    continue;
                }
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

        private Note FindNoteByID(int noteID) {
            return Bars.SelectMany(bar => bar.Notes).FirstOrDefault(note => note.ID == noteID);
        }

    }
}
