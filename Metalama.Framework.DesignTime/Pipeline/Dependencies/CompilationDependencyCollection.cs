﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

internal readonly struct CompilationDependencyCollection
{
    public static CompilationDependencyCollection Empty { get; } = new( ImmutableDictionary<AssemblyIdentity, CompilationDependency>.Empty );

    public ImmutableDictionary<AssemblyIdentity, CompilationDependency> Compilations { get; }

    public CompilationDependencyCollection Update(
        IEnumerable<string> syntaxTrees,
        DependencyCollector dependencies,
        DesignTimeCompilationReferenceCollection references )
    {
        var compilationsBuilder = this.Compilations.ToBuilder();

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
                    var hashCode = references.References.TryGetValue( compilation, out var reference ) ? reference.CompileTimeProjectHash : 0;
                    currentDependenciesOfCompilation = new CompilationDependency( compilation, hashCode );
                }

                if ( currentDependenciesOfCompilation.TryUpdateDependency(
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
                foreach ( var compilationDependencies in compilationsBuilder )
                {
                    if ( compilationDependencies.Value.TryRemoveDependency( dependentFilePath, out var newDependencies ) )
                    {
                        compilationsBuilder[compilationDependencies.Key] = newDependencies;
                    }
                }
            }
        }

        return new CompilationDependencyCollection( compilationsBuilder.ToImmutable() );
    }

    private CompilationDependencyCollection( ImmutableDictionary<AssemblyIdentity, CompilationDependency> compilations )
    {
        this.Compilations = compilations;
    }
}