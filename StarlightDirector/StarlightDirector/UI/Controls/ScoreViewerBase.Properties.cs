using System.Collections.Generic;
using System.Collections.ObjectModel;
using StarlightDirector.UI.Controls.Primitives;

namespace StarlightDirector.UI.Controls {
    partial class ScoreViewerBase {

        public ReadOnlyCollection<ScoreNote> ScoreNotes { get; }

        protected List<ScoreNote> EditableScoreNotes { get; }

    }
}
