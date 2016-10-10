using System;
using System.Collections.Generic;

namespace DereTore.Applications.StarlightDirector.Extensions {
    public static class ComparisonExtensions {

        public static ComparisonChain<T> Then<T>(this Comparison<T> comparison, Comparison<T> anotherComparison) {
            var t = new ComparisonChain<T>();
            t.Add(comparison);
            t.Add(anotherComparison);
            return t;
        }

        public static ComparisonChain<T> Then<T>(this ComparisonChain<T> comparisonChain, Comparison<T> comparison) {
            comparisonChain.Add(comparison);
            return comparisonChain;
        }

        public sealed class ComparisonChain<T> {

            public Comparison<T> Comparison => Compare;

            internal void Add(Comparison<T> comparison) {
                _comparisons.Add(comparison);
            }

            private int Compare(T x, T y) {
                var value = 0;
                foreach (var comparison in _comparisons) {
                    if (comparison != null) {
                        value = comparison(x, y);
                    }
                    if (value != 0) {
                        break;
                    }
                }
                return value;
            }

            private readonly List<Comparison<T>> _comparisons = new List<Comparison<T>>();

        }

    }
}
