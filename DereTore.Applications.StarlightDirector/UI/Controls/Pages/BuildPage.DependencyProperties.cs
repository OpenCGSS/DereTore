using System.Windows;
using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector.UI.Controls.Pages {
    partial class BuildPage {

        public Project Project {
            get { return (Project)GetValue(ProjectProperty); }
            set { SetValue(ProjectProperty, value); }
        }

        public bool CreateLz4CompressedAcbFile {
            get { return (bool)GetValue(CreateLz4CompressedAcbFileProperty); }
            set { SetValue(CreateLz4CompressedAcbFileProperty, value); }
        }

        public bool CreateNameHashedAcbFile {
            get { return (bool)GetValue(CreateNameHashedAcbFileProperty); }
            set { SetValue(CreateNameHashedAcbFileProperty, value); }
        }

        public bool CreateNameHashedBdbFile {
            get { return (bool)GetValue(CreateNameHashedBdbFileProperty); }
            set { SetValue(CreateNameHashedBdbFileProperty, value); }
        }

        public static readonly DependencyProperty ProjectProperty = DependencyProperty.Register(nameof(Project), typeof(Project), typeof(BuildPage),
            new PropertyMetadata(null));

        public static readonly DependencyProperty CreateLz4CompressedAcbFileProperty = DependencyProperty.Register(nameof(CreateLz4CompressedAcbFile), typeof(bool), typeof(BuildPage),
            new PropertyMetadata(false));

        public static readonly DependencyProperty CreateNameHashedAcbFileProperty = DependencyProperty.Register(nameof(CreateNameHashedAcbFile), typeof(bool), typeof(BuildPage),
            new PropertyMetadata(false));

        public static readonly DependencyProperty CreateNameHashedBdbFileProperty = DependencyProperty.Register(nameof(CreateNameHashedBdbFile), typeof(bool), typeof(BuildPage),
            new PropertyMetadata(false));

    }
}
