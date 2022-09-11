// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

internal readonly struct DependencyGraphByMasterSyntaxTree
{
    private static readonly ImmutableHashSet<string> _emptyDependencies = ImmutableHashSet.Create<string>().WithComparer( StringComparer.Ordinal );

    /// <summary>
    /// Gets the file path of the master syntax tree.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets the hash of of the master syntax tree.
    /// </summary>
    public ulong DeclarationHash { get; }

    /// <summary>
    /// Gets the list of dependent syntax trees, by their file path.
    /// </summary>
    public ImmutableHashSet<string> DependentFilePaths { get; }

    public DependencyGraphByMasterSyntaxTree( string filePath, ulong declarationHash ) : this( filePath, declarationHash, _emptyDependencies ) { }

    private DependencyGraphByMasterSyntaxTree( string filePath, ulong declarationHash, ImmutableHashSet<string> dependentFilePaths )
    {
        this.FilePath = filePath;
        this.DeclarationHash = declarationHash;
        this.DependentFilePaths = dependentFilePaths;
    }

    public DependencyGraphByMasterSyntaxTree AddSyntaxTreeDependency( string dependentFilePath )
        => new( this.FilePath, this.DeclarationHash, this.DependentFilePaths.Add( dependentFilePath ) );

    public DependencyGraphByMasterSyntaxTree RemoveDependency( string dependentFilePath )
        => new( this.FilePath, this.DeclarationHash, this.DependentFilePaths.Remove( dependentFilePath ) );
}