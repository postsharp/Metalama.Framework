// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

/// <summary>
/// Represents a dependency of a compilation to another compilation.
/// </summary>
internal readonly struct CompilationDependency
{
    private static readonly ImmutableDictionary<string, SyntaxTreeDependencyCollection> _emptyDependenciesByMasterFilePath =
        ImmutableDictionary<string, SyntaxTreeDependencyCollection>.Empty.WithComparers( StringComparer.Ordinal );

    private static readonly ImmutableDictionary<string, SyntaxTreeDependencyInCompilationCollector> _emptyDependenciesByDependentFilePath =
        ImmutableDictionary<string, SyntaxTreeDependencyInCompilationCollector>.Empty.WithComparers( StringComparer.Ordinal );

    public AssemblyIdentity AssemblyIdentity { get; }

    public ulong CompileTimeProjectHash { get; }

    /// <summary>
    /// Gets the list of dependencies on syntax trees within the master compilation, indexed by file path.
    /// </summary>
    public ImmutableDictionary<string, SyntaxTreeDependencyCollection> DependenciesByMasterFilePath { get; }

    private readonly ImmutableDictionary<string, SyntaxTreeDependencyInCompilationCollector> _dependenciesByDependentFilePath;

    public CompilationDependency( AssemblyIdentity assemblyIdentity, ulong compileTimeProjectHash ) : this(
        assemblyIdentity,
        compileTimeProjectHash,
        _emptyDependenciesByMasterFilePath,
        _emptyDependenciesByDependentFilePath ) { }

    private CompilationDependency(
        AssemblyIdentity assemblyIdentity,
        ulong compileTimeProjectHash,
        ImmutableDictionary<string, SyntaxTreeDependencyCollection> dependenciesByMasterFilePath,
        ImmutableDictionary<string, SyntaxTreeDependencyInCompilationCollector> dependenciesByDependentFilePath )
    {
        this.AssemblyIdentity = assemblyIdentity;
        this.CompileTimeProjectHash = compileTimeProjectHash;
        this.DependenciesByMasterFilePath = dependenciesByMasterFilePath;
        this._dependenciesByDependentFilePath = dependenciesByDependentFilePath;
    }

    public bool TryRemoveDependency( string dependentFilePath, out CompilationDependency newDependencies )
    {
        if ( !this._dependenciesByDependentFilePath.TryGetValue( dependentFilePath, out var oldDependencies ) )
        {
            // There is nothing to do because the dependency was not present.
            newDependencies = this;

            return false;
        }

        var dependenciesByMasterFilePathBuilder = this.DependenciesByMasterFilePath.ToBuilder();

        foreach ( var oldMasterFilePathAndHash in oldDependencies.MasterFilePathsAndHashes )
        {
            var masterFilePath = oldMasterFilePathAndHash.Key;

            if ( dependenciesByMasterFilePathBuilder.TryGetValue( masterFilePath, out var syntaxTreeDependencies ) )
            {
                var newSyntaxTreeDependencies = syntaxTreeDependencies.RemoveDependency( dependentFilePath );

                if ( newSyntaxTreeDependencies.DependentFilePaths.IsEmpty )
                {
                    dependenciesByMasterFilePathBuilder.Remove( masterFilePath );
                }
                else
                {
                    dependenciesByMasterFilePathBuilder[masterFilePath] = newSyntaxTreeDependencies;
                }
            }
        }

        newDependencies = new CompilationDependency(
            this.AssemblyIdentity,
            this.CompileTimeProjectHash,
            dependenciesByMasterFilePathBuilder.ToImmutable(),
            this._dependenciesByDependentFilePath.Remove( dependentFilePath ) );

        return true;
    }

    public bool TryUpdateDependency(
        string dependentFilePath,
        SyntaxTreeDependencyInCompilationCollector dependencies,
        out CompilationDependency newDependencies )
    {
        // Check if there is any change.
        if ( this._dependenciesByDependentFilePath.TryGetValue( dependentFilePath, out var oldDependencies )
             && dependencies.IsStructurallyEqual( oldDependencies ) )
        {
            newDependencies = this;

            return false;
        }

        var dependenciesByMasterFilePathBuilder = this.DependenciesByMasterFilePath.ToBuilder();

        // Add dependencies.
        foreach ( var masterFilePathAndHash in dependencies.MasterFilePathsAndHashes )
        {
            if ( !dependenciesByMasterFilePathBuilder.TryGetValue( masterFilePathAndHash.Key, out var syntaxTreeDependencies ) )
            {
                syntaxTreeDependencies = new SyntaxTreeDependencyCollection( masterFilePathAndHash.Key, masterFilePathAndHash.Value );
                dependenciesByMasterFilePathBuilder.Add( masterFilePathAndHash.Key, syntaxTreeDependencies );
            }

            dependenciesByMasterFilePathBuilder[masterFilePathAndHash.Key] = syntaxTreeDependencies.AddDependency( dependentFilePath );
        }

        // Remove dependencies.
        if ( oldDependencies != null )
        {
            foreach ( var oldMasterFilePathAndHash in oldDependencies.MasterFilePathsAndHashes )
            {
                var masterFilePath = oldMasterFilePathAndHash.Key;

                if ( !dependencies.MasterFilePathsAndHashes.ContainsKey( masterFilePath ) )
                {
                    if ( dependenciesByMasterFilePathBuilder.TryGetValue( masterFilePath, out var syntaxTreeDependencies ) )
                    {
                        var newSyntaxTreeDependencies = syntaxTreeDependencies.RemoveDependency( dependentFilePath );

                        if ( newSyntaxTreeDependencies.DependentFilePaths.IsEmpty )
                        {
                            dependenciesByMasterFilePathBuilder.Remove( masterFilePath );
                        }
                        else
                        {
                            dependenciesByMasterFilePathBuilder[masterFilePath] = newSyntaxTreeDependencies;
                        }
                    }
                }
            }
        }

        newDependencies = new CompilationDependency(
            this.AssemblyIdentity,
            this.CompileTimeProjectHash,
            dependenciesByMasterFilePathBuilder.ToImmutable(),
            this._dependenciesByDependentFilePath.SetItem( dependentFilePath, dependencies ) );

        return true;
    }
}