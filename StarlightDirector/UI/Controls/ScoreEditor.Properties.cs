using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using StarlightDirector.UI.Controls.Primitives;

namespace StarlightDirector.UI.Controls {
    partial class ScoreEditor {

        public ReadOnlyCollection<ScoreBar> ScoreBars { get; }

        public ReadOnlyCollection<SpecialNotePointer> SpecialScoreNotes { get; }

        public ScoreBarHitTestInfo LastHitTestInfo { get; private set; }

        public Grid ContentsGridControl => ContentsGrid;

        private List<ScoreBar> EditableScoreBars { get; }

        private List<SpecialNotePointer> EditableSpecialScoreNotes { get; }

        private ScoreNote DraggingStartNote { get; set; }

        private ScoreNote DraggingEndNote { get; set; }

    }
}
