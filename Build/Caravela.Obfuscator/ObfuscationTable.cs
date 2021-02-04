using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Caravela.Obfuscator
{
    public class ObfuscationTable
    {
        private readonly SHA1 sha1 = SHA1.Create();
        private readonly Dictionary<string,string> hashToName = new Dictionary<string, string>(32*1024);
        private readonly Dictionary<string, string> nameToHash = new Dictionary<string, string>(32 * 1024);
        
        public ObfuscationTable()
        {
            
        }

        public int Count { get { return this.nameToHash.Count; } }
        public int ConflictCount { get; private set; }

        public string GetHash(string input)
        {
            string hashString;
            input = input.Normalize();
            if (!this.nameToHash.TryGetValue(input, out hashString))
            {
                throw new ArgumentException("No hash available for this value.");
            }

            return hashString;
        }

        public string CreateHash(string input)
        {
            input = input.Normalize();

            int hashLen;
            if (input.Length < 8)
                hashLen = 3;
            else if (input.Length < 32)
                hashLen = 6;
            else
                hashLen = 9;
              
            // Check if we already hash this string so we always return the same hash for the same string.
            string hashString;
            if ( this.nameToHash.TryGetValue( input, out hashString ))
            {
                return hashString;
            }
            
            byte[] inputBytes = Encoding.UTF8.GetBytes( input );
            bool hasConflict = false;
            do
            {
                byte[] hash = sha1.ComputeHash(inputBytes);
                hashString = "^" + Convert.ToBase64String(hash, 0, hashLen);

                string existingName;
                if ( this.hashToName.TryGetValue( hashString, out existingName ) && existingName != input)
                {
                    // We have a hash conflict. There is no good way to solve it, so we simply add some
                    // random.
                    
                    byte[] randomBytes = new byte[1];
                    RandomNumberGenerator.Create().GetBytes( randomBytes );
                    inputBytes[0] = randomBytes[0];
                    hasConflict = true;
                }
                else
                {
                    this.hashToName.Add( hashString, input );
                    this.nameToHash.Add( input, hashString );
                    if (hasConflict)
                    {
                        this.ConflictCount++;
                    }
                    return hashString;
                }
                
            } while ( true );
            
        }

    
        public void Write(TextWriter writer)
        {
            foreach ( KeyValuePair<string, string> pair in this.nameToHash )
            {
                writer.WriteLine("{0}: {1}", XmlConvert.EncodeName( pair.Key ), pair.Value);
            }
        }

    }
}
