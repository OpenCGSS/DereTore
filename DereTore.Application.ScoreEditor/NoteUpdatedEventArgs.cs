using System;
using DereTore.Application.ScoreEditor.Model;

namespace DereTore.Application.ScoreEditor {
    public sealed class NoteUpdatedEventArgs : EventArgs {

        public NoteUpdatedEventArgs(Note note) {
            Note = note;
        }

        public Note Note { get; }

    }
}
