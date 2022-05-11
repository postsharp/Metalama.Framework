// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using K4os.Hash.xxHash;
using Metalama.Framework.Engine.AdditionalOutputs;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.SourceGeneration;

/// <summary>
/// An implementation of <see cref="SourceGeneratorResult"/> that is backed by a set of
/// files on the filesystem, generated at compile-time when the design-time pipeline is disabled.
/// </summary>
public sealed class OfflineSourceGeneratorResult : SourceGeneratorResult
{
    public ImmutableArray<AdditionalCompilationOutputFile> OfflineFiles { get; }

    public OfflineSourceGeneratorResult( ImmutableArray<AdditionalCompilationOutputFile> offlineFiles )
    {
        this.OfflineFiles = offlineFiles;
    }

    protected override ulong ComputeDigest()
    {
        var xxh = new XXH64();
        ulong hash = 0;

        foreach ( var file in this.OfflineFiles )
        {
            xxh.Reset();
            xxh.Update( file.Path );
            xxh.Update( File.GetLastWriteTime( file.Path ).ToFileTimeUtc() );

            hash ^= xxh.Digest();
        }

        return hash;
    }

    public override void ProduceContent( SourceProductionContext context )
    {
        foreach ( var file in this.OfflineFiles )
        {
            using var stream = file.GetStream();
            context.AddSource( Path.GetFileName( file.Path ), SourceText.From( stream ) );
        }
    }

    public override string ToString() => $"{nameof(OfflineSourceGeneratorResult)} Count={this.OfflineFiles.Length}";
}