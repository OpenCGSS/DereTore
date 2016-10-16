using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DereTore.Applications.ScoreEditor.Controls {
    internal static class PlayerSettings {
        private static float _musicVolume = 0.7f;

        public static float MusicVolume {
            get { return _musicVolume; }
            set {
                _musicVolume = value;
                MusicVolumeChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static event EventHandler<EventArgs> MusicVolumeChanged;

        public static float SfxVolume { get; set; } = 0.5f;

        // Compensates for ~3ms silence at the beginning of SFX files
        public static TimeSpan SfxOffset { get; set; } = new TimeSpan(0, 0, 0, 0, -3);
    }
}
