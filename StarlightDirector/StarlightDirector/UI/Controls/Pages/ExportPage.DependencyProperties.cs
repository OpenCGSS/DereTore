using System.Windows;
using StarlightDirector.Entities;

namespace StarlightDirector.UI.Controls.Pages {
    partial class ExportPage {

        public Score SelectedScore {
            get { return (Score)GetValue(SelectedScoreProperty); }
            set { SetValue(SelectedScoreProperty, value); }
        }

        public static readonly DependencyProperty SelectedScoreProperty = DependencyProperty.Register(nameof(SelectedScore), typeof(Score), typeof(ExportPage),
            new PropertyMetadata(null));

    }
}
