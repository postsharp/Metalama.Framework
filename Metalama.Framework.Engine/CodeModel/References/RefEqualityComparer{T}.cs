// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Engine.CodeModel.References
{
    /// <summary>
    /// An implementation of <see cref="IEqualityComparer{T}"/> that can compare implementations of <see cref="Ref{T}"/>.
    /// The comparison is compilation-independent.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class RefEqualityComparer<T> : IEqualityComparer<Ref<T>>
        where T : class, ICompilationElement
    {
        public static readonly RefEqualityComparer<T> Default = new( SymbolEqualityComparer.Default );
        public static readonly RefEqualityComparer<T> IncludeNullability = new( SymbolEqualityComparer.IncludeNullability );

        private readonly SymbolEqualityComparer _symbolEqualityComparer;

        private RefEqualityComparer( SymbolEqualityComparer symbolEqualityComparer )
        {
            this._symbolEqualityComparer = symbolEqualityComparer;
        }

        private static ISymbol? GetSymbol( Ref<T> reference ) => reference.Target as ISymbol;

        public bool Equals( in Ref<T> x, Ref<T> y )
        {
            if ( x.TargetKind != y.TargetKind )
            {
                return false;
            }

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

        public int GetHashCode( in Ref<T> obj )
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

                return HashCode.Combine( targetHashCode, obj.TargetKind );
            }
        }

        bool IEqualityComparer<Ref<T>>.Equals( Ref<T> x, Ref<T> y ) => this.Equals( x, y );

        int IEqualityComparer<Ref<T>>.GetHashCode( Ref<T> obj ) => this.GetHashCode( obj );
    }
}