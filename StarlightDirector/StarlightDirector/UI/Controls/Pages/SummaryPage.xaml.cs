using System;
using System.Windows;
using StarlightDirector.Extensions;

namespace StarlightDirector.UI.Controls.Pages {
    public partial class SummaryPage : IDirectorPage {

        public SummaryPage() {
            InitializeComponent();
            Table = new ObservableDictionary<string, string>();
            DataContext = this;
        }

        public ObservableDictionary<string, string> Table { get; }

        private void SummaryPage_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var visible = (bool)e.NewValue;
            if (!visible) {
                return;
            }
            var t = Table;
            t.Clear();
            var mainWindow = this.GetMainWindow();
            var editor = mainWindow.Editor;

            Func<string, string> res = key => Application.Current.FindResource<string>(key);

            t[res(App.ResourceKeys.SummaryMusicFile)] = editor.Project?.HasMusic ?? false ? editor.Project.MusicFileName : "(none)";
            t[res(App.ResourceKeys.SummaryTotalNotes)] = editor.ScoreNotes.Count.ToString();
            t[res(App.ResourceKeys.SummaryTotalBars)] = editor.ScoreBars.Count.ToString();
        }

    }
}
