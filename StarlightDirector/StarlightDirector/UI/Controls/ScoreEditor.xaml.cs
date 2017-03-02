using System.Collections.Generic;
using StarlightDirector.UI.Controls.Primitives;

namespace StarlightDirector.UI.Controls {
    public partial class ScoreEditor {

        public ScoreEditor() {
            EditableScoreBars = new List<ScoreBar>();
            EditableSpecialScoreNotes = new List<SpecialNotePointer>();
            ScoreBars = EditableScoreBars.AsReadOnly();
            SpecialScoreNotes = EditableSpecialScoreNotes.AsReadOnly();

            InitializeComponent();
        }

    }
}
