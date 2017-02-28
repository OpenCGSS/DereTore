using System.Windows;
using StarlightDirector.Entities;

namespace StarlightDirector.UI.Controls.Pages {
    partial class BuildPage {

        public Project Project {
            get { return (Project)GetValue(ProjectProperty); }
            internal set { SetValue(ProjectProperty, value); }
        }

        public bool CreateLz4CompressedBdbFile {
            get { return (bool)GetValue(CreateLz4CompressedBdbFileProperty); }
            private set { SetValue(CreateLz4CompressedBdbFileProperty, value); }
        }

        public bool IsAcbBuildingEnvironmentOK {
            get { return (bool)GetValue(IsAcbBuildingEnvironmentOKProperty); }
            private set { SetValue(IsAcbBuildingEnvironmentOKProperty, value); }
        }

        public Difficulty MappingDebut {
            get { return (Difficulty)GetValue(MappingDebutProperty); }
            private set { SetValue(MappingDebutProperty, value); }
        }

        public Difficulty MappingRegular {
            get { return (Difficulty)GetValue(MappingRegularProperty); }
            private set { SetValue(MappingRegularProperty, value); }
        }

        public Difficulty MappingPro {
            get { return (Difficulty)GetValue(MappingProProperty); }
            private set { SetValue(MappingProProperty, value); }
        }

        public Difficulty MappingMaster {
            get { return (Difficulty)GetValue(MappingMasterProperty); }
            private set { SetValue(MappingMasterProperty, value); }
        }

        public Difficulty MappingMasterPlus {
            get { return (Difficulty)GetValue(MappingMasterPlusProperty); }
            private set { SetValue(MappingMasterPlusProperty, value); }
        }

        public static readonly DependencyProperty ProjectProperty = DependencyProperty.Register(nameof(Project), typeof(Project), typeof(BuildPage),
            new PropertyMetadata(null));

        public static readonly DependencyProperty CreateLz4CompressedBdbFileProperty = DependencyProperty.Register(nameof(CreateLz4CompressedBdbFile), typeof(bool), typeof(BuildPage),
            new PropertyMetadata(false));

        public static readonly DependencyProperty IsAcbBuildingEnvironmentOKProperty = DependencyProperty.Register(nameof(IsAcbBuildingEnvironmentOK), typeof(bool), typeof(BuildPage),
            new PropertyMetadata(false));

        public static readonly DependencyProperty MappingDebutProperty = DependencyProperty.Register(nameof(MappingDebut), typeof(Difficulty), typeof(BuildPage),
            new PropertyMetadata(Difficulty.Debut));

        public static readonly DependencyProperty MappingRegularProperty = DependencyProperty.Register(nameof(MappingRegular), typeof(Difficulty), typeof(BuildPage),
            new PropertyMetadata(Difficulty.Debut));

        public static readonly DependencyProperty MappingProProperty = DependencyProperty.Register(nameof(MappingPro), typeof(Difficulty), typeof(BuildPage),
            new PropertyMetadata(Difficulty.Debut));

        public static readonly DependencyProperty MappingMasterProperty = DependencyProperty.Register(nameof(MappingMaster), typeof(Difficulty), typeof(BuildPage),
            new PropertyMetadata(Difficulty.Debut));

        public static readonly DependencyProperty MappingMasterPlusProperty = DependencyProperty.Register(nameof(MappingMasterPlus), typeof(Difficulty), typeof(BuildPage),
            new PropertyMetadata(Difficulty.Debut));

    }
}
