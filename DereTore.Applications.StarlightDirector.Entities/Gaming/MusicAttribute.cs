using System;
using System.ComponentModel;

namespace DereTore.Applications.StarlightDirector.Entities.Gaming {
    [Flags]
    public enum MusicAttribute {
        
        [Description("Cute")]
        Cute = 0x01,
        [Description("Cool")]
        Cool = 0x02,
        [Description("Passion")]
        Passion = 0x04,
        [Description("Multicolor")]
        AllTypes = 0x08,
        [Description("Event")]
        Event = 0x10,
        [Description("Solo ver.")]
        Solo = 0x20

    }
}
