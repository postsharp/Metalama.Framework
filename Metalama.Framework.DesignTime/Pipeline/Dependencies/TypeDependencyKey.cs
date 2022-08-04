// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

/// <summary>
/// A weak equality key to identify types, where some collisions are acceptable.
/// </summary>
internal readonly struct TypeDependencyKey : IEquatable<TypeDependencyKey>
{
    private readonly int _hashCode;

#if DEBUG
    private readonly string _text;
#endif

    public TypeDependencyKey( ITypeSymbol type )
    {
        var hashCode = new HashCode();

        for ( var d = (ISymbol) type; d != null && d is not IAssemblySymbol and not IModuleSymbol; d = d.ContainingSymbol )
        {
            hashCode.Add( d.Name );
        }

        this._hashCode = hashCode.ToHashCode();

#if DEBUG
        this._text = type.ToString();
#endif
    }

    public bool Equals( TypeDependencyKey other ) => this._hashCode == other._hashCode;

    public override bool Equals( object? obj ) => obj is TypeDependencyKey other && this.Equals( other );

    public override int GetHashCode() => this._hashCode;

    public static bool operator ==( TypeDependencyKey left, TypeDependencyKey right ) => left.Equals( right );

    public static bool operator !=( TypeDependencyKey left, TypeDependencyKey right ) => !left.Equals( right );

#if DEBUG
    public override string ToString() => this._text;
#endif
}