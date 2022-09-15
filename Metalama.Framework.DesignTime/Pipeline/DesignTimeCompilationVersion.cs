// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal class DesignTimeCompilationVersion : ITransitiveAspectManifestProvider
{
    public ICompilationVersion CompilationVersion { get; }

    public ImmutableDictionary<AssemblyIdentity, DesignTimeCompilationReference> References { get; }

    // For test only.
    public DesignTimeCompilationVersion( ICompilationVersion compilationVersion ) : this(
        compilationVersion,
        compilationVersion.ReferencedCompilations.Values.Select( x => new DesignTimeCompilationReference( x ) ) ) { }

    public DesignTimeCompilationVersion( ICompilationVersion compilationVersion, IEnumerable<DesignTimeCompilationReference> references )
    {
        this.CompilationVersion = compilationVersion;
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