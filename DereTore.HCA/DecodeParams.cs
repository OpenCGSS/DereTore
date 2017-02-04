namespace DereTore.HCA {
    public struct DecodeParams {

        public uint Key1 { get; set; }
        public uint Key2 { get; set; }
        public SamplingMode Mode { get; set; }
        public float Volume { get; set; }

        public static DecodeParams CreateDefault() {
            return new DecodeParams {
                Key1 = 0,
                Key2 = 0,
                Mode = SamplingMode.S16,
                Volume = 1.0f
            };
        }

        public static readonly DecodeParams Default = CreateDefault();

    }
}
