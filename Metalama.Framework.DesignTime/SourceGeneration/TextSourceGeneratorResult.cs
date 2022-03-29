// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using K4os.Hash.xxHash;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime;

/// <summary>
/// An implementation of <see cref="SourceGeneratorResult"/> backed by strings. It is used in the VS user process,
/// because the strings come deserialized from the analysis process.
/// </summary>
public sealed class TextSourceGeneratorResult : SourceGeneratorResult
{
    public ImmutableDictionary<string, string> Sources { get; }

    public TextSourceGeneratorResult( ImmutableDictionary<string, string> sources )
    {
        this.Sources = sources;
    }

    protected override ulong ComputeHashCodeImpl()
    {
        var xxh = new XXH64();
        ulong hash = 0;

        foreach ( var source in this.Sources )
        {
            xxh.Reset();
            xxh.Update( source.Key );
            xxh.Update( source.Value );

            hash ^= xxh.Digest();
        }

        return hash;
    }

    public override void ProduceContent( SourceProductionContext context )
    {
        foreach ( var source in this.Sources )
        {
            context.AddSource( source.Key, source.Value );
        }
    }

    public override string ToString() => $"{nameof(TextSourceGeneratorResult)} Count={this.Sources.Count}";
}