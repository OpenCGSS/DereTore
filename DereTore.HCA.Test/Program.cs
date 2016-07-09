using System;
using System.Globalization;
using System.IO;
using System.Media;

namespace DereTore.HCA.Test {
    internal static class Program {

        public static void Main(string[] args) {
            TestHcaCipherConversion(args);
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

        private static void TestHcaCipherConversion(string[] args) {
            const string helpMessage = "Usage: <EXE> <input HCA> <output HCA> [-ot <output cipher type>] [-i1 <input key 1>] [-i2 <input key 2>] [-o1 <output key 1>] [-o2 <output key 2>]";
            if (args.Length < 2) {
                Console.WriteLine(helpMessage);
                return;
            }
            var inputFileName = args[0];
            var outputFileName = args[1];
            CipherConfig ccFrom = new CipherConfig(), ccTo = new CipherConfig();
            for (var i = 0; i < args.Length; ++i) {
                var arg = args[i];
                if (arg[0] == '-' || arg[0] == '/') {
                    switch (arg.Substring(1)) {
                        case "ot":
                            if (i < args.Length - 1) {
                                var us = ushort.Parse(args[++i]);
                                if (us != 0 && us != 1 && us != 56) {
                                    Console.WriteLine("ERROR: invalid cipher type.");
                                    return;
                                }
                                ccTo.CipherType = (CipherType)us;
                            }
                            break;
                        case "i1":
                            if (i < args.Length - 1) {
                                ccFrom.Key1 = uint.Parse(args[++i], NumberStyles.HexNumber);
                            }
                            break;
                        case "i2":
                            if (i < args.Length - 1) {
                                ccFrom.Key2 = uint.Parse(args[++i], NumberStyles.HexNumber);
                            }
                            break;
                        case "o1":
                            if (i < args.Length - 1) {
                                ccTo.Key1 = uint.Parse(args[++i], NumberStyles.HexNumber);
                            }
                            break;
                        case "o2":
                            if (i < args.Length - 1) {
                                ccTo.Key2 = uint.Parse(args[++i], NumberStyles.HexNumber);
                            }
                            break;
                    }
                }
                using (var inputStream = new FileStream(inputFileName, FileMode.Open, FileAccess.Read)) {
                    using (var outputStream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write)) {
                        var converter = new HcaCipherConverter(inputStream, outputStream, ccFrom, ccTo);
                        converter.ParseHeaders();
                        converter.InitializeCiphers();
                        converter.SetNewCipherType();
                        converter.ConvertData();
                    }
                }
            }

        }

    }
}