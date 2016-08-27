using System;
using DereTore.Application.ScoreEditor.Model;

namespace DereTore.Application.ScoreEditor.Controls {
    public sealed class NoteEnteringOrExitingStageEventArgs : EventArgs {

        public NoteEnteringOrExitingStageEventArgs(Note note, bool isEntering) {
            Note = note;
            IsEntering = isEntering;
        }

        public Note Note { get; }

        public bool IsEntering { get; }

    }
}
