// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

/// <summary>
/// Collects the dependencies of a given dependent syntax tree.
/// </summary>
internal class DependencyCollectorByDependentSyntaxTree
{
    private readonly Dictionary<AssemblyIdentity, DependencyCollectorByDependentSyntaxTreeAndMasterCompilation> _dependenciesByCompilation = new();

    public string DependentFilePath { get; }

    public IReadOnlyDictionary<AssemblyIdentity, DependencyCollectorByDependentSyntaxTreeAndMasterCompilation> DependenciesByCompilation
        => this._dependenciesByCompilation;

    public DependencyCollectorByDependentSyntaxTree( string dependentFilePath )
    {
        this.DependentFilePath = dependentFilePath;
    }

    public void AddDependency( AssemblyIdentity masterCompilation, string masterFilePath, ulong masterHash )
    {
#if DEBUG
        if ( this.IsReadOnly )
        {
            throw new InvalidOperationException();
        }
#endif

        if ( !this._dependenciesByCompilation.TryGetValue( masterCompilation, out var compilationCollector ) )
        {
            compilationCollector = new DependencyCollectorByDependentSyntaxTreeAndMasterCompilation( this.DependentFilePath, masterCompilation );
            this._dependenciesByCompilation.Add( masterCompilation, compilationCollector );
        }

        compilationCollector.AddDependency( masterFilePath, masterHash );
    }

#if DEBUG
    public bool IsReadOnly { get; private set; }

    public void Freeze()
    {
        this.IsReadOnly = true;

        foreach ( var child in this._dependenciesByCompilation.Values )
        {
            child.Freeze();
        }
    }
#endif
}