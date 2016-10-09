using System;
using System.Windows;
using DereTore.Applications.StarlightDirector.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DereTore.Applications.StarlightDirector.Entities {
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), MemberSerialization = MemberSerialization.OptIn)]
    public sealed class Note : DependencyObject {

        [JsonProperty]
        public int ID { get; private set; }

        [JsonProperty]
        public int PositionInGrid { get; set; }

        public NoteType Type {
            get { return (NoteType)GetValue(TypeProperty); }
            private set { SetValue(TypeProperty, value); }
        }

        [JsonProperty]
        public NotePosition StartPosition {
            get { return (NotePosition)GetValue(StartPositionProperty); }
            set { SetValue(StartPositionProperty, value); }
        }

        [JsonProperty]
        public NotePosition FinishPosition {
            get { return (NotePosition)GetValue(FinishPositionProperty); }
            set { SetValue(FinishPositionProperty, value); }
        }

        public int PositionInTrack => (int)FinishPosition - 1;

        [JsonProperty]
        public NoteFlickType FlickType {
            get { return (NoteFlickType)GetValue(FlickTypeProperty); }
            set { SetValue(FlickTypeProperty, value); }
        }

        public bool IsSync {
            get { return (bool)GetValue(IsSyncProperty); }
            private set { SetValue(IsSyncProperty, value); }
        }

        public Bar Bar { get; internal set; }

        public bool IsFlick {
            get { return (bool)GetValue(IsFlickProperty); }
            private set { SetValue(IsFlickProperty, value); }
        }

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
                    if (!IsHoldStart) {
                        FlickType = NoteFlickType.Tap;
                    }
                } else {
                    if (n1 != null && n2 == null) {
                        // Currently there isn't an example of quick 'Z' turn appeared in CGSS (as shown below),
                        // so the following completion method is good enough.
                        //     -->
                        //      ^
                        //       \
                        //      -->
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
                    if (!IsHoldStart) {
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
                var origSyncTarget = _syncTarget;
                _syncTarget = value;
                if (origSyncTarget?.SyncTarget != null && origSyncTarget.SyncTarget.Equals(this)) {
                    origSyncTarget.SyncTarget = null;
                }
                IsSync = value != null;
                SyncTargetID = value?.ID ?? EntityID.Invalid;
            }
        }

        public bool IsHold {
            get { return (bool)GetValue(IsHoldProperty); }
            private set { SetValue(IsHoldProperty, value); }
        }

        public bool IsHoldStart => Type == NoteType.Hold && HoldTarget != null;

        public bool IsHoldEnd => Type == NoteType.TapOrFlick && HoldTarget != null;

        [JsonProperty]
        public int HoldTargetID { get; private set; }

        public Note HoldTarget {
            get {
                return _holdTarget;
            }
            set {
                var origHoldTarget = _holdTarget;
                _holdTarget = value;
                if (origHoldTarget?.HoldTarget != null && origHoldTarget.HoldTarget.Equals(this)) {
                    origHoldTarget.HoldTarget = null;
                }
                IsHold = value != null;
                // Only the former of the hold pair is considered as a hold note. The other is a tap or flick note.
                Type = (value != null && value > this) ? NoteType.Hold : NoteType.TapOrFlick;
                HoldTargetID = value?.ID ?? EntityID.Invalid;
            }
        }

        public bool IsGamingNote => Type == NoteType.TapOrFlick || Type == NoteType.Hold;

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(nameof(Type), typeof(NoteType), typeof(Note),
            new PropertyMetadata(NoteType.Invalid, OnTypeChanged));

        public static readonly DependencyProperty FlickTypeProperty = DependencyProperty.Register(nameof(FlickType), typeof(NoteFlickType), typeof(Note),
            new PropertyMetadata(NoteFlickType.Tap, OnFlickTypeChanged));

        public static readonly DependencyProperty IsSyncProperty = DependencyProperty.Register(nameof(IsSync), typeof(bool), typeof(Note),
            new PropertyMetadata(false));

        public static readonly DependencyProperty IsFlickProperty = DependencyProperty.Register(nameof(IsFlick), typeof(bool), typeof(Note),
            new PropertyMetadata(false));

        public static readonly DependencyProperty IsHoldProperty = DependencyProperty.Register(nameof(IsHold), typeof(bool), typeof(Note),
            new PropertyMetadata(false));

        public static readonly DependencyProperty StartPositionProperty = DependencyProperty.Register(nameof(StartPosition), typeof(NotePosition), typeof(Note),
            new PropertyMetadata(NotePosition.Nowhere));

        public static readonly DependencyProperty FinishPositionProperty = DependencyProperty.Register(nameof(FinishPosition), typeof(NotePosition), typeof(Note),
            new PropertyMetadata(NotePosition.Nowhere));

        public static readonly Comparison<Note> TimingComparison = (n1, n2) => {
            if (n1.Bar == n2.Bar) {
                return n1.PositionInGrid.CompareTo(n2.PositionInGrid);
            } else {
                return n1.GetHitTiming().CompareTo(n2.GetHitTiming());
            }
        };

        public static bool operator >(Note left, Note right) {
            return TimingComparison(left, right) > 0;
        }

        public static bool operator <(Note left, Note right) {
            return TimingComparison(left, right) < 0;
        }

        public static void ConnectSync(Note n1, Note n2) {
            if (n1 != null) {
                n1.SyncTarget = n2;
            }
            if (n2 != null) {
                n2.SyncTarget = n1;
            }
        }

        public static void ConnectFlick(Note first, Note second) {
            if (first != null) {
                first.NextFlickNote = second;
            }
            if (second != null) {
                second.PrevFlickNote = first;
            }
        }

        public static void ConnectHold(Note n1, Note n2) {
            if (n1 != null) {
                n1.HoldTarget = n2;
            }
            if (n2 != null) {
                n2.HoldTarget = n1;
            }
        }

        internal bool TryGetFlickGroupID(out FlickGroupModificationResult modificationResult, out int knownGroupID, out Note groupStart) {
            if (!IsFlick || (IsHoldEnd && !HasNextFlick)) {
                knownGroupID = EntityID.Invalid;
                modificationResult = FlickGroupModificationResult.Declined;
                groupStart = null;
                return false;
            }
            var groupItemCount = 0;
            var temp = this;
            // Compiler trick.
            groupStart = temp;
            while (temp != null) {
                groupStart = temp;
                temp = temp.PrevFlickNote;
                ++groupItemCount;
            }
            temp = this;
            ++groupItemCount;
            while (temp.HasNextFlick) {
                temp = temp.NextFlickNote;
                ++groupItemCount;
            }
            if (groupItemCount < 2) {
                // Actually, the flick group is not fully filled. We should throw an exception.
                knownGroupID = EntityID.Invalid;
                modificationResult = FlickGroupModificationResult.Declined;
                return false;
            }
            if (groupStart.GroupID != EntityID.Invalid) {
                knownGroupID = groupStart.GroupID;
                modificationResult = FlickGroupModificationResult.Reused;
            } else {
                knownGroupID = EntityID.Invalid;
                modificationResult = FlickGroupModificationResult.CreationPending;
            }
            return true;
        }

        internal int GroupID { get; set; }

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

        private static void OnTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var note = (Note)obj;
            note.IsFlick = note.IsFlickInternal();
        }

        private static void OnFlickTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var note = (Note)obj;
            note.IsFlick = note.IsFlickInternal();
        }

        private bool IsFlickInternal() {
            return Type == NoteType.TapOrFlick && (FlickType == NoteFlickType.FlickLeft || FlickType == NoteFlickType.FlickRight);
        }

        private Note _prevFlickNote;
        private Note _nextFlickNote;
        private Note _syncTarget;
        private Note _holdTarget;

    }
}
