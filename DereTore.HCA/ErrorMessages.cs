namespace DereTore.HCA {
    internal static class ErrorMessages {

        public static string GetBufferTooSmall(int minimum, int actual) {
            return $"Buffer too small. Required minimum: {minimum}, actual: {actual}";
        }

        public static string GetInvalidParameter(string paramName) {
            return $"Parameter '{paramName}' is invalid.";
        }

        public static string GetChecksumNotMatch(int expected, int actual) {
            return $"Checksum does not match. Expected: {expected}({expected.ToString("x8")}), actual: {actual}({actual.ToString("x8")}).";
        }

        public static string GetMagicNotMatch(int expected, int actual) {
            return $"Magic does not match. Expected: {expected}({expected.ToString("x8")}), actual: {actual}({actual.ToString("x8")}).";
        }

        public static string GetAthInitializationFailed() {
            return "ATH table initialization failed.";
        }

        public static string GetCiphInitializationFailed() {
            return "CIPH table initialization failed.";
        }

    }
}
