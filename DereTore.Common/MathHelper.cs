using System;

namespace DereTore {
    public static class MathHelper {

        static MathHelper() {
            Random = new Random();
        }

        public static double ClampUpper(double value, double minimum) {
            return value < minimum ? minimum : value;
        }

        public static double ClampLower(double value, double maximum) {
            return value > maximum ? maximum : value;
        }

        public static double Clamp(double value, double minimum, double maximum) {
            return value < minimum ? minimum : (value > maximum ? maximum : value);
        }

        public static int NextRandomInt32() {
            return Random.Next();
        }

        public static int NextRandomInt32(int maxValue) {
            return Random.Next(maxValue);
        }

        public static int NextRandomInt32(int minValue, int maxValue) {
            return Random.Next(minValue, maxValue);
        }

        public static float NextRandomSingle() {
            return (float)Random.NextDouble();
        }

        public static double NextRandomDouble() {
            return Random.NextDouble();
        }

        private static readonly Random Random;

    }
}
