using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Caravela.Patterns.Costura
{
    public class Checksums
    {
        readonly Dictionary<string, string> checksums = new Dictionary<string, string>();

        public IReadOnlyDictionary<string, string> AllChecksums => checksums;
        public static string CalculateChecksum(string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return CalculateChecksum(fs);
            }
        }

        public static string CalculateChecksum(Stream stream)
        {
            using (var bs = new BufferedStream(stream))
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                var hash = sha1.ComputeHash(bs);
                var formatted = new StringBuilder(2 * hash.Length);
                foreach (var b in hash)
                {
                    formatted.AppendFormat("{0:X2}", b);
                }

                return formatted.ToString();
            }
        }

        public void Add(string resourceName, string checksum)
        {
            this.checksums.Add(resourceName, checksum);
        }

        public bool ContainsKey(string resourceName)
        {
            return checksums.ContainsKey(resourceName);
        }
    }
}