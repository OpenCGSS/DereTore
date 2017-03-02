using System;

namespace StarlightDirector.Exchange.Deleste.CommandParameters {
    internal sealed class ChangeBpmCommandParameters : ICloneable {

        public int StartMeasureIndex { get; set; }

        public double NewBpm { get; set; }

        public ChangeBpmCommandParameters Clone() {
            return new ChangeBpmCommandParameters {
                StartMeasureIndex = StartMeasureIndex,
                NewBpm = NewBpm
            };
        }

        object ICloneable.Clone() {
            return Clone();
        }

    }
}
