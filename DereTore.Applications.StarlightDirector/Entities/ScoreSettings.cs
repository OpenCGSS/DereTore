namespace DereTore.Applications.StarlightDirector.Entities {
    public sealed class ScoreSettings {

        /// <summary>
        /// Tempo，每分钟四分音符出现次数。
        /// </summary>
        public double GlobalBpm { get; set; }

        public double StartTimeOffset { get; set; }

        /// <summary>
        /// 细分级别，一个四分音符被分成多少份。
        /// 例如，分成2份，拍号3（3/4拍），速度120，则每一个小节长度为1.5秒（=60÷120×3），每个note定位精度为一个八分音符（=1/2四分音符）。
        /// </summary>
        public int GlobalGridPerSignature { get; set; }

        /// <summary>
        /// 拍号，以四分音符为标准，即 x/4 拍。
        /// </summary>
        public int GlobalSignature { get; set; }

        public static ScoreSettings CreateDefault() {
            return new ScoreSettings {
                GlobalBpm = 120,
                StartTimeOffset = 0,
                GlobalGridPerSignature = 4, // 最高分辨率为十六分音符
                GlobalSignature = 4 // 4/4拍
            };
        }

        private ScoreSettings() {
        }

    }
}
