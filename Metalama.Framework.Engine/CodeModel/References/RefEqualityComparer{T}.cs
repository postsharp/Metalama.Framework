// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities.Comparers;
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
    internal sealed class RefEqualityComparer<T> : IEqualityComparer<Ref<T>>
        where T : class, ICompilationElement
    {
        public static readonly RefEqualityComparer<T> Default = new( StructuralSymbolComparer.IncludeAssembly );
        public static readonly RefEqualityComparer<T> IncludeNullability = new( StructuralSymbolComparer.IncludeAssemblyAndNullability );

        private RefEqualityComparer( StructuralSymbolComparer symbolEqualityComparer )
        {
            this.StructuralSymbolComparer = symbolEqualityComparer;
        }

        public StructuralSymbolComparer StructuralSymbolComparer { get; }

        private static ISymbol? GetSymbol( in Ref<T> reference ) => reference.Target as ISymbol;

        public bool Equals( Ref<T> x, Ref<T> y )
        {
            if ( x.TargetKind != y.TargetKind )
            {
                return false;
            }

            var xSymbol = GetSymbol( x );

            if ( xSymbol != null )
            {
                return this.StructuralSymbolComparer.Equals( xSymbol, GetSymbol( y ) );
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
                    ? this.StructuralSymbolComparer.GetHashCode( xSymbol )
                    : RuntimeHelpers.GetHashCode( obj.Target );

                return HashCode.Combine( targetHashCode, obj.TargetKind );
            }
        }

        bool IEqualityComparer<Ref<T>>.Equals( Ref<T> x, Ref<T> y ) => this.Equals( x, y );

        int IEqualityComparer<Ref<T>>.GetHashCode( Ref<T> obj ) => this.GetHashCode( obj );
    }
}