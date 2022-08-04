// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

/// <summary>
/// Represents a dependency of a compilation to another compilation.
/// </summary>
internal readonly struct DependencyGraphByDependentCompilation
{
    private static readonly ImmutableDictionary<string, DependencyGraphByMasterSyntaxTree> _emptyDependenciesByMasterFilePath =
        ImmutableDictionary<string, DependencyGraphByMasterSyntaxTree>.Empty.WithComparers( StringComparer.Ordinal );

    private static readonly ImmutableDictionary<string, DependencyCollectorByDependentSyntaxTreeAndMasterCompilation> _emptyDependenciesByDependentFilePath =
        ImmutableDictionary<string, DependencyCollectorByDependentSyntaxTreeAndMasterCompilation>.Empty.WithComparers( StringComparer.Ordinal );

    public AssemblyIdentity AssemblyIdentity { get; }

    public ulong CompileTimeProjectHash { get; }

    /// <summary>
    /// Gets the list of dependencies on syntax trees within the master compilation, indexed by file path.
    /// </summary>
    public ImmutableDictionary<string, DependencyGraphByMasterSyntaxTree> DependenciesByMasterFilePath { get; }

    public ImmutableDictionary<TypeDependencyKey, DependencyGraphByMasterPartialType> DependenciesByMasterPartialType { get; }

    private readonly ImmutableDictionary<string, DependencyCollectorByDependentSyntaxTreeAndMasterCompilation> _dependenciesByDependentFilePath;

    public DependencyGraphByDependentCompilation( AssemblyIdentity assemblyIdentity, ulong compileTimeProjectHash ) : this(
        assemblyIdentity,
        compileTimeProjectHash,
        _emptyDependenciesByMasterFilePath,
        ImmutableDictionary<TypeDependencyKey, DependencyGraphByMasterPartialType>.Empty,
        _emptyDependenciesByDependentFilePath ) { }

    private DependencyGraphByDependentCompilation(
        AssemblyIdentity assemblyIdentity,
        ulong compileTimeProjectHash,
        ImmutableDictionary<string, DependencyGraphByMasterSyntaxTree> dependenciesByMasterFilePath,
        ImmutableDictionary<TypeDependencyKey, DependencyGraphByMasterPartialType> dependenciesByMasterPartialType,
        ImmutableDictionary<string, DependencyCollectorByDependentSyntaxTreeAndMasterCompilation> dependenciesByDependentFilePath )
    {
        this.AssemblyIdentity = assemblyIdentity;
        this.CompileTimeProjectHash = compileTimeProjectHash;
        this.DependenciesByMasterFilePath = dependenciesByMasterFilePath;
        this.DependenciesByMasterPartialType = dependenciesByMasterPartialType;
        this._dependenciesByDependentFilePath = dependenciesByDependentFilePath;
    }

    public bool TryRemoveDependentSyntaxTree( string dependentFilePath, out DependencyGraphByDependentCompilation newDependenciesGraph )
    {
        if ( !this._dependenciesByDependentFilePath.TryGetValue( dependentFilePath, out var oldDependencies ) )
        {
            // There is nothing to do because the dependency was not present.
            newDependenciesGraph = this;

            return false;
        }

        // Update syntax tree dependencies.
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

        // Update partial type dependencies.
        var dependenciesByMasterPartialTypesBuilder = this.DependenciesByMasterPartialType.ToBuilder();

        foreach ( var type in oldDependencies.MasterPartialTypes )
        {
            if ( dependenciesByMasterPartialTypesBuilder.TryGetValue( type, out var typeDependencies ) )
            {
                var newTypeDependencies = typeDependencies.RemoveDependency( dependentFilePath );

                if ( newTypeDependencies.DependentFilePaths.IsEmpty )
                {
                    dependenciesByMasterPartialTypesBuilder.Remove( type );
                }
                else
                {
                    dependenciesByMasterPartialTypesBuilder[type] = newTypeDependencies;
                }
            }
        }

        newDependenciesGraph = new DependencyGraphByDependentCompilation(
            this.AssemblyIdentity,
            this.CompileTimeProjectHash,
            dependenciesByMasterFilePathBuilder.ToImmutable(),
            dependenciesByMasterPartialTypesBuilder.ToImmutable(),
            this._dependenciesByDependentFilePath.Remove( dependentFilePath ) );

        return true;
    }

    public bool TryUpdateCompileTimeProjectHash( ulong hash, out DependencyGraphByDependentCompilation newDependenciesGraph )
    {
        if ( this.CompileTimeProjectHash == hash )
        {
            newDependenciesGraph = this;

            return false;
        }
        else
        {
            newDependenciesGraph = new DependencyGraphByDependentCompilation(
                this.AssemblyIdentity,
                hash,
                this.DependenciesByMasterFilePath,
                this.DependenciesByMasterPartialType,
                this._dependenciesByDependentFilePath );

            return true;
        }
    }

    public bool TryUpdateDependencies(
        string dependentFilePath,
        DependencyCollectorByDependentSyntaxTreeAndMasterCompilation dependencies,
        out DependencyGraphByDependentCompilation newDependenciesGraph )
    {
        // Check if there is any change.
        if ( this._dependenciesByDependentFilePath.TryGetValue( dependentFilePath, out var oldDependencies )
             && dependencies.IsStructurallyEqual( oldDependencies ) )
        {
            newDependenciesGraph = this;

            return false;
        }

        var dependenciesByMasterFilePathBuilder = this.DependenciesByMasterFilePath.ToBuilder();
        var dependenciesByMasterPartialTypeBuilder = this.DependenciesByMasterPartialType.ToBuilder();

        // Add syntax tree dependencies.
        foreach ( var masterFilePathAndHash in dependencies.MasterFilePathsAndHashes )
        {
            if ( !dependenciesByMasterFilePathBuilder.TryGetValue( masterFilePathAndHash.Key, out var syntaxTreeDependencies ) )
            {
                syntaxTreeDependencies = new DependencyGraphByMasterSyntaxTree( masterFilePathAndHash.Key, masterFilePathAndHash.Value );
            }

            dependenciesByMasterFilePathBuilder[masterFilePathAndHash.Key] = syntaxTreeDependencies.AddSyntaxTreeDependency( dependentFilePath );
        }

        // Add partial type dependencies.
        foreach ( var masterPartialType in dependencies.MasterPartialTypes )
        {
            if ( !dependenciesByMasterPartialTypeBuilder.TryGetValue( masterPartialType, out var partialTypeDependencies ) )
            {
                partialTypeDependencies = new DependencyGraphByMasterPartialType( masterPartialType );
            }

            dependenciesByMasterPartialTypeBuilder[masterPartialType] = partialTypeDependencies.AddPartialTypeDependency( dependentFilePath );
        }

        if ( oldDependencies != null )
        {
            // Remove syntax tree dependencies.
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

            // Remove partial types dependencies.

            foreach ( var type in oldDependencies.MasterPartialTypes )
            {
                if ( !dependencies.Contains( type ) )
                {
                    if ( dependenciesByMasterPartialTypeBuilder.TryGetValue( type, out var partialTypeDependencies ) )
                    {
                        var newPartialTypeDependencies = partialTypeDependencies.RemoveDependency( dependentFilePath );

                        if ( newPartialTypeDependencies.DependentFilePaths.IsEmpty )
                        {
                            dependenciesByMasterPartialTypeBuilder.Remove( type );
                        }
                        else
                        {
                            dependenciesByMasterPartialTypeBuilder[type] = newPartialTypeDependencies;
                        }
                    }
                }
            }
        }

        newDependenciesGraph = new DependencyGraphByDependentCompilation(
            this.AssemblyIdentity,
            this.CompileTimeProjectHash,
            dependenciesByMasterFilePathBuilder.ToImmutable(),
            dependenciesByMasterPartialTypeBuilder.ToImmutable(),
            this._dependenciesByDependentFilePath.SetItem( dependentFilePath, dependencies ) );

        return true;
    }
}