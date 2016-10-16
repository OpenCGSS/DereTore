using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;

namespace DereTore.Applications.StarlightDirector.UI.Controls.Pages {
    public partial class AboutPage : IDirectorPage {

        public AboutPage() {
            InitializeComponent();
            CommandHelper.InitializeCommandBindings(this);
        }

        private void AboutPage_OnLoaded(object sender, RoutedEventArgs e) {
            if (_pageLoaded) {
                return;
            }
            OnLoaded();
            _pageLoaded = true;
        }

        private void OnLoaded() {
            var mainAssembly = Assembly.GetEntryAssembly();
            var attributes = mainAssembly.GetCustomAttributes(false);
            var fileVersionAttribute = attributes.FirstOrDefault(a => a is AssemblyFileVersionAttribute) as AssemblyFileVersionAttribute;
            VersionText.Text = fileVersionAttribute?.Version;
            Contributors.Sort((kv1, kv2) => string.CompareOrdinal(kv1.Key, kv2.Key));
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
        public static string CodeName => "Rin";

        private static readonly List<KeyValuePair<string, string>> Contributors = new List<KeyValuePair<string, string>> {
            new KeyValuePair<string, string>("2GM2A", null),
            new KeyValuePair<string, string>("CaiMiao", "https://github.com/CaiMiao"),
            new KeyValuePair<string, string>("Ki2317", null),
            new KeyValuePair<string, string>("M.cy★幻光\"", null),
            new KeyValuePair<string, string>("MinamiKaze", null),
            new KeyValuePair<string, string>("Osiris", "https://twitter.com/axiaosiris"),
            new KeyValuePair<string, string>("chieri", "https://github.com/laurencedu"),
            new KeyValuePair<string, string>("dante", null),
            new KeyValuePair<string, string>("hyspace", "https://github.com/hyspace"),
            new KeyValuePair<string, string>("statementreply", "https://github.com/statementreply"),
            new KeyValuePair<string, string>("だいずP", "https://twitter.com/DICE__game"),
            new KeyValuePair<string, string>("のんのん", "https://twitter.com/blueapple25130"),
            new KeyValuePair<string, string>("山杉", "https://twitter.com/ymsgu"),
            new KeyValuePair<string, string>("羽田皐月", "https://twitter.com/iinosuke01"),
        };

        private bool _pageLoaded;

    }
}
