using System;
using System.Collections.Generic;
using System.Linq;

namespace DereTore {
    public static class EnumerableExtensions {

        public static int FirstIndexOf<T>(this IEnumerable<T> enumerable, Predicate<T> predicate, int startIndex) {
            var i = startIndex;
            var newEnumerable = startIndex == 0 ? enumerable : enumerable.Skip(startIndex);
            foreach (var obj in newEnumerable) {
                if (predicate(obj)) {
                    return i;
                }
                ++i;
            }
            return -1;
        }

        public static int FirstIndexOf<T>(this IEnumerable<T> enumerable, Predicate<T> predicate) {
            return FirstIndexOf(enumerable, predicate, 0);
        }

    }
}
