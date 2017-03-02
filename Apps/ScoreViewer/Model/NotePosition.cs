using System.ComponentModel;

namespace DereTore.Apps.ScoreViewer.Model {
    public enum NotePosition {

        [Description("Invalid value for gaming note position")]
        Nowhere = 0,
        [Description("[1] Left")]
        P1 = 1,
        [Description("[2] Center Left")]
        P2 = 2,
        [Description("[3] Center")]
        P3 = 3,
        [Description("[4] Center Right")]
        P4 = 4,
        [Description("[5] Right")]
        P5 = 5

    }
}
