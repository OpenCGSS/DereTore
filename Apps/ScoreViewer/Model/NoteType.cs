using System.ComponentModel;

namespace DereTore.Apps.ScoreViewer.Model {
    public enum NoteType {

        Invalid = 0,

        [Description("Tap or flick")]
        TapOrFlick = 1,
        [Description("Hold")]
        Hold = 2,
        [Description("Slide")]
        Slide = 3,

        Debug1 = 81,
        Debug2 = 82,
        Debug3 = 91,
        DebugSongEnd = 92,
        Debug5 = 100

    }
}
