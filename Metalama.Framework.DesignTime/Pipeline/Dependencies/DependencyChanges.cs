// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Diff;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

internal class DependencyChanges
{
    private static readonly ImmutableHashSet<string> _emptyHashSet = ImmutableHashSet<string>.Empty.WithComparer( StringComparer.Ordinal );

    private DependencyChanges( bool hasCompileTimeChange, ImmutableHashSet<string> invalidatedSyntaxTrees )
    {
        this.HasCompileTimeChange = hasCompileTimeChange;
        this.InvalidatedSyntaxTrees = invalidatedSyntaxTrees;
    }

    public static readonly DependencyChanges Empty = new( false, _emptyHashSet );

    public bool HasCompileTimeChange { get; }

    public bool IsEmpty => !this.HasCompileTimeChange && this.InvalidatedSyntaxTrees.IsEmpty;

    public ImmutableHashSet<string> InvalidatedSyntaxTrees { get; }

    public static async ValueTask<DependencyChanges> IncrementalFromReferencesAsync(
        CompilationChangesProvider compilationChangesProvider,
        DependencyGraph oldGraph,
        DesignTimeCompilationReferenceCollection newReferences,
        CancellationToken cancellationToken = default )
    {
        var invalidatedFiles = ImmutableHashSet.CreateBuilder<string>( StringComparer.Ordinal );

        foreach ( var compilationReference in newReferences.References )
        {
            cancellationToken.ThrowIfCancellationRequested();

            if ( oldGraph.DependenciesByCompilation.TryGetValue( compilationReference.Key, out var dependenciesOfReference ) )
            {
                if ( dependenciesOfReference.CompileTimeProjectHash != compilationReference.Value.CompilationVersion.CompileTimeProjectHash )
                {
                    // If we have a compile-time change, there is no need to do anything.
                    return new DependencyChanges( true, ImmutableHashSet<string>.Empty );
                }

                oldGraph.Compilations.TryGetValue( compilationReference.Key, out var oldReference );

                var compilationChanges = await compilationChangesProvider.GetCompilationChangesAsync(
                    oldReference?.Compilation,
                    compilationReference.Value.CompilationVersion.Compilation,
                    compilationReference.Value.IsMetalamaEnabled,
                    cancellationToken );

                foreach ( var syntaxTreeChange in compilationChanges.SyntaxTreeChanges )
                {
                    if ( dependenciesOfReference.DependenciesByMasterFilePath.TryGetValue( syntaxTreeChange.FilePath, out var dependenciesOfSyntaxTree ) )
                    {
                        invalidatedFiles.UnionWith( dependenciesOfSyntaxTree.DependentFilePaths );
                    }

                    foreach ( var partialTypeChange in syntaxTreeChange.PartialTypeChanges )
                    {
                        if ( dependenciesOfReference.DependenciesByMasterPartialType.TryGetValue( partialTypeChange.Type, out var dependenciesOfPartialType ) )
                        {
                            invalidatedFiles.UnionWith( dependenciesOfPartialType.DependentFilePaths );
                        }
                    }
                }
            }
        }

        return new DependencyChanges( false, invalidatedFiles.ToImmutable() );
    }
}