using System.Windows;

namespace StarlightDirector.UI.Controls.Primitives {
    public partial class ScoreNote {

        public ScoreNote() {
            InitializeComponent();
            Radius = DefaultRadius;
        }

        private void ScoreNote_OnSizeChanged(object sender, SizeChangedEventArgs e) {
            // TODO: recalc note symbol size
        }

        private static readonly double DefaultRadius = 15;

    }
}
