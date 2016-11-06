using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace DereTore.Interop.UnityEngine {
    public sealed partial class BundleFile : DisposableBase {

        public BundleFile(FileStream fileStream)
            : this(fileStream, fileStream.Name) {
        }

        public BundleFile(Stream fileStream, string fileName) {
            FileName = fileName;
            MemoryAssetsFiles = _memoryAssetsFiles.AsReadOnly();
            using (var reader = new EndianBinaryReader(fileStream, Endian.BigEndian)) {
                Initialize(reader);
            }
        }

        public string FileName { get; }

        public int Format { get; internal set; }

        public string PlayerVersion { get; internal set; }

        public string EngineVersion { get; internal set; }

        public ReadOnlyCollection<MemoryFileStream> MemoryAssetsFiles { get; }

        public string Signature { get; internal set; }

        // ------- Properties for serialization only ------
        public long BundleSize { get; internal set; }

        public int BaseOffset { get; internal set; }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                foreach (var memoryAssetsFile in MemoryAssetsFiles) {
                    memoryAssetsFile.Dispose();
                }
                _memoryAssetsFiles.Clear();
            }
        }

        internal BundleFile() {
        }

        private readonly List<MemoryFileStream> _memoryAssetsFiles = new List<MemoryFileStream>();

    }
}
