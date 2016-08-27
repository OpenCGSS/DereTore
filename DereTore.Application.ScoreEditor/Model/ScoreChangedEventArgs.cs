using System;

namespace DereTore.Application.ScoreEditor.Model {
    public sealed class ScoreChangedEventArgs : EventArgs {

        public ScoreChangedEventArgs(ScoreChangeReason reason, Note note) {
            Reason = reason;
            Note = note;
        }

        public Note Note { get; }

        public ScoreChangeReason Reason { get; }

    }
}
