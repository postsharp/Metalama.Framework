// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Caravela.Framework.Impl.Utilities
{
    public static class HashUtilities
    {
        public static string HashString( string s )
            => string.Join( "", SHA256.Create().ComputeHash( Encoding.UTF8.GetBytes( s ) ).Select( b => b.ToString( "x2" ) ) );
    }
}