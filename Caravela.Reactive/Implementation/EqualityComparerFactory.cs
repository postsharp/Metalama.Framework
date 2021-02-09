using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Caravela.Reactive.Implementation
{
    public static class EqualityComparerFactory
    {
        public static IEqualityComparer<T> GetEqualityComparer<T>()
        {
            if ( typeof( T ).IsValueType )
            {
                return EqualityComparer<T>.Default;
            }
            else
            {
                return ReferenceEqualityComparer<T>.Instance;
            }
        }

        private class ReferenceEqualityComparer<T> : IEqualityComparer<T>
        {
            public static readonly ReferenceEqualityComparer<T> Instance = new ReferenceEqualityComparer<T>();

            public bool Equals( T x, T y )
            {
                return ReferenceEquals( x, y );
            }

            public int GetHashCode( T obj )
            {
                return RuntimeHelpers.GetHashCode( obj );
            }
        }
    }
}