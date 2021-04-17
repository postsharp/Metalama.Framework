// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<T> WhereNotNull<T>( this IEnumerable<T?> items )
            where T : class
            => items.Where( i => i != null )!;

        public static IReadOnlyList<T> ConcatNotNull<T>( this IReadOnlyList<T> a, T? b )
        {
            if ( b == null )
            {
                return a;
            }

            if ( a.Count == 0 )
            {
                // ReSharper disable once RedundantExplicitArrayCreation
                return new T[] { b! };
            }

            var l = new List<T>( a.Count + 1 );
            l.AddRange( a );
            l.Add( b );

            return l;
        }

        public static IReadOnlyList<T> Concat<T>( this IReadOnlyList<T> a, T b )
        {
            if ( a.Count == 0 )
            {
                return new[] { b };
            }

            var l = new List<T>( a.Count + 1 );
            l.AddRange( a );
            l.Add( b );

            return l;
        }

        public static IReadOnlyList<T> Concat<T>( this IReadOnlyList<T> a, IReadOnlyList<T>? b )
        {
            if ( b == null || b.Count == 0 )
            {
                return a;
            }

            if ( a.Count == 0 )
            {
                return b;
            }

            var l = new List<T>( a.Count + b.Count );
            l.AddRange( a );
            l.AddRange( b );

            return l;
        }
    }
}