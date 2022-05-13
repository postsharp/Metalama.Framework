// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Pipeline;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.SourceGeneration;

/// <summary>
/// A cacheable (and therefore equatable) result of source generators. Returned by <see cref="ProjectHandler.GenerateSources"/>.
/// </summary>
public abstract class SourceGeneratorResult : IEquatable<SourceGeneratorResult>
{
    /// <summary>
    /// Gets an empty <see cref="SourceGeneratorResult"/>.
    /// </summary>
    public static SourceGeneratorResult Empty { get; } = new SyntaxTreeSourceGeneratorResult( ImmutableDictionary<string, IntroducedSyntaxTree>.Empty );

    private ulong _digest;

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

        return this.GetDigest() == other.GetDigest();
    }

    public ulong GetDigest()
    {
        if ( this._digest == 0 )
        {
            this._digest = this.ComputeDigest();
        }

        return this._digest;
    }

    protected abstract ulong ComputeDigest();

    public override int GetHashCode()
    {
        unchecked
        {
            return (int) this.GetDigest();
        }
    }

    /// <summary>
    /// Adds the content represented by the current object to a <see cref="SourceProductionContext"/>.
    /// </summary>
    public abstract void ProduceContent( SourceProductionContext context );
}