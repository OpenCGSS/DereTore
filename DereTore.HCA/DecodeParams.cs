namespace DereTore.HCA {
    public struct DecodeParams {

        public uint Key1 { get; set; }
        public uint Key2 { get; set; }
        public SamplingMode Mode { get; set; }
        public float Volume { get; set; }
        public bool CipherTypeOverrideEnabled { get; set; }
        public CipherType OverriddenCipherType { get; set; }

        public static DecodeParams CreateDefault() {
            return new DecodeParams {
                Key1 = 0,
                Key2 = 0,
                Mode = SamplingMode.S16,
                Volume = 1.0f,
                CipherTypeOverrideEnabled = false,
                OverriddenCipherType = CipherType.NoChipher
            };
        }

        public static readonly DecodeParams Default = CreateDefault();

    }
}
