using System.Windows;
using System.Windows.Controls;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    public class ScoreViewerBase : UserControl {

        public bool AreRelationIndicatorsVisible {
            get { return (bool)GetValue(AreRelationIndicatorsVisibleProperty); }
            set { SetValue(AreRelationIndicatorsVisibleProperty, value); }
        }

        public static readonly DependencyProperty AreRelationIndicatorsVisibleProperty = DependencyProperty.Register(nameof(AreRelationIndicatorsVisible), typeof(bool), typeof(ScoreEditor),
         new PropertyMetadata(false));

    }
}
