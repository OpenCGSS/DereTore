using System;

namespace DereTore.Applications.ScoreEditor {
    internal static class PlayerSettings {

        public static float MusicVolume {
            get { return _musicVolume; }
            set {
                value = MathHelper.Clamp(value, 0f, 1f);
                var b = !value.Equals(_musicVolume);
                if (b) {
                    _musicVolume = value;
                    MusicVolumeChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        public static event EventHandler<EventArgs> MusicVolumeChanged;

        public static float SfxVolume {
            get { return _sfxVolume; }
            set {
                value = MathHelper.Clamp(value, 0f, 1f);
                _sfxVolume = value;
            }
        }

        // Compensates for ~3ms silence at the beginning of SFX files
        public static TimeSpan SfxOffset { get; set; } = new TimeSpan(0, 0, 0, 0, -3);

        private static float _musicVolume = 0.7f;
        private static float _sfxVolume = 0.5f;

    }
}
