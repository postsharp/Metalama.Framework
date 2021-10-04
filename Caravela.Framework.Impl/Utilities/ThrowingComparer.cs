// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace Caravela.Framework.Impl.Utilities
{
    /// <summary>
    /// An implementation of <see cref="IComparer{T}"/> that throws an exception if the two members of the equality
    /// are not the same instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class ThrowingComparer<T> : IComparer<T>
        where T : class
    {
        public static readonly ThrowingComparer<T> Instance = new();

        private ThrowingComparer() { }

        public int Compare( T x, T y ) => ReferenceEquals( x, y ) ? 0 : throw new AssertionFailedException( $"'{x}' and '{y}' are not strongly ordered." );
    }
}