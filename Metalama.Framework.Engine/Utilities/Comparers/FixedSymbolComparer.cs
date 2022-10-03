// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Utilities.Comparers;

/// <summary>
/// An implementation of <see cref="IEqualityComparer{T}"/> for <see cref="ISymbol"/> that considers equal two <see cref="IAssemblySymbol"/>
/// instances that have the same <see cref="AssemblyIdentity"/>.
/// </summary>
internal class FixedSymbolComparer : IEqualityComparer<ISymbol>
{
    private readonly IEqualityComparer<ISymbol> _underlying;

    public static IEqualityComparer<ISymbol> Default { get; } = new FixedSymbolComparer( SymbolEqualityComparer.Default );

    private FixedSymbolComparer( IEqualityComparer<ISymbol> underlying )
    {
        this._underlying = underlying;
    }

    public bool Equals( ISymbol? x, ISymbol? y )
    {
        if ( ReferenceEquals( x, y ) )
        {
            return true;
        }
        else if ( x == null || y == null )
        {
            return false;
        }

        if ( this._underlying.Equals( x, y ) )
        {
            return true;
        }

        if ( x is IAssemblySymbol xAssembly && y is IAssemblySymbol yAssembly && xAssembly.Identity.Equals( yAssembly.Identity ) )
        {
            return true;
        }

        return false;
    }

    public int GetHashCode( ISymbol obj )
        => obj switch
        {
            IAssemblySymbol assemblySymbol => assemblySymbol.Identity.GetHashCode(),
            _ => this._underlying.GetHashCode( obj )
        };
}