// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

/// <summary>
/// A unit-testable base for <see cref="DependencyCollector"/>. 
/// </summary>
internal class BaseDependencyCollector
{
    public static BaseDependencyCollector Empty { get; } = new();

#if DEBUG
    static BaseDependencyCollector()
    {
        Empty.Freeze();
    }
#endif

    public ImmutableDictionary<AssemblyIdentity, ICompilationVersion> CompilationReferences { get; }

    private readonly Dictionary<string, DependencyCollectorByDependentSyntaxTree> _dependenciesByDependentFilePath = new();

    public IReadOnlyDictionary<string, DependencyCollectorByDependentSyntaxTree> DependenciesByDependentFilePath => this._dependenciesByDependentFilePath;

    public BaseDependencyCollector( IEnumerable<ICompilationVersion>? compilationReferences = null )
    {
        if ( compilationReferences != null )
        {
            this.CompilationReferences = compilationReferences.ToImmutableDictionary( x => x.AssemblyIdentity, x => x );
        }
        else
        {
            this.CompilationReferences = ImmutableDictionary<AssemblyIdentity, ICompilationVersion>.Empty;
        }
    }

    public IEnumerable<DependencyEdge> EnumerateDependencies()
    {
        foreach ( var dependenciesByDependentSyntaxTree in this._dependenciesByDependentFilePath )
        {
            foreach ( var dependenciesInCompilation in dependenciesByDependentSyntaxTree.Value.DependenciesByCompilation )
            {
                foreach ( var dependency in dependenciesInCompilation.Value.MasterFilePathsAndHashes )
                {
                    yield return new DependencyEdge( dependenciesInCompilation.Key, dependency.Key, dependency.Value, dependenciesByDependentSyntaxTree.Key );
                }
            }
        }
    }

    public void AddDependency( string dependentFilePath, AssemblyIdentity masterCompilation, string masterFilePath, ulong masterHash )
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

        dependencies.AddDependency( masterCompilation, masterFilePath, masterHash );
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