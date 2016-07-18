namespace DereTore.HCA {
    internal static class ErrorMessages {

        public static string GetBufferTooSmall(int minimum, int actual) {
            return string.Format("Buffer too small. Required minimum: {0}, actual: {1}", minimum, actual);
        }

        public static string GetInvalidParameter(string paramName) {
            return string.Format("Parameter '{0}' is invalid.", paramName);
        }

        public static string GetChecksumNotMatch(int expected, int actual) {
            return string.Format("Checksum does not match. Expected: {0}({1}), actual: {2}({3}).", expected, expected.ToString("x8"), actual, actual.ToString("x8"));
        }

        public static string GetMagicNotMatch(int expected, int actual) {
            return string.Format("Magic does not match. Expected: {0}({1}), actual: {2}({3}).", expected, expected.ToString("x8"), actual, actual.ToString("x8"));
        }

        public static string GetAthInitializationFailed() {
            return "ATH table initialization failed.";
        }

        public static string GetCiphInitializationFailed() {
            return "CIPH table initialization failed.";
        }

    }
}
