using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;
using DereTore.Applications.StarlightDirector.Components;

namespace DereTore.Applications.StarlightDirector.UI.Controls.Pages {
    public partial class AboutPage : IDirectorPage {

        public AboutPage() {
            InitializeComponent();
            CommandHelper.InitializeCommandBindings(this);
        }

        private void AboutPage_OnLoaded(object sender, RoutedEventArgs e) {
            if (!_pageLoaded) {
                OnLoaded();
                _pageLoaded = true;
            }
        }

        private void OnLoaded() {
            var mainAssembly = Assembly.GetEntryAssembly();
            var attributes = mainAssembly.GetCustomAttributes(false);
            var fileVersionAttribute = attributes.FirstOrDefault(a => a is AssemblyFileVersionAttribute) as AssemblyFileVersionAttribute;
            VersionText.Text = fileVersionAttribute?.Version;
            foreach (var contributor in Contributors) {
                if (!string.IsNullOrEmpty(contributor.Value)) {
                    var hyperlink = new Hyperlink();
                    hyperlink.Inlines.Add(contributor.Key);
                    hyperlink.CommandParameter = contributor.Value;
                    ContributorsBlock.Inlines.Add(hyperlink);
                } else {
                    var run = new Run();
                    run.Text = contributor.Key;
                    ContributorsBlock.Inlines.Add(run);
                }
                ContributorsBlock.Inlines.Add(", ");
            }
        }

        public static string VersionPrerelease => "alpha";
        public static string CodeName => "Uzuki";

        private static readonly Dictionary<string, string> Contributors = new Dictionary<string, string> {
            { "CaiMiao", "https://github.com/CaiMiao" },
            { "hyspace", "https://github.com/hyspace"},
            { "のんのん", "https://twitter.com/blueapple25130" },
            { "羽田皐月", "https://twitter.com/iinosuke01" },
            { "MinamiKaze", null },
            { "M.cy★幻光\"", null },
            { "山杉", "https://twitter.com/ymsgu" },
            { "だいずP", "https://twitter.com/DICE__game" },
            { "Ki2317", null },
            { "chieri", "https://github.com/laurencedu" },
            { "2GM2A", null },
            { "dante", null },
            { "Osiris", "https://twitter.com/axiaosiris" }
        };

        private bool _pageLoaded;

    }
}
