using System;

namespace DereTore.ACB.Test {
    internal static class Program {

        public static void Main(string[] args) {
            if (args.Length < 2) {
                Console.WriteLine("Usage: <EXE> <Input ACB File>");
                return;
            }
            var fileName = args[1];
            var acb = CriAcbFile.FromFile(fileName);
            foreach (var s in acb.GetFileNames()) {
                Console.WriteLine(s);
            }
        }

    }
}
