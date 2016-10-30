using System.Collections.Generic;
using System.Collections.ObjectModel;
using DereTore.Applications.StarlightDirector.UI.Controls.Primitives;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    partial class ScoreViewerBase {

        public ReadOnlyCollection<ScoreNote> ScoreNotes { get; }

        protected List<ScoreNote> EditableScoreNotes { get; }

    }
}
