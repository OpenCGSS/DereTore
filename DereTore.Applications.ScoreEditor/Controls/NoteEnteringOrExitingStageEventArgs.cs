using System;
using DereTore.Applications.ScoreEditor.Model;

namespace DereTore.Applications.ScoreEditor.Controls {
    public sealed class NoteEnteringOrExitingStageEventArgs : EventArgs {

        public NoteEnteringOrExitingStageEventArgs(Note note, bool isEntering) {
            Note = note;
            IsEntering = isEntering;
        }

        public Note Note { get; }

        public bool IsEntering { get; }

    }
}
