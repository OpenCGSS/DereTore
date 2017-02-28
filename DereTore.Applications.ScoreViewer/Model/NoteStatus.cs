using System.ComponentModel;

namespace DereTore.Applications.ScoreViewer.Model {
    public enum NoteStatus {

        [Description("Tap")]
        Tap = 0,
        [Description("Flick left")]
        FlickLeft = 1,
        [Description("Flick right")]
        FlickRight = 2

    }
}
