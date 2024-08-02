// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities.Comparers;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Engine.CodeModel.References
{
    internal sealed class RefEqualityComparer : IEqualityComparer<IRef<ICompilationElement>>
    {
        public static readonly RefEqualityComparer Default = new( StructuralSymbolComparer.IncludeAssembly, StructuralDeclarationComparer.IncludeAssembly );

        public static readonly RefEqualityComparer IncludeNullability = new(
            StructuralSymbolComparer.IncludeAssemblyAndNullability,
            StructuralDeclarationComparer.IncludeAssemblyAndNullability );

        public static RefEqualityComparer GetInstance( bool includeNullability ) => includeNullability ? IncludeNullability : Default;

        private RefEqualityComparer( StructuralSymbolComparer symbolEqualityComparer, StructuralDeclarationComparer declarationEqualityComparer )
        {
            this.StructuralSymbolComparer = symbolEqualityComparer;
            this.StructuralDeclarationComparer = declarationEqualityComparer;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public StructuralSymbolComparer StructuralSymbolComparer { get; }

        public StructuralDeclarationComparer StructuralDeclarationComparer { get; }

        private static ISymbol? GetSymbol( IRefImpl reference ) => reference.Target as ISymbol;

        public bool Equals( IRef<ICompilationElement>? x, IRef<ICompilationElement>? y )
        {
            if ( ReferenceEquals( x, y ) )
            {
                return true;
            }

            if ( x == null || y == null )
            {
                return false;
            }

            var xImpl = (IRefImpl) x;
            var yImpl = (IRefImpl) y;

            if ( xImpl.TargetKind != yImpl.TargetKind )
            {
                return false;
            }

            var xSymbol = GetSymbol( xImpl );

            if ( xSymbol != null )
            {
                return this.StructuralSymbolComparer.Equals( xSymbol, GetSymbol( yImpl ) );
            }
            else
            {
                return ReferenceEquals( xImpl.Target, yImpl.Target );
            }
        }

        public int GetHashCode( IRef<ICompilationElement> obj )
        {
            var impl = (IRefImpl) obj;

            if ( impl.IsDefault )
            {
                return 0;
            }
            else
            {
                var xSymbol = GetSymbol( impl );

                var targetHashCode = xSymbol != null
                    ? this.StructuralSymbolComparer.GetHashCode( xSymbol )
                    : RuntimeHelpers.GetHashCode( impl.Target );

                // PERF: Cast enum to int otherwise it will be boxed on .NET Framework.
                return HashCode.Combine( targetHashCode, (int) impl.TargetKind );
            }
        }

        bool IEqualityComparer<IRef<ICompilationElement>>.Equals( IRef<ICompilationElement>? x, IRef<ICompilationElement>? y ) => this.Equals( x, y );

        int IEqualityComparer<IRef<ICompilationElement>>.GetHashCode( IRef<ICompilationElement> obj ) => this.GetHashCode( obj );
    }
}