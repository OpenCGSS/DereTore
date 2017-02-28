using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using DereTore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace StarlightDirector.Entities {
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), MemberSerialization = MemberSerialization.OptIn)]
    public sealed class Note : DependencyObject, IComparable<Note> {

        public event EventHandler<EventArgs> ExtraParamsChanged;

        [JsonProperty]
        public int ID { get; private set; }

        [JsonIgnore]
        public double HitTiming => Bar.TimeAtGrid(IndexInGrid);

        private int _indexInGrid;

        // "PositionInGrid" was the first name of this property used in serialization.
        [JsonProperty("positionInGrid")]
        public int IndexInGrid {
            get { return _indexInGrid; }
            set {
                _indexInGrid = value;
                Bar?.SortNotes();
            }
        }

        public NoteType Type {
            get { return (NoteType)GetValue(TypeProperty); }
            internal set { SetValue(TypeProperty, value); }
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

        public int IndexInTrack => (int)FinishPosition - 1;

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

        [JsonProperty("prevFlickNoteID")]
        public int PrevFlickOrSlideNoteID { get; internal set; }

        public Note PrevFlickOrSlideNote {
            get { return _prevFlickOrSlideNote; }
            set {
                UpdateFlickTypeStep1(value);
                _prevFlickOrSlideNote = value;
                UpdateFlickTypeStep2();
                PrevFlickOrSlideNoteID = value?.ID ?? EntityID.Invalid;
            }
        }

        public bool HasPrevFlickOrSlide => (Type == NoteType.TapOrFlick || Type == NoteType.Slide) && PrevFlickOrSlideNote != null;

        [JsonProperty("nextFlickNoteID")]
        public int NextFlickOrSlideNoteID { get; internal set; }

        public Note NextFlickOrSlideNote {
            get { return _nextFlickOrSlideNote; }
            set {
                UpdateFlickTypeStep1(value);
                _nextFlickOrSlideNote = value;
                UpdateFlickTypeStep2();
                NextFlickOrSlideNoteID = value?.ID ?? EntityID.Invalid;
            }
        }

        public bool HasNextFlickOrSlide => (Type == NoteType.TapOrFlick || Type == NoteType.Slide) && NextFlickOrSlideNote != null;

        [Obsolete("This property is provided for forward compatibility only.")]
        [JsonProperty]
        public int SyncTargetID {
            get {
                // For legacy versions that generate sync connection from this field
                // Connect the first note and the last note of sync groups
                if (HasPrevSync) {
                    if (HasNextSync) {
                        return EntityID.Invalid;
                    }
                    var final = PrevSyncTarget;
                    while (final.HasPrevSync) {
                        final = final.PrevSyncTarget;
                    }
                    return final.ID;
                } else {
                    if (!HasNextSync) {
                        return EntityID.Invalid;
                    }
                    var final = NextSyncTarget;
                    while (final.HasNextSync) {
                        final = final.NextSyncTarget;
                    }
                    return final.ID;
                }
            }
        }

        public Note PrevSyncTarget {
            get { return _prevSyncTarget; }
            internal set { SetPrevSyncTargetInternal(value); }
        }

        public Note NextSyncTarget {
            get { return _nextSyncTarget; }
            internal set { SetNextSyncTargetInternal(value); }
        }

        public bool HasPrevSync => PrevSyncTarget != null;

        public bool HasNextSync => NextSyncTarget != null;

        public bool IsHold {
            get { return (bool)GetValue(IsHoldProperty); }
            private set { SetValue(IsHoldProperty, value); }
        }

        public bool IsHoldStart => Type == NoteType.Hold && HoldTarget != null;

        public bool IsHoldEnd => Type == NoteType.TapOrFlick && HoldTarget != null;

        [JsonProperty]
        public int HoldTargetID { get; internal set; }

        public Note HoldTarget {
            get { return _holdTarget; }
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

        public bool IsSlide {
            get { return (bool)GetValue(IsSlideProperty); }
            private set { SetValue(IsSlideProperty, value); }
        }

        public bool IsSlideStart => Type == NoteType.Slide && !HasPrevFlickOrSlide && HasNextFlickOrSlide;

        public bool IsSlideContinuation => Type == NoteType.Slide && HasPrevFlickOrSlide && HasNextFlickOrSlide;

        public bool IsSlideEnd => Type == NoteType.Slide && !HasNextFlickOrSlide && HasPrevFlickOrSlide;

        public bool IsTap {
            get { return (bool)GetValue(IsTapProperty); }
            private set { SetValue(IsTapProperty, value); }
        }

        public bool IsGamingNote => IsTypeGaming(Type);

        public bool IsSpecialNote => IsTypeSpecial(Type);

        public NoteExtraParams ExtraParams {
            get { return (NoteExtraParams)GetValue(ExtraParamsProperty); }
            set { SetValue(ExtraParamsProperty, value); }
        }

        public bool ShouldBeRenderedAsFlick {
            get { return (bool)GetValue(ShouldBeRenderedAsFlickProperty); }
            private set { SetValue(ShouldBeRenderedAsFlickProperty, value); }
        }

        public bool ShouldBeRenderedAsSlide {
            get { return (bool)GetValue(ShouldBeRenderedAsSlideProperty); }
            private set { SetValue(ShouldBeRenderedAsSlideProperty, value); }
        }

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

        public static readonly DependencyProperty IsSlideProperty = DependencyProperty.Register(nameof(IsSlide), typeof(bool), typeof(Note),
            new PropertyMetadata(false));

        public static readonly DependencyProperty IsTapProperty = DependencyProperty.Register(nameof(IsTap), typeof(bool), typeof(Note),
            new PropertyMetadata(true));

        public static readonly DependencyProperty StartPositionProperty = DependencyProperty.Register(nameof(StartPosition), typeof(NotePosition), typeof(Note),
            new PropertyMetadata(NotePosition.Nowhere));

        public static readonly DependencyProperty FinishPositionProperty = DependencyProperty.Register(nameof(FinishPosition), typeof(NotePosition), typeof(Note),
            new PropertyMetadata(NotePosition.Nowhere));

        public static readonly DependencyProperty ExtraParamsProperty = DependencyProperty.Register(nameof(ExtraParams), typeof(NoteExtraParams), typeof(Note),
            new PropertyMetadata(null, OnExtraParamsChanged));

        public static readonly DependencyProperty ShouldBeRenderedAsFlickProperty = DependencyProperty.Register(nameof(ShouldBeRenderedAsFlick), typeof(bool), typeof(Note),
            new PropertyMetadata(false));

        public static readonly DependencyProperty ShouldBeRenderedAsSlideProperty = DependencyProperty.Register(nameof(ShouldBeRenderedAsSlide), typeof(bool), typeof(Note),
            new PropertyMetadata(false));

        public static readonly Comparison<Note> TimingThenPositionComparison = (x, y) => {
            var r = TimingComparison(x, y);
            return r == 0 ? TrackPositionComparison(x, y) : r;
        };

        public static readonly Comparison<Note> TimingComparison = (x, y) => {
            if (x == null) {
                throw new ArgumentNullException(nameof(x));
            }
            if (y == null) {
                throw new ArgumentNullException(nameof(y));
            }
            if (x.Equals(y)) {
                return 0;
            }
            if (x.Bar != y.Bar) {
                return x.Bar.Index.CompareTo(y.Bar.Index);
            }
            var r = x.IndexInGrid.CompareTo(y.IndexInGrid);
            if (r == 0 && x.Type != y.Type && (x.Type == NoteType.VariantBpm || y.Type == NoteType.VariantBpm)) {
                // The Variant BPM note is always placed at the end on the same grid line.
                return x.Type == NoteType.VariantBpm ? 1 : -1;
            } else {
                return r;
            }
        };

        public int CompareTo(Note other) {
            if (other == null) {
                throw new ArgumentNullException(nameof(other));
            }
            return Equals(other) ? 0 : TimingComparison(this, other);
        }

        public static readonly Comparison<Note> TrackPositionComparison = (n1, n2) => ((int)n1.FinishPosition).CompareTo((int)n2.FinishPosition);

        public static bool operator >(Note left, Note right) {
            return TimingComparison(left, right) > 0;
        }

        public static bool operator <(Note left, Note right) {
            return TimingComparison(left, right) < 0;
        }

        public static bool IsTypeGaming(NoteType type) {
            return type == NoteType.TapOrFlick || type == NoteType.Hold || type == NoteType.Slide;
        }

        public static bool IsTypeSpecial(NoteType type) {
            return type == NoteType.VariantBpm;
        }

        public static void ConnectSync(Note first, Note second) {
            /*
             * Before:
             *     ... <==>    first    <==> first_next   <==> ...
             *     ... <==> second_prev <==>   second     <==> ...
             *
             * After:
             *                               first_next   <==> ...
             *     ... <==>    first    <==>   second     <==> ...
             *     ... <==> second_prev
             */
            if (first == second) {
                throw new ArgumentException("A note should not be connected to itself", nameof(second));
            } else if (first?.NextSyncTarget == second && second?.PrevSyncTarget == first) {
                return;
            }
            first?.NextSyncTarget?.SetPrevSyncTargetInternal(null);
            second?.PrevSyncTarget?.SetNextSyncTargetInternal(null);
            first?.SetNextSyncTargetInternal(second);
            second?.SetPrevSyncTargetInternal(first);
        }

        public void RemoveSync() {
            /*
             * Before:
             *     ... <==> prev <==> this <==> next <==> ...
             *
             * After:
             *     ... <==> prev <============> next <==> ...
             *                        this
             */
            PrevSyncTarget?.SetNextSyncTargetInternal(NextSyncTarget);
            NextSyncTarget?.SetPrevSyncTargetInternal(PrevSyncTarget);
            SetPrevSyncTargetInternal(null);
            SetNextSyncTargetInternal(null);
        }

        public void FixSync() {
            if (!IsGamingNote) {
                return;
            }
            RemoveSync();
            Note prev = null;
            Note next = null;
            foreach (var note in Bar.Notes) {
                if (note == this) {
                    continue;
                }
                if (!note.IsGamingNote) {
                    continue;
                }
                if (note.IndexInGrid == IndexInGrid) {
                    if (note.IndexInTrack < IndexInTrack) {
                        if (prev == null || prev.IndexInTrack < note.IndexInTrack) {
                            prev = note;
                        }
                    } else {
                        if (next == null || note.IndexInTrack < next.IndexInTrack) {
                            next = note;
                        }
                    }
                }
            }
            ConnectSync(prev, this);
            ConnectSync(this, next);
        }

        public static void ConnectFlick(Note first, Note second) {
            if (first != null) {
                first.NextFlickOrSlideNote = second;
            }
            if (second != null) {
                second.PrevFlickOrSlideNote = first;
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

        internal int GroupID { get; set; }

        [JsonConstructor]
        internal Note(int id, Bar bar) {
            ID = id;
            Bar = bar;
            IndexInGrid = 0;
            Type = NoteType.TapOrFlick;
            StartPosition = NotePosition.Nowhere;
            FinishPosition = NotePosition.Nowhere;
            FlickType = NoteFlickType.Tap;
        }

        internal void Reset() {
            if (NextFlickOrSlideNote != null) {
                NextFlickOrSlideNote.PrevFlickOrSlideNote = null;
            }
            NextFlickOrSlideNote = null;
            if (PrevFlickOrSlideNote != null) {
                PrevFlickOrSlideNote.NextFlickOrSlideNote = null;
            }
            PrevFlickOrSlideNote = null;
            if (HoldTarget != null) {
                HoldTarget.HoldTarget = null;
                if (HoldTarget != null) {
                    if (!HoldTarget.HasNextFlickOrSlide && !HoldTarget.HasPrevFlickOrSlide && HoldTarget.FlickType != NoteFlickType.Tap) {
                        HoldTarget.FlickType = NoteFlickType.Tap;
                    }
                }
            }
            FlickType = NoteFlickType.Tap;
            HoldTarget = null;
        }

        internal void SetSpecialType(NoteType type) {
            switch (type) {
                case NoteType.VariantBpm:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
            Type = type;
        }

        // Why is ugly functions like this even exist?
        internal void SetIndexInGridWithoutSorting(int newIndex) {
            _indexInGrid = newIndex;
        }

        private static void OnTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var note = (Note)obj;
            note.IsFlick = note.IsFlickInternal();
            note.IsTap = note.IsTapInternal();
            note.IsSlide = note.IsSlideInternal();
            note.UpdateFlickTypeStep2();
        }

        private static void OnFlickTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var note = (Note)obj;
            note.IsFlick = note.IsFlickInternal();
            note.IsTap = note.IsTapInternal();
            note.IsSlide = note.IsSlideInternal();
        }

        private static void OnExtraParamsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var note = (Note)obj;
            var oldParams = (NoteExtraParams)e.OldValue;
            var newParams = (NoteExtraParams)e.NewValue;
            if (oldParams != null) {
                oldParams.ParamsChanged -= note.ExtraParams_ParamsChanged;
            }
            if (newParams != null) {
                newParams.ParamsChanged += note.ExtraParams_ParamsChanged;
            }
        }

        private void UpdateFlickTypeStep1(Note value) {
            var n1 = PrevFlickOrSlideNote;
            var n2 = NextFlickOrSlideNote;
            if (IsSlide) {
                FlickType = NoteFlickType.Tap;
            } else {
                if (n1 == null && n2 == null && value != null) {
                    FlickType = FinishPosition >= NotePosition.Center ? NoteFlickType.FlickRight : NoteFlickType.FlickLeft;
                }
            }
        }

        private void UpdateFlickTypeStep2() {
            var n1 = PrevFlickOrSlideNote;
            var n2 = NextFlickOrSlideNote;
            if (n1 == null && n2 == null) {
                if (!IsHoldStart) {
                    FlickType = NoteFlickType.Tap;
                }
            } else {
                // Currently there isn't an example of quick 'Z' turn appeared in CGSS (as shown below),
                // so the following completion method is good enough.
                //     -->
                //      ^
                //       \
                //      -->
                //Debug.Print(HitTiming.ToString(CultureInfo.InvariantCulture));
                if (IsSlide && (n2 == null || !n2.IsFlick)) {
                    FlickType = NoteFlickType.Tap;
                } else {
                    if (n2 != null) {
                        FlickType = n2.FinishPosition > FinishPosition ? NoteFlickType.FlickRight : NoteFlickType.FlickLeft;
                    } else {
                        FlickType = n1.FinishPosition > FinishPosition ? NoteFlickType.FlickLeft : NoteFlickType.FlickRight;
                    }
                }
            }

            if (IsFlick || IsSlide) {
                if (IsSlide) {
                    ShouldBeRenderedAsSlide = HasNextFlickOrSlide && !NextFlickOrSlideNote.IsFlick;
                } else {
                    ShouldBeRenderedAsSlide = false;
                }
                ShouldBeRenderedAsFlick = !ShouldBeRenderedAsSlide;
            } else {
                ShouldBeRenderedAsFlick = ShouldBeRenderedAsSlide = false;
            }
        }

        private bool IsFlickInternal() => Type == NoteType.TapOrFlick && (FlickType == NoteFlickType.FlickLeft || FlickType == NoteFlickType.FlickRight);

        private bool IsTapInternal() => Type == NoteType.TapOrFlick && FlickType == NoteFlickType.Tap;

        private bool IsSlideInternal() => Type == NoteType.Slide;

        private void ExtraParams_ParamsChanged(object sender, EventArgs e) {
            // if we a BPM note is changed, inform the Bar to update its timings
            Bar?.UpdateTimingsChain();
            ExtraParamsChanged.Raise(sender, e);
        }

        private void SetPrevSyncTargetInternal(Note prev) {
            _prevSyncTarget = prev;
            IsSync = _prevSyncTarget != null || _nextSyncTarget != null;
        }

        private void SetNextSyncTargetInternal(Note next) {
            _nextSyncTarget = next;
            IsSync = _prevSyncTarget != null || _nextSyncTarget != null;
        }

        private Note _prevFlickOrSlideNote;
        private Note _nextFlickOrSlideNote;
        private Note _prevSyncTarget;
        private Note _nextSyncTarget;
        private Note _holdTarget;

    }
}
