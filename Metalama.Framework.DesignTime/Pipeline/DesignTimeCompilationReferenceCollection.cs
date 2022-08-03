// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Aspects;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal class DesignTimeCompilationReferenceCollection : ITransitiveAspectManifestProvider
{
    public static DesignTimeCompilationReferenceCollection Empty { get; } = new( Enumerable.Empty<DesignTimeCompilationReference>() );

    public ImmutableDictionary<Compilation, DesignTimeCompilationReference> References { get; }

    public DesignTimeCompilationReferenceCollection( IEnumerable<DesignTimeCompilationReference> references )
    {
        this.References = references.ToImmutableDictionary( x => x.CompilationVersion.Compilation, x => x );
    }

    public ITransitiveAspectsManifest? GetTransitiveAspectsManifest( Compilation compilation, CancellationToken cancellationToken )
    {
        if ( this.References.TryGetValue( compilation, out var reference ) )
        {
            return reference.TransitiveAspectsManifest;
        }

        return null;
    }
}