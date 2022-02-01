// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using K4os.Hash.xxHash;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;

namespace Metalama.Framework.Engine.Utilities
{
    public static class HashUtilities
    {
        public static string HashString( string s ) => XXH64.DigestOf( Encoding.UTF8.GetBytes( s ) ).ToString( "x16", CultureInfo.InvariantCulture );

        public static void Update( this XXH64 hash, string s ) => hash.Update( Encoding.UTF8.GetBytes( s ) );

        public static unsafe void Update<T>( this XXH64 hash, T value )
            where T : unmanaged
            => hash.Update( (byte*) &value, sizeof(T) );

        public static void Update( this XXH64 hash, ImmutableArray<byte> bytes ) => hash.Update( bytes.AsSpan() );
    }
}