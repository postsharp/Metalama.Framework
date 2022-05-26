// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Engine.CodeModel.References
{
    /// <summary>
    /// An implementation of <see cref="IEqualityComparer{T}"/> that can compare implementations of <see cref="IRefImpl"/>.
    /// The comparison is compilation-independent.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DeclarationRefEqualityComparer<T> : IEqualityComparer<T>
        where T : IRefImpl
    {
        public static readonly DeclarationRefEqualityComparer<T> Default = new( SymbolEqualityComparer.Default );
        public static readonly DeclarationRefEqualityComparer<T> IncludeNullability = new( SymbolEqualityComparer.IncludeNullability );

        private readonly SymbolEqualityComparer _symbolEqualityComparer;

        private DeclarationRefEqualityComparer( SymbolEqualityComparer symbolEqualityComparer )
        {
            this._symbolEqualityComparer = symbolEqualityComparer;
        }

        private static ISymbol? GetSymbol( T reference ) => reference.Target as ISymbol;

        public bool Equals( T x, T y )
        {
            var xSymbol = GetSymbol( x );

            if ( xSymbol != null )
            {
                return this._symbolEqualityComparer.Equals( xSymbol, GetSymbol( y ) );
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