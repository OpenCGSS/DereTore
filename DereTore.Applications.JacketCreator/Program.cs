using System;
using System.Drawing;
using System.IO;
using CommandLine;
using CommandLine.Text;
using DereTore.Interop.D3DX9;
using DereTore.Interop.PVRTexLib;
using DereTore.Interop.UnityEngine;
using DereTore.Interop.UnityEngine.Serialization;

namespace DereTore.Applications.JacketCreator {
    internal static class Program {

        private static void Main(string[] args) {


            var options = new Options();
            var isOptionsValid = Parser.Default.ParseArguments(args, options);
            if (!isOptionsValid) {
                var helpText = HelpText.AutoBuild(options);
                HelpText.DefaultParsingErrorsHandler(options, helpText);
                Console.WriteLine(helpText);
                return;
            }

            options.SongID = Math.Abs(options.SongID) % 10000;
            if (string.IsNullOrEmpty(options.ImageFileName) && !File.Exists(options.ImageFileName)) {
                Console.WriteLine($"ERROR: image file '{options.ImageFileName}' not found.");
                return;
            }
            var fullDirectoryName = (new DirectoryInfo(options.OutputDirectory)).FullName;
            if (!Directory.Exists(fullDirectoryName)) {
                try {
                    Directory.CreateDirectory(fullDirectoryName);
                } catch (Exception ex) {
                    Console.WriteLine($"ERROR: Tried to create directory '{fullDirectoryName}' but failed.\n{ex.Message}");
                    return;
                }
            }

            Bitmap bitmap;
            try {
                bitmap = (Bitmap)Image.FromFile(options.ImageFileName);
            } catch (Exception ex) {
                Console.WriteLine($"ERROR: Cannot read image file '{options.ImageFileName}'.\n{ex.Message}");
                return;
            }

            const int smallImageWidth = 128;
            const int smallImageHeight = 128;
            const int mediumImageWidth = 264;
            const int mediumImageHeight = 264;
            byte[] pvr, dds;
            using (var smallImage = new Bitmap(bitmap, smallImageWidth, smallImageHeight)) {
                pvr = PvrUtilities.GetPvrTextureFromImage(smallImage);
            }
            using (var mediumImage = new Bitmap(bitmap, mediumImageWidth, mediumImageHeight)) {
                dds = DdsUtilities.GetDdsTextureFromImage(mediumImage);
            }
            bitmap.Dispose();

            var fileName = fullDirectoryName + $"jacket_{options.SongID}_android.unity3d";
            using (var fileStream = File.Open(fileName, FileMode.Create, FileAccess.Write)) {
                JacketBundle.Serialize(pvr, smallImageWidth, smallImageHeight, dds, mediumImageWidth, mediumImageHeight, options.SongID, UnityPlatformID.Android, fileStream);
            }
            fileName = fullDirectoryName + $"jacket_{options.SongID}_ios.unity3d";
            using (var fileStream = File.Open(fileName, FileMode.Create, FileAccess.Write)) {
                JacketBundle.Serialize(pvr, smallImageWidth, smallImageHeight, dds, mediumImageWidth, mediumImageHeight, options.SongID, UnityPlatformID.iOS, fileStream);
            }
            Console.WriteLine($"Building complete. Files are written to '{fullDirectoryName}', song ID = {options.SongID}.");
        }

    }
}
