namespace DereTore.Interop.UnityEngine.Extensions {
    public static class PPtrExtensions {

        public static PPtr ReadPPtr(this AssetsFile sourceFile) {
            var result = new PPtr();
            var reader = sourceFile.AssetReader;
            var fileID = reader.ReadInt32();
            if (fileID >= 0 && fileID < sourceFile.SharedAssetsList.Count) {
                result.FileID = sourceFile.SharedAssetsList[fileID].Index;
            }
            result.PathID = sourceFile.FormatSignature < 14 ? reader.ReadInt32() : reader.ReadInt64();
            return result;
        }

    }
}
