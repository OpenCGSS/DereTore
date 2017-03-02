using DereTore.Exchange.UnityEngine.Extensions;

namespace DereTore.Exchange.UnityEngine.UnityClasses {
    public sealed class BuildSettings {

        public BuildSettings(AssetPreloadData preloadData) {
            var sourceFile = preloadData.SourceFile;
            var reader = preloadData.SourceFile.AssetReader;
            reader.Position = preloadData.Offset;

            var levels = reader.ReadInt32();
            for (var l = 0; l < levels; l++) {
                var level = reader.ReadAlignedUtf8String(reader.ReadInt32());
            }

            if (sourceFile.RawVersion[0] == 5) {
                var preloadedPlugins = reader.ReadInt32();
                for (var l = 0; l < preloadedPlugins; l++) {
                    var preloadedPlugin = reader.ReadAlignedUtf8String(reader.ReadInt32());
                }
            }

            reader.Position += 4;
            if (sourceFile.FormatSignature >= 8) {
                reader.Position += 4;
            }
            if (sourceFile.FormatSignature >= 9) {
                reader.Position += 4;
            }
            if (sourceFile.RawVersion[0] == 5 || (sourceFile.RawVersion[0] == 4 && (sourceFile.RawVersion[1] >= 3 || (sourceFile.RawVersion[1] == 2 && sourceFile.BuildTypes[0] != "a")))) {
                reader.Position += 4;
            }

            var versionLength = reader.ReadInt32();
            VersionString = reader.ReadAlignedUtf8String(versionLength);
        }

        public string VersionString { get; private set; }

    }
}
