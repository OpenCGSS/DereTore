using System.ComponentModel;

namespace StarlightDirector.Entities {
    public enum Difficulty {

        Invalid,
        [Description("Debut")]
        Debut,
        [Description("Regular")]
        Regular,
        [Description("Pro")]
        Pro,
        [Description("Master")]
        Master,
        [Description("Master+")]
        MasterPlus

    }
}
