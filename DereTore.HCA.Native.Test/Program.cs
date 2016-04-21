using System;
using System.Media;

namespace DereTore.HCA.Native.Test {
    public static class Program {
        public static void Main(string[] args) {
            uint key1 = 0, key2 = 0;
            string fileName = null;
            if (args.Length < 2) {
                Console.WriteLine("Usage: <EXE> <File to play> [key1] [key2]");
                //return;
            }
            //fileName = args[1];
            if (args.Length >= 4) {
                key1 = uint.Parse(args[2]);
                key2 = uint.Parse(args[3]);
            }
            fileName = CgssHcaConfig.FileName;
            key1 = CgssHcaConfig.Key1;
            key2 = CgssHcaConfig.Key2;
            using (var hca = new HcaAudioStream(fileName, key1, key2)) {
                using (var sp = new SoundPlayer(hca)) {
                    sp.PlaySync();
                }
            }
        }
    }
}
