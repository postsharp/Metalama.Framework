using System.Collections.Generic;

namespace Caravela.Framework.Impl
{

    internal static class EnumerableExtensions
    {
        public static IReadOnlyList<T> ConcatNotNull<T>( this IReadOnlyList<T> a, T? b )
        {
            if ( b == null )
            {
                return a;
            }
            else if ( a.Count == 0 )
            {
                return new[] { b };
            }
            else
            {
                var l = new List<T>( a.Count + 1 );
                l.AddRange( a );
                l.Add( b );
                return l;
            }
        }
        
        public static IReadOnlyList<T> Concat<T>( this IReadOnlyList<T> a, T b )
        {
            if ( a.Count == 0 )
            {
                return new[] { b };
            }
            else
            {
                var l = new List<T>( a.Count + 1 );
                l.AddRange( a );
                l.Add( b );
                return l;
            }
        }

        public static IReadOnlyList<T> Concat<T>( this IReadOnlyList<T> a, IReadOnlyList<T>? b )
        {
            if ( b == null || b.Count == 0 )
            {
                return a;
            }
            else if ( a.Count == 0 )
            {
                return b;
            }
            else
            {
                var l = new List<T>( a.Count + b.Count );
                l.AddRange( a );
                l.AddRange( b );
                return l;
            }
        }
    }
}
