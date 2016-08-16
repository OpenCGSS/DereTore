using System;
using DereTore.Application.ScoreEditor.Model;

namespace DereTore.Application.ScoreEditor.Controls {
    public sealed class NoteEnteringOrExitingStageEventArgs : EventArgs {

        public NoteEnteringOrExitingStageEventArgs(Note note) {
            Note = note;
        }

        public Note Note { get; }

    }
}
