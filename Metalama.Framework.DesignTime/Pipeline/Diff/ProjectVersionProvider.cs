// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Dependencies;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline.Diff;

/// <summary>
/// Computes and instances of <see cref="ProjectVersion"/> and <see cref="CompilationChanges"/>.
/// </summary>
internal partial class ProjectVersionProvider : IGlobalService, IDisposable
{
    private readonly Implementation _implementation;

    public ProjectVersionProvider( GlobalServiceProvider serviceProvider, bool isTest = false )
    {
        this._implementation = new Implementation( serviceProvider, isTest );
    }

    public ValueTask<ProjectVersion> GetCompilationVersionAsync(
        Compilation? oldCompilation,
        Compilation newCompilation,
        TestableCancellationToken cancellationToken = default )
        => this._implementation.GetCompilationVersionCoreAsync( oldCompilation, newCompilation, false, cancellationToken );

    public ValueTask<ProjectVersion> GetCompilationVersionAsync(
        Compilation newCompilation,
        TestableCancellationToken cancellationToken = default )
        => this._implementation.GetCompilationVersionCoreAsync( null, newCompilation, false, cancellationToken );

    public ValueTask<CompilationChanges> GetCompilationChangesAsync(
        Compilation? oldCompilation,
        Compilation newCompilation,
        TestableCancellationToken cancellationToken = default )
        => this._implementation.GetCompilationChangesAsyncCoreAsync( oldCompilation, newCompilation, false, cancellationToken );

    public async ValueTask<DependencyGraph> ProcessCompilationChangesAsync(
        CompilationChanges changes,
        DependencyGraph dependencyGraph,
        Action<string> invalidateAction,
        bool invalidateOnlyDependencies = false,
        TestableCancellationToken cancellationToken = default )
    {
        HashSet<Compilation> processedCompilations = new();
        var dependencyGraphBuilder = dependencyGraph.ToBuilder();

        await ProcessCompilationRecursiveAsync( changes );

        if ( !invalidateOnlyDependencies )
        {
            foreach ( var syntaxTreeChange in changes.SyntaxTreeChanges )
            {
                invalidateAction( syntaxTreeChange.Key );
            }
        }

        return dependencyGraphBuilder.ToImmutable();

        async ValueTask ProcessCompilationRecursiveAsync( CompilationChanges currentCompilationChanges )
        {
            // Prevent duplicate processing.
            if ( !processedCompilations.Add( currentCompilationChanges.NewProjectVersion.Compilation ) )
            {
                // This set of changes has already been processed.
                return;
            }

            if ( dependencyGraph.DependenciesByMasterProject.TryGetValue( currentCompilationChanges.ProjectKey, out var dependenciesOfCompilation ) )
            {
                // Process syntax trees.
                foreach ( var syntaxTreeChangeEntry in currentCompilationChanges.SyntaxTreeChanges )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var syntaxTreeChange = syntaxTreeChangeEntry.Value;

                    if ( syntaxTreeChange.SyntaxTreeChangeKind is SyntaxTreeChangeKind.Changed or SyntaxTreeChangeKind.Removed )
                    {
                        if ( dependenciesOfCompilation.DependenciesByMasterFilePath.TryGetValue( syntaxTreeChange.FilePath, out var dependenciesOfSyntaxTree ) )
                        {
                            foreach ( var dependentSyntaxTree in dependenciesOfSyntaxTree.DependentFilePaths )
                            {
                                invalidateAction( dependentSyntaxTree );
                            }
                        }

                        if ( syntaxTreeChange.SyntaxTreeChangeKind == SyntaxTreeChangeKind.Removed )
                        {
                            dependencyGraphBuilder.RemoveDependentSyntaxTree( syntaxTreeChange.FilePath );
                        }
                    }

                    // Process partial types.
                    foreach ( var partialTypeChange in syntaxTreeChange.PartialTypeChanges )
                    {
                        if ( dependenciesOfCompilation.DependenciesByMasterPartialType.TryGetValue(
                                partialTypeChange.Type,
                                out var dependenciesOfPartialType ) )
                        {
                            foreach ( var dependentSyntaxTree in dependenciesOfPartialType.DependentFilePaths )
                            {
                                invalidateAction( dependentSyntaxTree );
                            }
                        }
                    }
                }
            }
            else
            {
                // There is no dependency on this compilation, but there may be on recursively referenced compilations.
            }

            // Process references.
            foreach ( var reference in currentCompilationChanges.ReferencedCompilationChanges )
            {
                cancellationToken.ThrowIfCancellationRequested();

                switch ( reference.Value.ChangeKind )
                {
                    case ReferenceChangeKind.Modified
                        when !processedCompilations.Contains( reference.Value.NewCompilation.AssertNotNull() ):
                        {
                            var referenceChanges = reference.Value.Changes
                                                   ?? await this.GetCompilationChangesAsync(
                                                       reference.Value.OldCompilation.AssertNotNull(),
                                                       reference.Value.NewCompilation.AssertNotNull(),
                                                       cancellationToken );

                            await ProcessCompilationRecursiveAsync( referenceChanges );

                            break;
                        }

                    case ReferenceChangeKind.Removed:
                        dependencyGraphBuilder.RemoveProject( reference.Key );

                        break;
                }
            }
        }
    }

    public void Dispose() => this._implementation.Dispose();
}