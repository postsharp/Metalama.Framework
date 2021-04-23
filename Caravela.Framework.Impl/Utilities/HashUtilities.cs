// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using K4os.Hash.xxHash;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Caravela.Framework.Impl.Utilities
{
    public static class HashUtilities
    {
        public static string HashString( string s ) => XXH64.DigestOf( Encoding.UTF8.GetBytes( s ) ).ToString("x16");

        public static void Update( this XXH64 hash, string s ) => hash.Update( Encoding.UTF8.GetBytes( s ) );
    }
    
}