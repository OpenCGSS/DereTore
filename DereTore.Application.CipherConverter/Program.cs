using System;
using System.Globalization;
using System.IO;
using DereTore.HCA;

namespace DereTore.Application.CipherConverter {
    internal static class Program {

        private static int Main(string[] args) {
            if (args.Length < 2) {
                Console.WriteLine(HelpMessage);
                return -1;
            }
            var inputFileName = args[0];
            var outputFileName = args[1];
            CipherConfig ccFrom = new CipherConfig(), ccTo = new CipherConfig();
            try {
                for (var i = 0; i < args.Length; ++i) {
                    var arg = args[i];
                    if (arg[0] == '-' || arg[0] == '/') {
                        switch (arg.Substring(1)) {
                            case "ot":
                                if (i < args.Length - 1) {
                                    var us = ushort.Parse(args[++i]);
                                    if (us != 0 && us != 1 && us != 56) {
                                        Console.WriteLine("ERROR: invalid cipher type.");
                                        return -2;
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
                }
            } catch (Exception) {
                return -3;
            }
            try {
                using (var inputStream = new FileStream(inputFileName, FileMode.Open, FileAccess.Read)) {
                    using (var outputStream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write)) {
                        var converter = new HCA.CipherConverter(inputStream, outputStream, ccFrom, ccTo);
                        converter.Convert();
                    }
                }
            } catch (Exception) {
                return -4;
            }
            return 0;
        }

        private static readonly string HelpMessage = "Usage: hcacc.exe <input HCA> <output HCA> [-ot <output cipher type>] [-i1 <input key 1>] [-i2 <input key 2>] [-o1 <output key 1>] [-o2 <output key 2>]";

    }
}
