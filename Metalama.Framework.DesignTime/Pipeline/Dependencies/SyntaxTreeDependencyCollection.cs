// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

internal class SyntaxTreeDependencyCollection
{
    private readonly HashSet<string> _dependencies = new( StringComparer.Ordinal );

    /// <summary>
    /// Gets the file path of the master syntax tree.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets the hash of of the master syntax tree.
    /// </summary>
    public ulong Hash { get; private set; }

    /// <summary>
    /// Gets the list of dependent syntax trees, by their file path.
    /// </summary>
    public IReadOnlyCollection<string> Dependencies => this._dependencies;

    public SyntaxTreeDependencyCollection( string filePath, ulong hash )
    {
        this.FilePath = filePath;
        this.Hash = hash;
    }

    public void AddDependency( string dependency )
    {
        this._dependencies.Add( dependency );
    }
}