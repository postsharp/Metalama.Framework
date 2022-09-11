// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal class DesignTimeCompilationReferenceCollection : ITransitiveAspectManifestProvider
{
    public static DesignTimeCompilationReferenceCollection Empty { get; } = new( Enumerable.Empty<DesignTimeCompilationReference>() );

    public ImmutableDictionary<AssemblyIdentity, DesignTimeCompilationReference> References { get; }

    public DesignTimeCompilationReferenceCollection( params DesignTimeCompilationReference[] references ) : this(
        (IEnumerable<DesignTimeCompilationReference>) references ) { }

    public DesignTimeCompilationReferenceCollection( IEnumerable<DesignTimeCompilationReference> references )
    {
        this.References = references.ToImmutableDictionary( x => x.CompilationVersion.AssemblyIdentity, x => x );
    }

    public ITransitiveAspectsManifest? GetTransitiveAspectsManifest( Compilation compilation, CancellationToken cancellationToken )
    {
        if ( this.References.TryGetValue( compilation.Assembly.Identity, out var reference ) )
        {
            return reference.TransitiveAspectsManifest;
        }

        return null;
    }
}