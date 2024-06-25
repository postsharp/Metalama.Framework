// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Framework.Engine.Utilities.Comparers;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Introspection;
using System.Threading;

namespace Metalama.Framework.Engine.Introspection.References;

#pragma warning disable CA1001
internal sealed class ProjectIntrospectionService : IProjectIntrospectionService
#pragma warning restore CA1001
{
    private readonly IConcurrentTaskRunner _concurrentTaskRunner = new SingleThreadedTaskRunner();
    private readonly ITaskRunner _taskRunner;
    private readonly WeakCache<ICompilation, ProjectReferenceGraph> _cache = new();

    public ProjectIntrospectionService( ProjectServiceProvider serviceProvider )
    {
        this._taskRunner = serviceProvider.Global.GetRequiredService<ITaskRunner>();
    }

    public IReferenceGraph GetReferenceGraph( ICompilation compilation )
    {
        // Lock to prevent concurrent evaluation.
        lock ( this._cache )
        {
            return this._cache.GetOrAdd( compilation, this.GetReferenceGraphCore );
        }
    }

    private ProjectReferenceGraph GetReferenceGraphCore( ICompilation compilation )
    {
        var compilationModel = (CompilationModel) compilation;
        var compilationContext = compilationModel.CompilationContext;

        var builder = new ReferenceIndexBuilder( compilationModel.Project.ServiceProvider, ReferenceIndexerOptions.All, StructuralSymbolComparer.Default );

        this._taskRunner.RunSynchronously(
            () => this._concurrentTaskRunner.RunConcurrentlyAsync(
                compilationContext.Compilation.SyntaxTrees,
                tree => builder.IndexSyntaxTree( tree, compilationContext.SemanticModelProvider ),
                CancellationToken.None ) );

        var referenceIndex = builder.ToReadOnly();

        return new ProjectReferenceGraph( compilationModel, referenceIndex );
    }
}