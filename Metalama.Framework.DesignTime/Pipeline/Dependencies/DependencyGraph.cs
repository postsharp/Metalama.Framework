﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

/// <summary>
/// Represents all dependencies of a given compilation.
/// </summary>
internal class DependencyGraph
{
    /// <summary>
    /// Gets the main compilation for which this <see cref="DependencyGraph"/> is created.
    /// </summary>
    public ICompilationVersion Compilation { get; }

    public static DependencyGraph Create( ICompilationVersion compilation, BaseDependencyCollector dependencies )
    {
        var emptyGraph = new DependencyGraph( compilation, ImmutableDictionary<AssemblyIdentity, DependencyGraphByDependentCompilation>.Empty );

        return emptyGraph.Update( compilation, dependencies );
    }

    /// <summary>
    /// Gets the dependencies indexed by compilation.
    /// </summary>
    public ImmutableDictionary<AssemblyIdentity, DependencyGraphByDependentCompilation> DependenciesByCompilation { get; }

    public DependencyGraph Update(
        ICompilationVersion compilationVersion,
        BaseDependencyCollector dependencies )
    {
        var dependenciesByCompilationDictionaryBuilder = this.DependenciesByCompilation.ToBuilder();

        // Updating compilation references.
        foreach ( var compilationReference in dependencies.CompilationReferences )
        {
            if ( this.DependenciesByCompilation.TryGetValue( compilationReference.Key, out var currentDependencyGraphByDependentCompilation ) )
            {
                if ( currentDependencyGraphByDependentCompilation.TryUpdateCompileTimeProjectHash(
                        compilationReference.Value.CompileTimeProjectHash,
                        out var newValue ) )
                {
                    dependenciesByCompilationDictionaryBuilder[compilationReference.Key] = newValue;
                }
            }
            else
            {
                dependenciesByCompilationDictionaryBuilder[compilationReference.Key] = new DependencyGraphByDependentCompilation(
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

                if ( !dependenciesByCompilationDictionaryBuilder.TryGetValue( compilation, out var currentDependenciesOfCompilation ) )
                {
                    var hashCode = dependencies.CompilationReferences.TryGetValue( compilation, out var reference ) ? reference.CompileTimeProjectHash : 0;
                    currentDependenciesOfCompilation = new DependencyGraphByDependentCompilation( compilation, hashCode );
                }

                if ( currentDependenciesOfCompilation.TryUpdateDependencies(
                        dependentFilePath,
                        dependenciesByCompilation.Value,
                        out var newDependenciesOfCompilation ) )
                {
                    dependenciesByCompilationDictionaryBuilder[compilation] = newDependenciesOfCompilation;
                }
                else
                {
                    // The dependencies have not changed.
                }
            }
        }

        // Remove graphs for syntax trees that have been removed from the compilation
        foreach ( var dependentFilePath in compilationVersion.EnumerateSyntaxTreePaths() )
        {
            if ( !dependencies.DependenciesByDependentFilePath.ContainsKey( dependentFilePath ) )
            {
                // The syntax tree does not have any dependency in any compilation.
                foreach ( var compilationDependencies in this.DependenciesByCompilation )
                {
                    if ( compilationDependencies.Value.TryRemoveDependentSyntaxTree( dependentFilePath, out var newDependencies ) )
                    {
                        dependenciesByCompilationDictionaryBuilder[compilationDependencies.Key] = newDependencies;
                    }
                }
            }
        }

        return new DependencyGraph( compilationVersion, dependenciesByCompilationDictionaryBuilder.ToImmutable() );
    }

    private DependencyGraph(
        ICompilationVersion compilation,
        ImmutableDictionary<AssemblyIdentity, DependencyGraphByDependentCompilation> dependenciesByCompilation )
    {
        this.DependenciesByCompilation = dependenciesByCompilation;
        this.Compilation = compilation;
    }
}