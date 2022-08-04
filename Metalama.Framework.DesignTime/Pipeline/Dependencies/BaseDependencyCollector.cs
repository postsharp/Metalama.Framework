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

    public BaseDependencyCollector( params ICompilationVersion[] compilationReferences ) : this( (IEnumerable<ICompilationVersion>) compilationReferences ) { }

    public BaseDependencyCollector( IEnumerable<ICompilationVersion> compilationReferences )
    {
        this.CompilationReferences = compilationReferences.ToImmutableDictionary( x => x.AssemblyIdentity, x => x );
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