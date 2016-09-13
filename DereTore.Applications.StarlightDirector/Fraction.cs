namespace DereTore.Applications.StarlightDirector {
    public struct Fraction {

        public Fraction(int numerator, int denominator) {
            Numerator = numerator;
            Denominator = denominator;
        }

        public int Numerator { get; set; }

        public int Denominator { get; set; }

        public double GetValue() {
            if (Denominator == 0) {
                return Numerator >= 0 ? double.PositiveInfinity : double.NegativeInfinity;
            } else {
                return Numerator / (double)Denominator;
            }
        }

    }
}
