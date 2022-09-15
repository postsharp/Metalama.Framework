// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

/// <summary>
/// Collects the dependencies of a given dependent syntax tree.
/// </summary>
internal class DependencyCollectorByDependentSyntaxTree
{
    private readonly Dictionary<ProjectKey, DependencyCollectorByDependentSyntaxTreeAndMasterProject> _dependenciesByCompilation = new();

    public string DependentFilePath { get; }

    public IReadOnlyDictionary<ProjectKey, DependencyCollectorByDependentSyntaxTreeAndMasterProject> DependenciesByCompilation
        => this._dependenciesByCompilation;

    public DependencyCollectorByDependentSyntaxTree( string dependentFilePath )
    {
        this.DependentFilePath = dependentFilePath;
    }

    public void AddSyntaxTreeDependency( ProjectKey masterCompilation, string masterFilePath, ulong masterHash )
    {
#if DEBUG
        if ( this.IsReadOnly )
        {
            throw new InvalidOperationException();
        }
#endif

        if ( !this._dependenciesByCompilation.TryGetValue( masterCompilation, out var compilationCollector ) )
        {
            compilationCollector = new DependencyCollectorByDependentSyntaxTreeAndMasterProject( this.DependentFilePath, masterCompilation );
            this._dependenciesByCompilation.Add( masterCompilation, compilationCollector );
        }

        compilationCollector.AddSyntaxTreeDependency( masterFilePath, masterHash );
    }

    public void AddPartialTypeDependency( ProjectKey masterCompilation, TypeDependencyKey masterPartialType )
    {
#if DEBUG
        if ( this.IsReadOnly )
        {
            throw new InvalidOperationException();
        }
#endif

        if ( !this._dependenciesByCompilation.TryGetValue( masterCompilation, out var compilationCollector ) )
        {
            compilationCollector = new DependencyCollectorByDependentSyntaxTreeAndMasterProject( this.DependentFilePath, masterCompilation );
            this._dependenciesByCompilation.Add( masterCompilation, compilationCollector );
        }

        compilationCollector.AddPartialTypeDependency( masterPartialType );
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