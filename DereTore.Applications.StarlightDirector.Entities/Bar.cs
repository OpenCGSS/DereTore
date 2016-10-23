using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DereTore.Applications.StarlightDirector.Entities {
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), MemberSerialization = MemberSerialization.OptIn)]
    public sealed class Bar {

        public Note AddNote() {
            var id = MathHelper.NextRandomPositiveInt32();
            while (Score.ExistingIDs.Contains(id)) {
                id = MathHelper.NextRandomPositiveInt32();
            }
            return AddNote(id);
        }

        public bool RemoveNote(Note note) {
            if (!Notes.Contains(note)) {
                return false;
            }
            Notes.Remove(note);
            Score.Notes.Remove(note);
            Score.ExistingIDs.Remove(note.ID);
            return true;
        }

        [JsonProperty]
        public InternalList<Note> Notes { get; }

        [JsonProperty]
        public BarParams Params { get; internal set; }

        [JsonProperty]
        public int Index { get; internal set; }

        public Score Score { get; internal set; }

        [JsonConstructor]
        internal Bar(Score score, int index) {
            Score = score;
            Notes = new InternalList<Note>();
            Index = index;
        }

        internal Note AddNote(int id) {
            if (Score.ExistingIDs.Contains(id)) {
                return null;
            }
            var note = new Note(id, this);
            Notes.Add(note);
            Score.Notes.Add(note);
            Score.ExistingIDs.Add(id);
            return note;
        }

        internal Note AddNoteWithoutUpdatingGlobalNotes(int id) {
            if (Score.ExistingIDs.Contains(id)) {
                return null;
            }
            var note = new Note(id, this);
            Notes.Add(note);
            Score.ExistingIDs.Add(id);
            return note;
        }

        internal void SquashParams() {
            if (Params?.CanBeSquashed ?? false) {
                Params = null;
            }
        }

    }
}
