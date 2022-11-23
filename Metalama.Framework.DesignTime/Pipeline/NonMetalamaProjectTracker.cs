// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline;

internal class NonMetalamaProjectTracker
{
    private readonly ProjectVersionProvider _projectVersionProvider;

    public NonMetalamaProjectTracker( GlobalServiceProvider serviceProvider )
    {
        this._projectVersionProvider = serviceProvider.GetRequiredService<ProjectVersionProvider>();
    }

    public async ValueTask<DesignTimeProjectReference> GetCompilationReferenceAsync(
        Compilation? oldCompilation,
        Compilation newCompilation,
        TestableCancellationToken cancellationToken )
    {
        // We don't need the changes, but using the CompilationChangesProvider allows us to get the new CompilationVersion incrementally,
        // without recomputing it from scratch.
        var compilationVersion = await this._projectVersionProvider.GetCompilationVersionAsync( oldCompilation, newCompilation, cancellationToken );

        return new DesignTimeProjectReference( compilationVersion );
    }
}