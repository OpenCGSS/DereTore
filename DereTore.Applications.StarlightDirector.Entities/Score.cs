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
                bar.UpdateTimings();

            } else {
                Bars.Insert(indexBefore, bar);
                bar.UpdateTimings();

                foreach (var b in Bars.Skip(indexBefore + 1)) {
                    ++b.Index;
                    b.UpdateTimings();
                }
            }
            return bar;
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
                bars[i].UpdateTimings();
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

        internal void FixSyncNotes() {
            foreach (var bar in Bars) {
                var gridIndexGroups =
                    from n in bar.Notes
                    where n.IsGamingNote
                    group n by n.IndexInGrid into g
                    select g;
                foreach (var group in gridIndexGroups) {
                    var sortedNotesInGroup =
                        from n in @group
                        orderby n.IndexInTrack
                        select n;
                    Note prev = null;
                    foreach (var note in sortedNotesInGroup) {
                        Note.ConnectSync(prev, note);
                        prev = note;
                    }
                    Note.ConnectSync(prev, null);
                }
            }
        }

        private Note FindNoteByID(int noteID) {
            return Bars.SelectMany(bar => bar.Notes).FirstOrDefault(note => note.ID == noteID);
        }

    }
}
