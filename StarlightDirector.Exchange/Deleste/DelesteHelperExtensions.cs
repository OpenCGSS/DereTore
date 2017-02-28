using System.Linq;

namespace StarlightDirector.Exchange.Deleste {
    internal static class DelesteHelperExtensions {

        public static bool StartsWithOfGroup(this string str, string[] group) {
            return group.Any(str.StartsWith);
        }

    }
}
