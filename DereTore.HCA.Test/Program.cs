using System;
using System.Globalization;
using System.IO;
using System.Media;

namespace DereTore.HCA.Test {
    internal static class Program {

        public static void Main(string[] args) {
            TestPlayHca(args);
        }

        private static void TestPlayHca(string[] args) {
            string fileName;
            uint key1, key2;

#if false
            key1 = CgssHcaConfig.Key1;
            key2 = CgssHcaConfig.Key2;
            fileName = CgssHcaConfig.FileName;
#else
            if (args.Length < 1) {
                Console.WriteLine("Usage: <EXE> <hca file> <key1> <key2>");
                return;
            }

            if (args.Length < 3) {
                key1 = 0;
                key2 = 0;
            } else {
                key1 = uint.Parse(args[1]);
                key2 = uint.Parse(args[2]);
            }
            fileName = args[0];
#endif

            var param = new DecodeParams() { Key1 = key1, Key2 = key2 };
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {
                using (var hca = new HcaAudioStream(fs, param)) {
                    using (var sp = new SoundPlayer(hca)) {
                        sp.LoadTimeout = 5000000;
                        sp.PlaySync();
                    }
                }
            }
        }

    }
}