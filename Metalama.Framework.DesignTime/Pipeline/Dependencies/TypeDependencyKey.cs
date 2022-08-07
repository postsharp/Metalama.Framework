// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Globalization;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

/// <summary>
/// A weak equality key to identify types, where some collisions are acceptable.
/// </summary>
internal readonly struct TypeDependencyKey : IEquatable<TypeDependencyKey>
{
    private readonly int _hashCode;
    private readonly string? _text;

    public TypeDependencyKey( ITypeSymbol type, bool storeTypeName )
    {
        var hashCode = default(HashCode);

        for ( var d = (ISymbol) type; d is { } and not IAssemblySymbol and not IModuleSymbol; d = d.ContainingSymbol )
        {
            hashCode.Add( d.Name );
        }

        this._hashCode = hashCode.ToHashCode();

        if ( storeTypeName )
        {
            this._text = type.ToString();
        }
        else
        {
            this._text = null;
        }
    }

    // For test only.
    public TypeDependencyKey( string name )
    {
        this._hashCode = name.GetHashCode();
        this._text = name;
    }

    public bool Equals( TypeDependencyKey other ) => this._hashCode == other._hashCode;

    public override bool Equals( object? obj ) => obj is TypeDependencyKey other && this.Equals( other );

    public override int GetHashCode() => this._hashCode;

    public static bool operator ==( TypeDependencyKey left, TypeDependencyKey right ) => left.Equals( right );

    public static bool operator !=( TypeDependencyKey left, TypeDependencyKey right ) => !left.Equals( right );

    public override string ToString() => this._text ?? this._hashCode.ToString(CultureInfo.InvariantCulture);
}