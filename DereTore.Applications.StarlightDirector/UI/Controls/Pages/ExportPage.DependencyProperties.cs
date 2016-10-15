using System.Windows;
using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector.UI.Controls.Pages {
    partial class ExportPage {

        public Score SelectedScore {
            get { return (Score)GetValue(SelectedScoreProperty); }
            set { SetValue(SelectedScoreProperty, value); }
        }

        public static readonly DependencyProperty SelectedScoreProperty = DependencyProperty.Register(nameof(SelectedScore), typeof(Score), typeof(ExportPage),
            new PropertyMetadata(null));

    }
}
