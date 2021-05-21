// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Caravela.Framework.Impl.CodeModel.References
{
    /// <summary>
    /// An implementation of <see cref="IEqualityComparer{T}"/> that can compare implementations of <see cref="IDeclarationRef"/>.
    /// The comparison is compilation-independent.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DeclarationRefEqualityComparer<T> : IEqualityComparer<T>
        where T : IDeclarationRef
    {
        public static readonly DeclarationRefEqualityComparer<T> Instance = new();

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