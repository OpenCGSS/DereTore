using System;

namespace DereTore.Exchange.UnityEngine.Serialization {
    public sealed class BundleOptions {

        public BundleOptions() {
            PvrWidth = PvrHeight = SmallImageSize;
            DdsWidth = DdsHeight = MediumImageSize;
            SongID = DefaultSongID;
            Platform = UnityPlatformID.Android;
        }

        public byte[] PvrImage { get; set; }
        public int PvrWidth { get; set; }
        public int PvrHeight { get; set; }
        public long PvrPathID { get; set; }
        public byte[] DdsImage { get; set; }
        public int DdsWidth { get; set; }
        public int DdsHeight { get; set; }
        public long DdsPathID { get; set; }

        public int SongID {
            get { return _songID; }
            set { _songID = Math.Abs(value) % 10000; }
        }

        public int Platform { get; set; }

        public const int SmallImageSize = 128;
        public const int MediumImageSize = 264;
        public const int DefaultSongID = 1001;

        private int _songID;

    }
}
