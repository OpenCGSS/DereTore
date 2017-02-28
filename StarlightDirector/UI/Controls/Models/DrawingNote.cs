using StarlightDirector.Entities;

namespace StarlightDirector.UI.Controls.Models
{
    /// <summary>
    /// Internal representation of notes for drawing
    /// </summary>
    internal sealed class DrawingNote
    {
        public Note Note { get; set; }
        public DrawingNote HoldTarget { get; set; }
        public DrawingNote SyncTarget { get; set; }
        public DrawingNote GroupTarget { get; set; }
        public int Timing { get; set; }
        public bool Done { get; set; }
        public int Duration { get; set; }
        public bool IsHoldStart { get; set; }
        public double LastT { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public NoteDrawType DrawType { get; set; }
        public int HitPosition { get; set; }
        public bool EffectShown { get; set; }
    }
}
