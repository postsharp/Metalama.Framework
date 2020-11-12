using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Caravela.Patterns.Costura
{
    public static class ResourceHash
    {
        public static string CalculateHash(List<(string Name, Stream Stream)> resources)
        {
            var data = resources
                .OrderBy(r => r.Name)
                .Where(r => r.Name.StartsWith("costura"))
                .Select(r => r.Stream)
                .ToArray();
            ConcatenatedStream allStream = new ConcatenatedStream(data);
            
            using (var md5 = MD5.Create())
            {
                var hashBytes = md5.ComputeHash(allStream);
            
                var sb = new StringBuilder();
                for (var i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                
                allStream.ResetAllToZero();
                return sb.ToString();
            }
        }
    }
}