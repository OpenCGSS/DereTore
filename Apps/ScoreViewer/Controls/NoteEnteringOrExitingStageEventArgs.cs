using System;
using DereTore.Apps.ScoreViewer.Model;

namespace DereTore.Apps.ScoreViewer.Controls {
    public sealed class NoteEnteringOrExitingStageEventArgs : EventArgs {

        public NoteEnteringOrExitingStageEventArgs(Note note, bool isEntering) {
            Note = note;
            IsEntering = isEntering;
        }

        public Note Note { get; }

        public bool IsEntering { get; }

    }
}
