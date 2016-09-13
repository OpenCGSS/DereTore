using System;
using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector.UI {
    public sealed class ScoreChangedEventArgs : EventArgs {

        internal ScoreChangedEventArgs(Score oldValue, Score newValue) {
            Old = oldValue;
            New = newValue;
        }

        public Score Old { get; }

        public Score New { get; }

    }
}
