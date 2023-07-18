// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Engine.CodeModel.References
{
    /// <summary>
    /// An implementation of <see cref="IEqualityComparer{T}"/> that can compare implementations of <see cref="Ref{T}"/>.
    /// The comparison is compilation-independent.
    /// </summary>
    internal sealed class MemberRefEqualityComparer<T> : IEqualityComparer<MemberRef<T>>
        where T : class, IMemberOrNamedType
    {
        private readonly IEqualityComparer<ISymbol> _symbolEqualityComparer;

        internal MemberRefEqualityComparer( IEqualityComparer<ISymbol> symbolEqualityComparer )
        {
            this._symbolEqualityComparer = symbolEqualityComparer;
        }

        private static ISymbol? GetSymbol( MemberRef<T> reference ) => reference.Target as ISymbol;

        bool IEqualityComparer<MemberRef<T>>.Equals( MemberRef<T> x, MemberRef<T> y )
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

        int IEqualityComparer<MemberRef<T>>.GetHashCode( MemberRef<T> obj )
        {
            if ( obj.IsDefault )
            {
                return 0;
            }
            else
            {
                var xSymbol = GetSymbol( obj );

                var targetHashCode = xSymbol != null
                    ? this._symbolEqualityComparer.GetHashCode( xSymbol )
                    : RuntimeHelpers.GetHashCode( obj.Target );

                return targetHashCode;
            }
        }
    }
}