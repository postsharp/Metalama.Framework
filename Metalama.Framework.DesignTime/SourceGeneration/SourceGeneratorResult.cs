// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Pipeline;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime;

/// <summary>
/// A cacheable (and therefore equatable) result of source generators. Returned by <see cref="ProjectHandler.GenerateSources"/>.
/// </summary>
public abstract class SourceGeneratorResult : IEquatable<SourceGeneratorResult>
{
    /// <summary>
    /// Gets an empty <see cref="SourceGeneratorResult"/>.
    /// </summary>
    public static SourceGeneratorResult Empty { get; } = new SyntaxTreeSourceGeneratorResult( ImmutableDictionary<string, IntroducedSyntaxTree>.Empty );

    private ulong _hash;

    public bool Equals( SourceGeneratorResult? other )
    {
        if ( ReferenceEquals( null, other ) )
        {
            return false;
        }

        if ( ReferenceEquals( this, other ) )
        {
            return true;
        }

        return this.ComputeHashCode() == other.ComputeHashCode();
    }

    private ulong ComputeHashCode()
    {
        if ( this._hash == 0 )
        {
            this._hash = this.ComputeHashCodeImpl();
        }

        return this._hash;
    }

    protected abstract ulong ComputeHashCodeImpl();

    public override int GetHashCode()
    {
        unchecked
        {
            return (int) this.ComputeHashCode();
        }
    }

    /// <summary>
    /// Adds the content represented by the current object to a <see cref="SourceProductionContext"/>.
    /// </summary>
    public abstract void ProduceContent( SourceProductionContext context );
}