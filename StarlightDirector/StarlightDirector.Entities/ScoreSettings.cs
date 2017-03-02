using System;
using System.Windows;
using DereTore.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace StarlightDirector.Entities {
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), MemberSerialization = MemberSerialization.OptIn)]
    public sealed class ScoreSettings : DependencyObject, ICloneable {

        public event EventHandler<EventArgs> SettingChanged;

        /// <summary>
        /// Tempo，每分钟四分音符出现次数。
        /// </summary>
        [JsonProperty]
        public double GlobalBpm {
            get { return (double)GetValue(GlobalBpmProperty); }
            set { SetValue(GlobalBpmProperty, value); }
        }

        [JsonProperty]
        public double StartTimeOffset {
            get { return (double)GetValue(StartTimeOffsetProperty); }
            set { SetValue(StartTimeOffsetProperty, value); }
        }

        /// <summary>
        /// 细分级别，一个四分音符被分成多少份。
        /// 例如，分成2份，拍号3（3/4拍），速度120，则每一个小节长度为1.5秒（=60÷120×3），每个note定位精度为一个八分音符（=1/2四分音符）。
        /// </summary>
        [JsonProperty]
        public int GlobalGridPerSignature { get; set; }

        /// <summary>
        /// 拍号，以四分音符为标准，即 x/4 拍。
        /// </summary>
        [JsonProperty]
        public int GlobalSignature { get; set; }

        public static ScoreSettings CreateDefault() {
            return new ScoreSettings {
                GlobalBpm = DefaultGlobalBpm,
                StartTimeOffset = DefaultStartTimeOffset,
                GlobalGridPerSignature = DefaultGlobalGridPerSignature, // 最高分辨率为九十六分音符
                GlobalSignature = DefaultGlobalSignature // 4/4拍
            };
        }

        public static readonly DependencyProperty GlobalBpmProperty = DependencyProperty.Register(nameof(GlobalBpm), typeof(double), typeof(ScoreSettings),
            new PropertyMetadata(120d, OnGlobalBpmChanged));

        public static readonly DependencyProperty StartTimeOffsetProperty = DependencyProperty.Register(nameof(StartTimeOffset), typeof(double), typeof(ScoreSettings),
            new PropertyMetadata(0d, OnStartTimeOffsetChanged));

        public static readonly double DefaultGlobalBpm = 120;
        public static readonly double DefaultStartTimeOffset = 0;
        public static readonly int DefaultGlobalGridPerSignature = 24;
        public static readonly int DefaultGlobalSignature = 4;

        public ScoreSettings Clone() {
            return new ScoreSettings {
                GlobalBpm = GlobalBpm,
                StartTimeOffset = StartTimeOffset,
                GlobalGridPerSignature = GlobalGridPerSignature,
                GlobalSignature = GlobalSignature
            };
        }

        public void CopyFrom(ScoreSettings settings) {
            if (settings == null) {
                throw new ArgumentNullException(nameof(settings));
            }
            GlobalBpm = settings.GlobalBpm;
            StartTimeOffset = settings.StartTimeOffset;
            GlobalGridPerSignature = settings.GlobalGridPerSignature;
            GlobalSignature = settings.GlobalSignature;
        }

        private static void OnGlobalBpmChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var settings = (ScoreSettings)obj;
            settings.SettingChanged.Raise(obj, EventArgs.Empty);
        }

        private static void OnStartTimeOffsetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var settings = (ScoreSettings)obj;
            settings.SettingChanged.Raise(obj, EventArgs.Empty);
        }

        [JsonConstructor]
        private ScoreSettings() {
        }

        object ICloneable.Clone() {
            return Clone();
        }

    }
}
