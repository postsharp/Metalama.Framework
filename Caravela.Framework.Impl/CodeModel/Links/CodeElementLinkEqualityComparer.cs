using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel.Links
{
    /// <summary>
    /// An implementation of <see cref="IEqualityComparer{T}"/> that can compare implementations of <see cref="ICodeElementLink"/>.
    /// The comparison is compilation-independent.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CodeElementLinkEqualityComparer<T> : IEqualityComparer<T>
        where T : ICodeElementLink
    {
        public static readonly CodeElementLinkEqualityComparer<T> Instance = new CodeElementLinkEqualityComparer<T>();

        private static ISymbol? GetSymbol( T link ) => link.Target as ISymbol;

        public bool Equals( T x, T y )
        {
            var xSymbol = GetSymbol( x );
            if ( xSymbol != null )
            {
                return SymbolEqualityComparer.Default.Equals( xSymbol, GetSymbol( y ) );
            }
            else
            {
                return ReferenceEquals( x.Target, y.Target );
            }
        }

        public int GetHashCode( T? obj )
        {
            if ( obj == null )
            {
                return 0;
            }
            else
            {
                var xSymbol = GetSymbol( obj );
                if ( xSymbol != null )
                {
                    return SymbolEqualityComparer.Default.GetHashCode( xSymbol );
                }
                else
                {
                    return RuntimeHelpers.GetHashCode( obj.Target );
                }
            }
        }
    }
}
