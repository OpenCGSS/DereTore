using System;
using System.Collections.Generic;

namespace DereTore.Applications.StarlightDirector.Components {
    public sealed class ObjectMatrix<T> {

        public void Add(int x, int y, T value) {
            var key = MakeKey(x, y);
            _matrix.Add(key, value);
        }

        public bool Remove(int x, int y) {
            var key = MakeKey(x, y);
            return _matrix.Remove(key);
        }

        public void Clear() {
            _matrix.Clear();
        }

        public bool ContainsKey(int x, int y) {
            var key = MakeKey(x, y);
            return _matrix.ContainsKey(key);
        }

        public T this[int x, int y] {
            get {
                var key = MakeKey(x, y);
                return _matrix[key];
            }
            set {
                var key = MakeKey(x, y);
                _matrix[key] = value;
            }
        }

        public bool TryGetValue(int x, int y, out T value) {
            var key = MakeKey(x, y);
            return _matrix.TryGetValue(key, out value);
        }

        private readonly Dictionary<Tuple<int, int>, T> _matrix = new Dictionary<Tuple<int, int>, T>();

        private static Tuple<int, int> MakeKey(int x, int y) {
            return new Tuple<int, int>(x, y);
        }

    }
}
