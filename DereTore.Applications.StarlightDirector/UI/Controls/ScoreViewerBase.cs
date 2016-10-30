using System.Collections.Generic;
using System.Windows.Controls;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.UI.Controls.Primitives;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    public partial class ScoreViewerBase : UserControl {

        public ScoreViewerBase() {
            EditableScoreNotes = new List<ScoreNote>();
            ScoreNotes = EditableScoreNotes.AsReadOnly();
        }

        protected virtual void ReloadScore(Score toBeSet) {
        }

    }
}
