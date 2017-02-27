using System;
using System.Windows.Input;

namespace StarlightDirector.UI {
    public sealed class ScoreBarHitTestEventArgs : EventArgs {

        public ScoreBarHitTestEventArgs(ScoreBarHitTestInfo info, MouseButtonEventArgs buttonEventArgs) {
            Info = info;
            ButtonEventArgs = buttonEventArgs;
        }

        public ScoreBarHitTestInfo Info { get; }

        public MouseButtonEventArgs ButtonEventArgs { get; }

    }
}
