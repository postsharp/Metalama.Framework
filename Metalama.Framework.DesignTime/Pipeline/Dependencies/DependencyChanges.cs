// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

internal readonly struct DependencyChanges
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

    public static DependencyChanges Create(
        DependencyGraph dependencies,
        DesignTimeCompilationReferenceCollection currentReferences,
        CancellationToken cancellationToken = default )
    {
        var invalidatedFiles = ImmutableHashSet.CreateBuilder<string>( StringComparer.Ordinal );

        foreach ( var compilationReference in currentReferences.References )
        {
            cancellationToken.ThrowIfCancellationRequested();

            if ( dependencies.Compilations.TryGetValue( compilationReference.Key, out var dependenciesOfReference ) )
            {
                if ( dependenciesOfReference.CompileTimeProjectHash != compilationReference.Value.CompilationVersion.CompileTimeProjectHash )
                {
                    // If we have a compile-time change, there is no need to do anything.
                    return new DependencyChanges( true, ImmutableHashSet<string>.Empty );
                }

                foreach ( var syntaxTreeDependencyCollection in dependenciesOfReference.DependenciesByMasterFilePath )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if ( !compilationReference.Value.CompilationVersion.TryGetSyntaxTreeVersion(
                             syntaxTreeDependencyCollection.Key,
                             out var syntaxTreeVersion )
                         || syntaxTreeVersion.DeclarationHash != syntaxTreeDependencyCollection.Value.DeclarationHash )
                    {
                        // The file was changed or removed.
                        invalidatedFiles.UnionWith( syntaxTreeDependencyCollection.Value.DependentFilePaths );
                    }
                }
            }
        }

        return new DependencyChanges( false, invalidatedFiles.ToImmutable() );
    }

    public bool IsUninitialized => this.InvalidatedSyntaxTrees == null!;
}