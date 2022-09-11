// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Diff;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline;

internal class NonMetalamaProjectTracker
{
    private readonly CompilationVersionGraph _compilationVersionGraph;

    public NonMetalamaProjectTracker( ProjectKey projectKey, IServiceProvider serviceProvider )
    {
        this.ProjectKey = projectKey;
        this._compilationVersionGraph = new CompilationVersionGraph( new DiffStrategy( serviceProvider, false, true ) );
    }

    public ProjectKey ProjectKey { get; }

    public async ValueTask<DesignTimeCompilationReference> GetCompilationReferenceAsync(
        Compilation? oldCompilation,
        Compilation newCompilation,
        CancellationToken cancellationToken )
    {
        var compilationVersion = await this._compilationVersionGraph.GetCompilationVersion( oldCompilation, newCompilation, cancellationToken );

        // If we fall here, it means that the new compilation is identical to the previous one.
        return new DesignTimeCompilationReference( compilationVersion );
    }
}