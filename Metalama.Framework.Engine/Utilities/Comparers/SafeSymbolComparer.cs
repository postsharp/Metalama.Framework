// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Utilities.Comparers;

public sealed class SafeSymbolComparer : IEqualityComparer<ISymbol>
{
    private readonly SymbolEqualityComparer _underlying;
    private readonly CompilationContext _compilationContext;

    public SafeSymbolComparer( CompilationContext compilationContext, SymbolEqualityComparer? underlying = null )
    {
        this._underlying = underlying ?? SymbolEqualityComparer.Default;
        this._compilationContext = compilationContext;
    }

    public bool Equals( ISymbol? x, ISymbol? y )
    {
        if ( ReferenceEquals( x, y ) )
        {
            return true;
        }

        if ( x == null || y == null )
        {
            return false;
        }

#if DEBUG
        if ( x.BelongsToCompilation( this._compilationContext ) == false )
        {
            throw new AssertionFailedException( $"The symbol '{x}' does not belong to the current compilation." );
        }

        if ( y.BelongsToCompilation( this._compilationContext ) == false )
        {
            throw new AssertionFailedException( $"The symbol '{y}' does not belong to the current compilation." );
        }
#endif
        return this._underlying.Equals( x, y );
    }

    public int GetHashCode( ISymbol obj ) => this._underlying.GetHashCode( obj );

    internal bool IsMemberOf( ISymbol member, INamedTypeSymbol type )
    {
        if ( member.ContainingType == null )
        {
            return false;
        }

        member.ThrowIfBelongsToDifferentCompilationThan( type );

        if ( SymbolEqualityComparer.Default.Equals( member.ContainingType, type ) )
        {
            return true;
        }

        if ( type.BaseType != null )
        {
            return this.IsMemberOf( member, type.BaseType );
        }

        return false;
    }

    internal bool Is( ITypeSymbol left, ITypeSymbol right )
    {
        if ( left is IErrorTypeSymbol )
        {
            return false;
        }

        left.ThrowIfBelongsToDifferentCompilationThan( right );

        if ( SymbolEqualityComparer.Default.Equals( left, right ) )
        {
            return true;
        }
        else if ( left.BaseType != null && this.Is( left.BaseType, right ) )
        {
            return true;
        }
        else
        {
            foreach ( var i in left.Interfaces )
            {
                if ( this.Is( i, right ) )
                {
                    return true;
                }
            }

            return false;
        }
    }
}