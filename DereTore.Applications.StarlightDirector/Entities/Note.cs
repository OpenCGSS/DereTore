using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DereTore.Applications.StarlightDirector.Entities {
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class Note {

        public int ID { get; private set; }

        public int PositionInGrid { get; set; }

        public NoteType Type { get; set; }

        public NotePosition StartPosition { get; set; }

        public NotePosition FinishPosition { get; set; }

        public NoteFlickType FlickType { get; set; }

        public bool IsSync { get; set; }

        [JsonIgnore]
        public Bar Bar { get; internal set; }

        [JsonIgnore]
        public CompiledNote CompilationResult { get; internal set; }

        [JsonIgnore]
        public bool IsFlick => Type == NoteType.TapOrFlick && (FlickType == NoteFlickType.FlickLeft || FlickType == NoteFlickType.FlickRight);

        public int PrevFlickNoteID { get; private set; }

        [JsonIgnore]
        public Note PrevFlickNote {
            get {
                return _prevFlickNote;
            }
            set {
                _prevFlickNote = value;
                PrevFlickNoteID = value?.ID ?? EntityID.Invalid;
            }
        }

        [JsonIgnore]
        public bool HasPrevFlick => Type == NoteType.TapOrFlick && PrevFlickNote != null;

        public int NextFlickNoteID { get; private set; }

        [JsonIgnore]
        public Note NextFlickNote {
            get {
                return _nextFlickNote;
            }
            set {
                _nextFlickNote = value;
                NextFlickNoteID = value?.ID ?? EntityID.Invalid;
            }
        }

        [JsonIgnore]
        public bool HasNextFlick => Type == NoteType.TapOrFlick && NextFlickNote != null;

        public int SyncTargetID { get; private set; }

        [JsonIgnore]
        public Note SyncTarget {
            get {
                return _syncTarget;
            }
            set {
                _syncTarget = value;
                SyncTargetID = value?.ID ?? EntityID.Invalid;
            }
        }

        [JsonIgnore]
        public bool IsGamingNote => Type == NoteType.TapOrFlick || Type == NoteType.Hold;

        public bool TryGetFlickGroupID(ref int suggestedID) {
            if (!IsFlick) {
                return false;
            }
            var groupItemCount = 0;
            var temp = this;
            var groupStart = this;
            while (temp.HasPrevFlick) {
                groupStart = temp;
                temp = temp.PrevFlickNote;
                ++groupItemCount;
            }
            temp = this;
            while (temp.HasNextFlick) {
                temp = temp.NextFlickNote;
                ++groupItemCount;
            }
            if (groupItemCount < 2) {
                // Actually, the flick group is not fully filled.
                return false;
            }
            if (groupStart.ID != EntityID.Invalid) {
                suggestedID = groupStart.ID;
            }
            return true;
        }

        [JsonConstructor]
        internal Note(int id, Bar bar) {
            ID = id;
            Bar = bar;
            PositionInGrid = 0;
            Type = NoteType.Invalid;
            StartPosition = NotePosition.Nowhere;
            FinishPosition = NotePosition.Nowhere;
            FlickType = NoteFlickType.Tap;
            IsSync = false;
        }

        private Note _prevFlickNote;
        private Note _nextFlickNote;
        private Note _syncTarget;

    }
}
