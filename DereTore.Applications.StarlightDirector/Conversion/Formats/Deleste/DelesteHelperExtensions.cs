using System.Linq;

namespace DereTore.Applications.StarlightDirector.Conversion.Formats.Deleste {
    internal static class DelesteHelperExtensions {

        public static bool StartsWithOfGroup(this string str, string[] group) {
            return group.Any(str.StartsWith);
        }

    }
}
