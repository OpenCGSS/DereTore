using System;

namespace DereTore.Interop.UnityEngine {
    partial class BundleFile {

        private void Initialize(EndianBinaryReader reader) {
            var signature = reader.ReadAsciiStringToNull();
            Signature = signature;
            if (signature == BundleFileSignature.Raw || signature == BundleFileSignature.Web || signature == BundleFileSignature.Special) {
                LoadOldFormats(reader);
            } else if (signature == BundleFileSignature.FS) {
                LoadFSv5(reader);
            }
        }

        private void LoadOldFormats(EndianBinaryReader reader) {
            var format = reader.ReadInt32();
            Format = format;
            PlayerVersion = reader.ReadAsciiStringToNull();
            EngineVersion = reader.ReadAsciiStringToNull();
            if (format < 6) {
                BundleSize = reader.ReadInt32();
            } else {
                BundleSize = reader.ReadInt64();
                return;
            }
            var dummy2 = reader.ReadInt16();
            int baseOffset = reader.ReadInt16();
            BaseOffset = baseOffset;
            var dummy3 = reader.ReadInt32();
            var lzmaChunks = reader.ReadInt32();

            var lzmaSize = 0;
            long streamSize = 0;

            for (var i = 0; i < lzmaChunks; i++) {
                lzmaSize = reader.ReadInt32();
                streamSize = reader.ReadInt32();
            }

            reader.Position = baseOffset;
            switch (Signature) {
                case BundleFileSignature.Special: //.bytes
                case BundleFileSignature.Web:
                    throw new NotSupportedException($"Bundle type {Signature} is not supported.");
                case BundleFileSignature.Raw:
                    ExtractFiles(reader, baseOffset);
                    break;
                default:
                    throw new ApplicationException("What?");
            }
        }

        private void LoadFSv5(EndianBinaryReader reader) {
            throw new NotSupportedException("UnityFS bundles are not supported.");
        }

        private void ExtractFiles(EndianBinaryReader reader, int baseOffset) {
            var fileCount = reader.ReadInt32();
            for (var i = 0; i < fileCount; i++) {
                var fileName = reader.ReadAsciiStringToNull();
                var fileOffset = reader.ReadInt32();
                fileOffset += baseOffset;
                var fileSize = reader.ReadInt32();
                var nextFile = reader.Position;
                reader.Position = fileOffset;

                var buffer = new byte[fileSize];
                reader.Read(buffer, 0, fileSize);

                var memoryFileStream = new MemoryFileStream(buffer, false);
                memoryFileStream.FileName = fileName;
                _memoryAssetsFiles.Add(memoryFileStream);

                reader.Position = nextFile;
            }
        }

    }
}
