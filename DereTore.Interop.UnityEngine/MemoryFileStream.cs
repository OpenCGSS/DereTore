using System.IO;

namespace DereTore.Interop.UnityEngine {
    public sealed class MemoryFileStream : MemoryStream {

        public MemoryFileStream() {
        }

        public MemoryFileStream(byte[] buffer)
            : base(buffer) {
        }

        public MemoryFileStream(byte[] buffer, bool writable)
            : base(buffer, writable) {
        }

        public string FileName { get; set; }

    }
}
