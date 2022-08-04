// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

/// <summary>
/// Collects the dependencies of a given dependent syntax tree.
/// </summary>
internal class SyntaxTreeDependencyCollector
{
    private readonly Dictionary<AssemblyIdentity, SyntaxTreeDependencyInCompilationCollector> _dependenciesByCompilation = new();

    public string DependentFilePath { get; }

    public IReadOnlyDictionary<AssemblyIdentity, SyntaxTreeDependencyInCompilationCollector> DependenciesByCompilation => this._dependenciesByCompilation;

    public SyntaxTreeDependencyCollector( string dependentFilePath )
    {
        this.DependentFilePath = dependentFilePath;
    }

    public void AddDependency( Compilation masterCompilation, string masterFilePath, ulong masterHash )
    {
#if DEBUG
        if ( this.IsReadOnly )
        {
            throw new InvalidOperationException();
        }
#endif

        var assemblyIdentity = masterCompilation.Assembly.Identity;

        if ( !this._dependenciesByCompilation.TryGetValue( assemblyIdentity, out var compilationCollector ) )
        {
            compilationCollector = new SyntaxTreeDependencyInCompilationCollector( this.DependentFilePath, assemblyIdentity );
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