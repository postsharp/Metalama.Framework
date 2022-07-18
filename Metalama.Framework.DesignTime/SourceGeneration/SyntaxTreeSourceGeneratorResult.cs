// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using K4os.Hash.xxHash;
using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
#if DEBUG
using Metalama.Framework.Engine;
#endif

namespace Metalama.Framework.DesignTime.SourceGeneration;

/// <summary>
/// An implementation of <see cref="SourceGeneratorResult"/> backed by a set of <see cref="SyntaxTree"/>.
/// </summary>
public sealed class SyntaxTreeSourceGeneratorResult : SourceGeneratorResult
{
    public ImmutableDictionary<string, IntroducedSyntaxTree> AdditionalSources { get; }

    public SyntaxTreeSourceGeneratorResult( ImmutableDictionary<string, IntroducedSyntaxTree> additionalSources )
    {
        this.AdditionalSources = additionalSources;
    }

    protected override ulong ComputeDigest()
    {
        var xxh = new XXH64();
        var hasher = new RunTimeCodeHasher( xxh );
        ulong hash = 0;

#if DEBUG
        var uniqueHashes = new HashSet<ulong>();
#endif

        foreach ( var tree in this.AdditionalSources.Values )
        {
            xxh.Reset();
            xxh.Update( tree.Name );
            hasher.Visit( tree.GeneratedSyntaxTree.GetRoot() );

            var digest = xxh.Digest();

#if DEBUG
            if ( !uniqueHashes.Add( digest ) )
            {
                // It is essential that hashes are distinct, because identical hashes nullify themselves.
                throw new AssertionFailedException();
            }
#endif

            hash ^= digest;
        }

        return hash;
    }

    public override void ProduceContent( SourceProductionContext context )
    {
        foreach ( var source in this.AdditionalSources.Values )
        {
            context.AddSource( source.Name, source.GeneratedSyntaxTree.GetText( context.CancellationToken ) );
        }
    }

    public override string ToString() => $"{nameof(SyntaxTreeSourceGeneratorResult)} Count={this.AdditionalSources.Count}";
}