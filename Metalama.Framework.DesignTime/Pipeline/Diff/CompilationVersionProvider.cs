// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Dependencies;
using Metalama.Framework.Engine;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline.Diff;

/// <summary>
/// Computes and caches the <see cref="CompilationChanges"/> between pairs of <see cref="Compilation"/> instances.
/// </summary>
internal partial class CompilationVersionProvider : IService
{
    private readonly Implementation _implementation;

    public CompilationVersionProvider( IServiceProvider serviceProvider )
    {
        this._implementation = new Implementation( serviceProvider );
    }

    public ValueTask<CompilationVersion> GetCompilationVersionAsync(
        Compilation? oldCompilation,
        Compilation newCompilation,
        CancellationToken cancellationToken = default )
        => this._implementation.GetCompilationVersionCoreAsync( oldCompilation, newCompilation, false, cancellationToken );

    public ValueTask<CompilationVersion> GetCompilationVersionAsync(
        Compilation newCompilation,
        CancellationToken cancellationToken = default )
        => this._implementation.GetCompilationVersionCoreAsync( null, newCompilation, false, cancellationToken );

    public ValueTask<CompilationChanges> GetCompilationChangesAsync(
        Compilation? oldCompilation,
        Compilation newCompilation,
        CancellationToken cancellationToken = default )
        => this._implementation.GetCompilationChangesAsyncCore( oldCompilation, newCompilation, false, cancellationToken );

    public ValueTask<CompilationChanges> MergeChangesAsync( CompilationChanges first, CompilationChanges second, CancellationToken cancellationToken )
        => this._implementation.MergeChangesCoreAsync( first, second, false, cancellationToken );

    public async ValueTask<DependencyGraph> ProcessCompilationChangesAsync(
        CompilationChanges changes,
        DependencyGraph dependencyGraph,
        Action<string> invalidateAction,
        bool invalidateOnlyDependencies = false,
        CancellationToken cancellationToken = default )
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
            if ( !processedCompilations.Add( currentCompilationChanges.NewCompilationVersion.Compilation ) )
            {
                // This set of changes has already been processed.
                return;
            }

            if ( dependencyGraph.DependenciesByCompilation.TryGetValue( currentCompilationChanges.AssemblyIdentity, out var dependenciesOfCompilation ) )
            {
                // Process syntax trees.
                foreach ( var syntaxTreeChangeEntry in currentCompilationChanges.SyntaxTreeChanges )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var syntaxTreeChange = syntaxTreeChangeEntry.Value;

                    if ( syntaxTreeChange.SyntaxTreeChangeKind is SyntaxTreeChangeKind.Changed or SyntaxTreeChangeKind.Deleted )
                    {
                        if ( dependenciesOfCompilation.DependenciesByMasterFilePath.TryGetValue( syntaxTreeChange.FilePath, out var dependenciesOfSyntaxTree ) )
                        {
                            foreach ( var dependentSyntaxTree in dependenciesOfSyntaxTree.DependentFilePaths )
                            {
                                invalidateAction( dependentSyntaxTree );
                            }
                        }

                        if ( syntaxTreeChange.SyntaxTreeChangeKind == SyntaxTreeChangeKind.Deleted )
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
                    case ReferencedCompilationChangeKind.Modified
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

                    case ReferencedCompilationChangeKind.Removed:
                        dependencyGraphBuilder.RemoveCompilation( reference.Key );

                        break;
                }
            }
        }
    }
}