using System;

namespace StarlightDirector.Exchange.Deleste {
    internal sealed class DelesteState : ICloneable {

        public double BPM { get; set; }

        public int Signature { get; set; }

        public DelesteState Clone() {
            return new DelesteState {
                BPM = BPM,
                Signature = Signature
            };
        }

        object ICloneable.Clone() {
            return Clone();
        }

    }
}
