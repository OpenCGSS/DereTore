using System.Collections.Generic;
using System.Windows.Controls;
using StarlightDirector.Entities;
using StarlightDirector.UI.Controls.Primitives;

namespace StarlightDirector.UI.Controls {
    public partial class ScoreViewerBase : UserControl {

        public ScoreViewerBase() {
            EditableScoreNotes = new List<ScoreNote>();
            ScoreNotes = EditableScoreNotes.AsReadOnly();
        }

        protected virtual void ReloadScore(Score toBeSet) {
        }

    }
}
