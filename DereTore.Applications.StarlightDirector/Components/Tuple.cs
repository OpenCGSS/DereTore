using System.Collections.Generic;

namespace DereTore.Applications.StarlightDirector.Components {
    public struct Tuple<T1, T2> {

        public static Tuple<T1, T2> Create(T1 v1, T2 v2) {
            var t = new Tuple<T1, T2> {
                Value1 = v1,
                Value2 = v2
            };
            return t;
        }

        public T1 Value1 { get; private set; }
        public T2 Value2 { get; private set; }

        public override bool Equals(object obj) {
            if (!(obj is Tuple<T1, T2>)) {
                return base.Equals(obj);
            }
            return Equals((Tuple<T1, T2>)obj);
        }

        public override int GetHashCode() {
            var hash1 = Value1 != null ? Value1.GetHashCode() : 0;
            var hash2 = Value2 != null ? Value2.GetHashCode() : 0;
            return hash1 ^ hash2;
        }

        public bool Equals(Tuple<T1, T2> other) {
            return EqualityComparer<T1>.Default.Equals(Value1, other.Value1) && EqualityComparer<T2>.Default.Equals(Value2, other.Value2);
        }

        public static bool operator ==(Tuple<T1, T2> left, Tuple<T1, T2> right) {
            return left.Equals(right);
        }

        public static bool operator !=(Tuple<T1, T2> left, Tuple<T1, T2> right) {
            return !left.Equals(right);
        }

    }
}
