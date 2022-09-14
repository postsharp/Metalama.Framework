// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline;

internal class NonMetalamaProjectTracker
{
    private readonly CompilationChangesProvider _compilationChangesProvider;

    public NonMetalamaProjectTracker( IServiceProvider serviceProvider )
    {
        this._compilationChangesProvider = serviceProvider.GetRequiredService<DesignTimeAspectPipelineFactory>().CompilationChangesProvider;
    }

    public async ValueTask<DesignTimeCompilationReference> GetCompilationReferenceAsync(
        Compilation? oldCompilation,
        Compilation newCompilation,
        CancellationToken cancellationToken )
    {
        // We don't need the changes, but using the CompilationChangesProvider allows us to get the new CompilationVersion incrementally,
        // without recomputing it from scratch.
        var changes = await this._compilationChangesProvider.GetCompilationChangesAsync( oldCompilation, newCompilation, false, cancellationToken );

        return new DesignTimeCompilationReference( changes.NewCompilationVersion, false );
    }
}