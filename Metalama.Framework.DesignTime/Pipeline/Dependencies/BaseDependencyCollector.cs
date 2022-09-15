// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

/// <summary>
/// A unit-testable base for <see cref="DependencyCollector"/>. 
/// </summary>
internal class BaseDependencyCollector
{
    public ICompilationVersion CompilationVersion { get; }

    /// <summary>
    /// Gets the <see cref="PartialCompilation"/> for which the dependency graph was collected.
    /// </summary>
    public PartialCompilation PartialCompilation { get; }

    private readonly Dictionary<string, DependencyCollectorByDependentSyntaxTree> _dependenciesByDependentFilePath = new();

    public IReadOnlyDictionary<string, DependencyCollectorByDependentSyntaxTree> DependenciesByDependentFilePath => this._dependenciesByDependentFilePath;

    public BaseDependencyCollector( ICompilationVersion compilationVersion, PartialCompilation? partialCompilation = null )
    {
        this.CompilationVersion = compilationVersion;
        this.PartialCompilation = partialCompilation ?? PartialCompilation.CreateComplete( compilationVersion.Compilation );
    }

    /// <summary>
    /// Enumerates the syntax tree dependencies. This method is used in tests only.
    /// </summary>
    public IEnumerable<SyntaxTreeDependency> EnumerateSyntaxTreeDependencies()
    {
        foreach ( var dependenciesByDependentSyntaxTree in this._dependenciesByDependentFilePath )
        {
            foreach ( var dependenciesInCompilation in dependenciesByDependentSyntaxTree.Value.DependenciesByCompilation )
            {
                foreach ( var masterFilePath in dependenciesInCompilation.Value.MasterFilePathsAndHashes.Keys )
                {
                    yield return new SyntaxTreeDependency( masterFilePath, dependenciesInCompilation.Value.DependentFilePath );
                }
            }
        }
    }

    /// <summary>
    /// Enumerates partial type dependencies. This method is used in tests only.
    /// </summary>
    public IEnumerable<PartialTypeDependency> EnumeratePartialTypeDependencies()
    {
        foreach ( var dependenciesByDependentSyntaxTree in this._dependenciesByDependentFilePath )
        {
            foreach ( var dependenciesInCompilation in dependenciesByDependentSyntaxTree.Value.DependenciesByCompilation )
            {
                foreach ( var masterType in dependenciesInCompilation.Value.MasterPartialTypes )
                {
                    yield return new PartialTypeDependency( masterType, dependenciesInCompilation.Value.DependentFilePath );
                }
            }
        }
    }

    public void AddPartialTypeDependency( string dependentFilePath, AssemblyIdentity masterCompilationIdentity, TypeDependencyKey masterPartialType )
    {
#if DEBUG
        if ( this.IsReadOnly )
        {
            throw new InvalidOperationException();
        }
#endif

        if ( !this._dependenciesByDependentFilePath.TryGetValue( dependentFilePath, out var dependencies ) )
        {
            dependencies = new DependencyCollectorByDependentSyntaxTree( dependentFilePath );
            this._dependenciesByDependentFilePath.Add( dependentFilePath, dependencies );
        }

        dependencies.AddPartialTypeDependency( masterCompilationIdentity, masterPartialType );
    }

    public void AddSyntaxTreeDependency( string dependentFilePath, AssemblyIdentity masterCompilationIdentity, string masterFilePath, ulong masterHash )
    {
#if DEBUG
        if ( this.IsReadOnly )
        {
            throw new InvalidOperationException();
        }
#endif

        if ( !this._dependenciesByDependentFilePath.TryGetValue( dependentFilePath, out var dependencies ) )
        {
            dependencies = new DependencyCollectorByDependentSyntaxTree( dependentFilePath );
            this._dependenciesByDependentFilePath.Add( dependentFilePath, dependencies );
        }

        dependencies.AddSyntaxTreeDependency( masterCompilationIdentity, masterFilePath, masterHash );
    }

#if DEBUG
    public bool IsReadOnly { get; private set; }

    public void Freeze()
    {
        this.IsReadOnly = true;

        foreach ( var child in this._dependenciesByDependentFilePath.Values )
        {
            child.Freeze();
        }
    }
#endif
}