// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Code.Collections
{
    internal class ReferenceEqualityComparer<T> : IEqualityComparer<T>
        where T : class
    {
        public static readonly ReferenceEqualityComparer<T> Instance = new();

        public bool Equals( T? x, T? y ) => ReferenceEquals( x, y );

        public int GetHashCode( T obj ) => RuntimeHelpers.GetHashCode( obj );
    }
}