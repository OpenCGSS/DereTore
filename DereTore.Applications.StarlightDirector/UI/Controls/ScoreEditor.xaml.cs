using System.Collections.Generic;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    public partial class ScoreEditor {

        public ScoreEditor() {
            EditableScoreNotes = new List<ScoreNote>();
            EditableScoreBars = new List<ScoreBar>();
            EditableSpecialScoreNotes = new List<SpecialNotePointer>();
            ScoreNotes = EditableScoreNotes.AsReadOnly();
            ScoreBars = EditableScoreBars.AsReadOnly();
            SpecialScoreNotes = EditableSpecialScoreNotes.AsReadOnly();

            InitializeComponent();
        }

    }
}
