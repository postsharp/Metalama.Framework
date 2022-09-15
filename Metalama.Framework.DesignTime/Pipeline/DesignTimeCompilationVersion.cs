// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal class DesignTimeCompilationVersion : ITransitiveAspectManifestProvider
{
    public IProjectVersion ProjectVersion { get; }

    public ImmutableDictionary<ProjectKey, DesignTimeCompilationReference> References { get; }

    // For test only.
    public DesignTimeCompilationVersion( IProjectVersion projectVersion ) : this(
        projectVersion,
        projectVersion.ReferencedProjectVersions.Values.Select( x => new DesignTimeCompilationReference( x ) ) ) { }

    public DesignTimeCompilationVersion( IProjectVersion projectVersion, IEnumerable<DesignTimeCompilationReference> references )
    {
        this.ProjectVersion = projectVersion;
        this.References = references.ToImmutableDictionary( x => x.ProjectVersion.ProjectKey, x => x );
    }

    public ITransitiveAspectsManifest? GetTransitiveAspectsManifest( Compilation compilation, CancellationToken cancellationToken )
    {
        if ( this.References.TryGetValue( ProjectKeyExtensions.GetProjectKey( compilation ), out var reference ) )
        {
            return reference.TransitiveAspectsManifest;
        }

        return null;
    }
}