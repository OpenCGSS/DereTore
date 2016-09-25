using System;
using System.Diagnostics;
using System.Windows;
using DereTore.Applications.StarlightDirector.Components;
using DereTore.Applications.StarlightDirector.Extensions;

namespace DereTore.Applications.StarlightDirector.UI.Pages {
    /// <summary>
    /// SummaryPage.xaml 的交互逻辑
    /// </summary>
    partial class SummaryPage : IDirectorPage {

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
            var mainWindow = this.FindMainWindow();
            var editor = mainWindow.Editor;

            Func<string, string> res = key => Application.Current.FindResource<string>(key);

            t[res(App.ResourceKeys.SummaryMusicFile)] = editor.Project.HasMusic ? editor.Project.MusicFileName : "(none)";
            t[res(App.ResourceKeys.SummaryTotalNotes)] = editor.ScoreNotes.Count.ToString();
            t[res(App.ResourceKeys.SummaryTotalBars)] = editor.ScoreBars.Count.ToString();
        }

    }
}
