// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline;

internal class NonMetalamaProjectTracker
{
    private readonly CompilationVersionProvider _compilationVersionProvider;

    public NonMetalamaProjectTracker( IServiceProvider serviceProvider )
    {
        this._compilationVersionProvider = serviceProvider.GetRequiredService<DesignTimeAspectPipelineFactory>().CompilationVersionProvider;
    }

    public async ValueTask<DesignTimeCompilationReference> GetCompilationReferenceAsync(
        Compilation? oldCompilation,
        Compilation newCompilation,
        CancellationToken cancellationToken )
    {
        // We don't need the changes, but using the CompilationChangesProvider allows us to get the new CompilationVersion incrementally,
        // without recomputing it from scratch.
        var compilationVersion = await this._compilationVersionProvider.GetCompilationVersionAsync( oldCompilation, newCompilation, cancellationToken );

        return new DesignTimeCompilationReference( compilationVersion, false );
    }
}