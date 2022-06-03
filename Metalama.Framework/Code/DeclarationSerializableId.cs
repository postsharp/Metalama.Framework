// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Code;

public readonly struct DeclarationSerializableId : IEquatable<DeclarationSerializableId>
{
    internal string Id { get; }

    // Intentionally public because this is used in the Workspace project where we need to pass the id as a string.
    public DeclarationSerializableId( string id )
    {
        this.Id = id;
    }

    public bool Equals( DeclarationSerializableId other ) => string.Equals( this.Id, other.Id, StringComparison.Ordinal );

    public override bool Equals( object? obj ) => obj is DeclarationSerializableId other && this.Equals( other );

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode( this.Id );

    public static bool operator ==( DeclarationSerializableId left, DeclarationSerializableId right ) => left.Equals( right );

    public static bool operator !=( DeclarationSerializableId left, DeclarationSerializableId right ) => !left.Equals( right );

    public override string ToString() => this.Id;
}