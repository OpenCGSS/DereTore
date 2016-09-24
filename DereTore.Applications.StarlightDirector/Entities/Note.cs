using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DereTore.Applications.StarlightDirector.Entities {
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), MemberSerialization = MemberSerialization.OptIn)]
    public sealed class Note : DependencyObject {

        [JsonProperty]
        public int ID { get; private set; }

        [JsonProperty]
        public int PositionInGrid { get; set; }

        public NoteType Type { get; private set; }

        [JsonProperty]
        public NotePosition StartPosition { get; set; }

        [JsonProperty]
        public NotePosition FinishPosition { get; set; }

        [JsonProperty]
        public NoteFlickType FlickType {
            get { return (NoteFlickType)GetValue(FlickTypeProperty); }
            set { SetValue(FlickTypeProperty, value); }
        }

        public bool IsSync { get; private set; }

        public Bar Bar { get; internal set; }

        public CompiledNote CompilationResult { get; internal set; }

        public bool IsFlick => Type == NoteType.TapOrFlick && (FlickType == NoteFlickType.FlickLeft || FlickType == NoteFlickType.FlickRight);

        [JsonProperty]
        public int PrevFlickNoteID { get; private set; }

        public Note PrevFlickNote {
            get {
                return _prevFlickNote;
            }
            set {
                Note n1 = PrevFlickNote, n2 = NextFlickNote;
                if (n1 == null && n2 == null && value != null) {
                    FlickType = FinishPosition >= NotePosition.Center ? NoteFlickType.FlickRight : NoteFlickType.FlickLeft;
                }
                _prevFlickNote = value;
                n1 = PrevFlickNote;
                n2 = NextFlickNote;
                if (n1 == null && n2 == null) {
                    if (!IsHold) {
                        FlickType = NoteFlickType.Tap;
                    }
                } else {
                    if (n1 != null) {
                        FlickType = n1.FlickType;
                    }
                }
                PrevFlickNoteID = value?.ID ?? EntityID.Invalid;
            }
        }

        public bool HasPrevFlick => Type == NoteType.TapOrFlick && PrevFlickNote != null;

        [JsonProperty]
        public int NextFlickNoteID { get; private set; }

        public Note NextFlickNote {
            get {
                return _nextFlickNote;
            }
            set {
                Note n1 = PrevFlickNote, n2 = NextFlickNote;
                if (n1 == null && n2 == null && value != null) {
                    FlickType = FinishPosition >= NotePosition.Center ? NoteFlickType.FlickRight : NoteFlickType.FlickLeft;
                }
                _nextFlickNote = value;
                n1 = PrevFlickNote;
                n2 = NextFlickNote;
                if (n1 == null && n2 == null) {
                    if (!IsHold) {
                        FlickType = NoteFlickType.Tap;
                    }
                } else {
                    if (n2 != null) {
                        FlickType = n2.FinishPosition > FinishPosition ? NoteFlickType.FlickRight : NoteFlickType.FlickLeft;
                    }
                }
                NextFlickNoteID = value?.ID ?? EntityID.Invalid;
            }
        }

        public bool HasNextFlick => Type == NoteType.TapOrFlick && NextFlickNote != null;

        [JsonProperty]
        public int SyncTargetID { get; private set; }

        public Note SyncTarget {
            get {
                return _syncTarget;
            }
            set {
                _syncTarget = value;
                IsSync = value != null;
                SyncTargetID = value?.ID ?? EntityID.Invalid;
            }
        }

        public bool IsHold => Type == NoteType.Hold && HoldTarget != null;

        [JsonProperty]
        public int HoldTargetID { get; private set; }

        public Note HoldTarget {
            get {
                return _holdTarget;
            }
            set {
                _holdTarget = value;
                Type = value != null ? NoteType.Hold : NoteType.TapOrFlick;
                HoldTargetID = value?.ID ?? EntityID.Invalid;
            }
        }

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

        public static readonly DependencyProperty FlickTypeProperty = DependencyProperty.Register(nameof(FlickType), typeof(NoteFlickType), typeof(Note),
            new PropertyMetadata(NoteFlickType.Tap));

        [JsonConstructor]
        internal Note(int id, Bar bar) {
            ID = id;
            Bar = bar;
            PositionInGrid = 0;
            Type = NoteType.TapOrFlick;
            StartPosition = NotePosition.Nowhere;
            FinishPosition = NotePosition.Nowhere;
            FlickType = NoteFlickType.Tap;
        }

        internal void Reset() {
            if (SyncTarget != null) {
                SyncTarget.SyncTarget = null;
            }
            SyncTarget = null;
            if (NextFlickNote != null) {
                NextFlickNote.PrevFlickNote = null;
            }
            NextFlickNote = null;
            if (PrevFlickNote != null) {
                PrevFlickNote.NextFlickNote = null;
            }
            PrevFlickNote = null;
            if (HoldTarget != null) {
                HoldTarget.HoldTarget = null;
                if (!HoldTarget.HasNextFlick && !HoldTarget.HasPrevFlick && HoldTarget.FlickType != NoteFlickType.Tap) {
                    HoldTarget.FlickType = NoteFlickType.Tap;
                }
            }
            FlickType = NoteFlickType.Tap;
            HoldTarget = null;
        }

        private Note _prevFlickNote;
        private Note _nextFlickNote;
        private Note _syncTarget;
        private Note _holdTarget;

    }
}
