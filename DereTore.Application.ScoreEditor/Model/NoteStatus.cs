using System.ComponentModel;

namespace DereTore.Application.ScoreEditor.Model {
    public enum NoteStatus {

        [Description("Tap")]
        Tap = 0,
        [Description("Flick left")]
        FlickLeft = 1,
        [Description("Flick right")]
        FlickRight = 2

    }
}
