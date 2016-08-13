namespace DereTore.Application.ScoreEditor.Model {
    public sealed class Note {

        public int Id { get; set; }
        public float Second { get; set; }
        public NoteType Type { get; set; }
        public NotePosition StartPosition { get; set; }
        public NotePosition FinishPosition { get; set; }
        public NoteStatus SwipeType { get; set; }
        public bool Sync { get; set; }
        public int GroupId { get; set; }

        public int NextHoldingIndex { get; set; } = -1;
        public int NextSwipeIndex { get; set; } = -1;
        public int PrevHoldingIndex { get; set; } = -1;
        public int PrevSwipeIndex { get; set; } = -1;
        public int SyncPairIndex { get; set; } = -1;

        public bool HasNextHolding => NextHoldingIndex >= 0;
        public bool HasNextSwipe => NextSwipeIndex >= 0;
        public bool HasPrevHolding => PrevHoldingIndex >= 0;
        public bool HasPrevSwipe => PrevSwipeIndex >= 0;
        public bool IsSwipe => Type == NoteType.TapOrSwipe && (SwipeType == NoteStatus.SwipeLeft || SwipeType == NoteStatus.SwipeRight);
        public bool IsTap => Type == NoteType.TapOrSwipe && SwipeType == NoteStatus.Tap;
        public bool IsHold => Type == NoteType.Hold;

        public bool Visible { get; set; }

    }
}
