namespace DereTore.Application.ScoreEditor.Model {
    public sealed class Note {

        public int Id { get; set; }
        public float Second { get; set; }
        public NoteType Type { get; set; }
        public NotePosition StartPosition { get; set; }
        public NotePosition FinishPosition { get; set; }
        public NoteStatus FlickType { get; set; }
        public bool Sync { get; set; }
        public int GroupId { get; set; }

        public int NextHoldingIndex { get; set; } = -1;
        public int NextFlickIndex { get; set; } = -1;
        public int PrevHoldingIndex { get; set; } = -1;
        public int PrevFlickIndex { get; set; } = -1;
        public int SyncPairIndex { get; set; } = -1;

        public bool HasNextHolding => NextHoldingIndex >= 0;
        public bool HasNextFlick => NextFlickIndex >= 0;
        public bool HasPrevHolding => PrevHoldingIndex >= 0;
        public bool HasPrevFlick => PrevFlickIndex >= 0;
        public bool IsFlick => Type == NoteType.TapOrFlick && (FlickType == NoteStatus.FlickLeft || FlickType == NoteStatus.FlickRight);
        public bool IsTap => Type == NoteType.TapOrFlick && FlickType == NoteStatus.Tap;
        public bool IsHold => Type == NoteType.Hold;
        public bool IsGamingNote => Type == NoteType.TapOrFlick || Type == NoteType.Hold;

        // Properties for editor control.
        public bool Visible { get; set; }
        public bool Selected { get; set; }

    }
}
