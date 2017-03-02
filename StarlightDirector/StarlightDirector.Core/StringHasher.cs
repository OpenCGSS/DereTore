using System;
using System.Security.Cryptography;
using System.Text;

namespace StarlightDirector {
    public static class StringHasher {

        public static string GetHash(string source, HashAlgorithm algorithm) {
            if (_latin1Encoding == null) {
                _latin1Encoding = Encoding.GetEncoding("ISO-8859-1");
            }
            var bytes = _latin1Encoding.GetBytes(source);
            bytes = algorithm.ComputeHash(bytes);
            var hashString = BitConverter.ToString(bytes);
            hashString = hashString.Replace("-", string.Empty).ToLowerInvariant();
            return hashString;
        }

        public static HashAlgorithm Md5 {
            get {
                if (_md5 == null) {
                    _md5 = new MD5Cng();
                    _md5.Initialize();
                }
                return _md5;
            }
        }

        public static HashAlgorithm Sha1 {
            get {
                if (_sha1 == null) {
                    _sha1 = new SHA1Cng();
                    _sha1.Initialize();
                }
                return _sha1;
            }
        }

        private static Encoding _latin1Encoding;
        private static HashAlgorithm _md5;
        private static HashAlgorithm _sha1;

    }
}
