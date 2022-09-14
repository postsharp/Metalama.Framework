// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

/// <summary>
/// Represents all dependencies of a given compilation.
/// </summary>
internal readonly struct DependencyGraph
{
    public ImmutableDictionary<AssemblyIdentity, ICompilationVersion> Compilations { get; }

    public static DependencyGraph Empty { get; } = new(
        ImmutableDictionary<AssemblyIdentity, DependencyGraphByDependentCompilation>.Empty,
        ImmutableDictionary<AssemblyIdentity, ICompilationVersion>.Empty );

    public ImmutableDictionary<AssemblyIdentity, DependencyGraphByDependentCompilation> DependenciesByCompilation { get; }

    public DependencyGraph Update(
        IEnumerable<string> syntaxTrees,
        BaseDependencyCollector dependencies )
    {
        var compilationsBuilder = this.DependenciesByCompilation.ToBuilder();

        // Updating compilation references.
        foreach ( var compilationReference in dependencies.CompilationReferences )
        {
            if ( this.DependenciesByCompilation.TryGetValue( compilationReference.Key, out var currentDependencyGraphByDependentCompilation ) )
            {
                if ( currentDependencyGraphByDependentCompilation.TryUpdateCompileTimeProjectHash(
                        compilationReference.Value.CompileTimeProjectHash,
                        out var newValue ) )
                {
                    compilationsBuilder[compilationReference.Key] = newValue;
                }
            }
            else
            {
                compilationsBuilder[compilationReference.Key] = new DependencyGraphByDependentCompilation(
                    compilationReference.Key,
                    compilationReference.Value.CompileTimeProjectHash );
            }
        }

        // Add or update dependencies.
        foreach ( var dependenciesByDependentFilePath in dependencies.DependenciesByDependentFilePath )
        {
            var dependentFilePath = dependenciesByDependentFilePath.Key;

            // ReSharper disable once SuspiciousTypeConversion.Global

            foreach ( var dependenciesByCompilation in dependenciesByDependentFilePath.Value.DependenciesByCompilation )
            {
                var compilation = dependenciesByCompilation.Key;

                if ( !compilationsBuilder.TryGetValue( compilation, out var currentDependenciesOfCompilation ) )
                {
                    var hashCode = dependencies.CompilationReferences.TryGetValue( compilation, out var reference ) ? reference.CompileTimeProjectHash : 0;
                    currentDependenciesOfCompilation = new DependencyGraphByDependentCompilation( compilation, hashCode );
                }

                if ( currentDependenciesOfCompilation.TryUpdateDependencies(
                        dependentFilePath,
                        dependenciesByCompilation.Value,
                        out var newDependenciesOfCompilation ) )
                {
                    compilationsBuilder[compilation] = newDependenciesOfCompilation;
                }
                else
                {
                    // The dependencies have not changed.
                }
            }
        }

        // Remove dependencies.
        foreach ( var dependentFilePath in syntaxTrees )
        {
            if ( !dependencies.DependenciesByDependentFilePath.ContainsKey( dependentFilePath ) )
            {
                // The syntax tree does not have any dependency in any compilation.
                foreach ( var compilationDependencies in this.DependenciesByCompilation )
                {
                    if ( compilationDependencies.Value.TryRemoveDependentSyntaxTree( dependentFilePath, out var newDependencies ) )
                    {
                        compilationsBuilder[compilationDependencies.Key] = newDependencies;
                    }
                }
            }
        }

        return new DependencyGraph( compilationsBuilder.ToImmutable(), dependencies.CompilationReferences );
    }

    private DependencyGraph(
        ImmutableDictionary<AssemblyIdentity, DependencyGraphByDependentCompilation> dependenciesByCompilation,
        ImmutableDictionary<AssemblyIdentity, ICompilationVersion> compilations )
    {
        this.DependenciesByCompilation = dependenciesByCompilation;
        this.Compilations = compilations;
    }
}