using System;

namespace DereTore.HCA {
    public sealed class DecodeParam : ICloneable {

        public DecodeParam() {
            Key1 = 0;
            Key2 = 0;
            Mode = SamplingMode.S16;
            Volume = 1.0f;
            Loop = 0;
            EnableLoop = false;
        }

        public uint Key1 { get; set; }
        public uint Key2 { get; set; }
        public SamplingMode Mode { get; set; }
        public float Volume { get; set; }
        public int Loop { get; set; }
        public bool EnableLoop { get; set; }

        public DecodeParam Clone() {
            return new DecodeParam() {
                Key1 = Key1,
                Key2 = Key2,
                Loop = Loop,
                Mode = Mode,
                Volume = Volume
            };
        }

        object ICloneable.Clone() {
            return Clone();
        }

    }
}
