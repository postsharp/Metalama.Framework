﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using K4os.Hash.xxHash;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;

namespace Metalama.Framework.Engine.Utilities
{
    public static class HashUtilities
    {
        public static string HashString( string s ) => XXH64.DigestOf( Encoding.UTF8.GetBytes( s ) ).ToString( "x16", CultureInfo.InvariantCulture );

        public static void Update( this XXH64 hash, string value ) => hash.Update( Encoding.UTF8.GetBytes( value ) );

        public static unsafe void Update<T>( this XXH64 hash, T value )
            where T : unmanaged
            => hash.Update( (byte*) &value, sizeof(T) );

        public static unsafe void Update( this XXH64 hash, long value ) => hash.Update( (byte*) &value, sizeof(long) );

        public static unsafe void Update( this XXH64 hash, ulong value ) => hash.Update( (byte*) &value, sizeof( ulong ) );

        // The following overloads are redundant but they work around a compiler bug.
        public static void Update( this XXH64 hash, ImmutableArray<byte> bytes ) => hash.Update( bytes.AsSpan() );

        public static unsafe void Update( this XXH64 hash, int value ) => hash.Update( (byte*) &value, sizeof(int) );
    }
}