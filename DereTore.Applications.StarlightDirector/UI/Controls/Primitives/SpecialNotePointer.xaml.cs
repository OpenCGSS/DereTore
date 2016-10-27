using System;
using System.Windows;
using DereTore.Applications.StarlightDirector.Extensions;

namespace DereTore.Applications.StarlightDirector.UI.Controls.Primitives {
    public partial class SpecialNotePointer {

        public SpecialNotePointer() {
            InitializeComponent();
            CommandHelper.InitializeCommandBindings(this);
        }

        private void SetEditingState(bool editing) {
            BpmDisplay.Visibility = editing ? Visibility.Collapsed : Visibility.Visible;
            BpmEditor.Visibility = editing ? Visibility.Visible : Visibility.Collapsed;
            _isEditing = editing;
        }

        private void Note_ExtraParamsChanged(object sender, EventArgs e) {
            var editor = this.FindVisualParent<ScoreEditor>();
            editor?.UpdateBarTexts();
        }

        private bool _isEditing;

    }
}
