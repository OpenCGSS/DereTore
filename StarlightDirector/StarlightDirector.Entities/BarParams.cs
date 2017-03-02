using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace StarlightDirector.Entities {
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class BarParams {

        [Obsolete("This property is not used since v0.5.0 alpha. Please consider Note with Note.Type == VariantBpm instead.")]
        public double? UserDefinedBpm { get; internal set; }

        public int? UserDefinedGridPerSignature { get; internal set; }

        public int? UserDefinedSignature { get; internal set; }

        [JsonIgnore]
        public bool CanBeSquashed => UserDefinedBpm == null && UserDefinedGridPerSignature == null && UserDefinedSignature == null;

    }
}
