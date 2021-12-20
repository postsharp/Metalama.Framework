// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Engine.DesignTime.Pipeline;

public readonly struct DesignTimeValidatorCollectionEqualityKey : IEquatable<DesignTimeValidatorCollectionEqualityKey>
{
    private readonly ulong _hashCode;

    internal static DesignTimeValidatorCollectionEqualityKey Empty { get; } = new( 0 ); 

    internal DesignTimeValidatorCollectionEqualityKey( ulong hashCode )
    {
        this._hashCode = hashCode;
    }

    public bool Equals( DesignTimeValidatorCollectionEqualityKey other ) => this._hashCode == other._hashCode;

    public override bool Equals( object? obj ) => obj is DesignTimeValidatorCollectionEqualityKey other && other._hashCode == this._hashCode;

    public override int GetHashCode() => (int) this._hashCode;

    public static bool operator ==( DesignTimeValidatorCollectionEqualityKey left, DesignTimeValidatorCollectionEqualityKey right ) => Equals( left, right );

    public static bool operator !=( DesignTimeValidatorCollectionEqualityKey left, DesignTimeValidatorCollectionEqualityKey right ) => !Equals( left, right );
}