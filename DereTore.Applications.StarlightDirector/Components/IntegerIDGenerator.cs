namespace DereTore.Applications.StarlightDirector.Components {
    public sealed class IntegerIDGenerator {

        public int Next() {
            return _current++;
        }

        public void Reset() {
            _current = _start;
        }

        public int Current => _current;

        internal IntegerIDGenerator()
            : this(0) {
        }

        internal IntegerIDGenerator(int start) {
            _start = _current = start;
        }

        private int _current;
        private readonly int _start;

    }
}
