using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.Utilities;

/// <summary>
/// Represents a dictionary key for an <see cref="ISymbol"/>. When created using the <see cref="Create"/> method, the key
/// does not hold a reference to the <see cref="ISymbol"/> itself, but only its <see cref="SymbolId"/>. The instance created
/// by this method is meant to be stored in the dictionary as the key, and to have a longer lifetime than the compilation.
/// When created using the <see cref="CreateLazy"/> method, the key holds a reference to the <see cref="ISymbol"/>. The comparison
/// with the <see cref="SymbolId"/> is done lazily, only in case where the hash codes match. This instance is meant to be
/// used for a dictionary lookup.
/// </summary>
public readonly struct SymbolKey : IEquatable<SymbolKey>
{
    private readonly int _hashCode;
    private readonly object _identity; // Can be a string (SymbolId) or an ISymbol.

    public SymbolKey( int hashCode, object identity )
    {
        this._hashCode = hashCode;
        this._identity = identity;
    }

    public bool Equals( SymbolKey other )
    {
        if ( this._hashCode != other._hashCode )
        {
            return false;
        }

        return this.GetId().Equals( other.GetId() );
    }

    internal static SymbolKey Create( ISymbol symbol )
        => new SymbolKey( StructuralSymbolComparer.Default.GetHashCode( symbol ), SymbolId.Create( symbol ).ToString() );
    
    public static SymbolKey CreateLazy( ISymbol symbol )
        => new SymbolKey( StructuralSymbolComparer.Default.GetHashCode( symbol ), symbol );

    internal SymbolId GetId()
        => this._identity switch
        {
            string s => new SymbolId( s ),
            ISymbol s => SymbolId.Create( s ),
            _ => throw new AssertionFailedException()
        };

    public override bool Equals( object? obj ) => obj is SymbolKey other && this.Equals( other );

    public override int GetHashCode() => this._hashCode;

    public static bool operator ==( SymbolKey left, SymbolKey right ) => left.Equals( right );

    public static bool operator !=( SymbolKey left, SymbolKey right ) => !left.Equals( right );
}