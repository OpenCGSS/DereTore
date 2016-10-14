using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DereTore.Applications.StarlightDirector.Entities {
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class BarParams {

        public double? UserDefinedBpm { get; internal set; }

        public int? UserDefinedGridPerSignature { get; internal set; }

        public int? UserDefinedSignature { get; internal set; }

        [JsonIgnore]
        public bool CanBeSquashed => UserDefinedBpm == null && UserDefinedGridPerSignature == null && UserDefinedSignature == null;

    }
}
