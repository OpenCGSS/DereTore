using System.Windows;
using System.Windows.Input;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
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

        private bool _isEditing = false;

    }
}
