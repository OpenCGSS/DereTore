using System;
using System.IO;

namespace DereTore.Applications.StarlightDirector {
    public static class DirectorHelper {

        public static double BpmToSeconds(double bpm) {
            return 60 / bpm;
        }

        // http://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path
        // For project music file.
        public static string ResolveRelativeFileName(string fromPath, string toPath) {
            if (string.IsNullOrEmpty(fromPath)) {
                throw new ArgumentNullException(nameof(fromPath));
            }
            if (string.IsNullOrEmpty(toPath)) {
                throw new ArgumentNullException(nameof(toPath));
            }

            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);
            if (fromUri.Scheme != toUri.Scheme) {
                // path can't be made relative.
                return toPath;
            }
            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());
            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase)) {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }
            return relativePath;
        }

    }
}
