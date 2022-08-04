// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

internal readonly struct DependencyGraphByDependentSyntaxTree
{
    private static readonly ImmutableHashSet<string> _emptyDependencies = ImmutableHashSet.Create<string>().WithComparer( StringComparer.Ordinal );

    /// <summary>
    /// Gets the file path of the master syntax tree.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets the hash of of the master syntax tree.
    /// </summary>
    public ulong Hash { get; }

    /// <summary>
    /// Gets the list of dependent syntax trees, by their file path.
    /// </summary>
    public ImmutableHashSet<string> DependentFilePaths { get; }

    public DependencyGraphByDependentSyntaxTree( string filePath, ulong hash ) : this( filePath, hash, _emptyDependencies ) { }

    private DependencyGraphByDependentSyntaxTree( string filePath, ulong hash, ImmutableHashSet<string> dependentFilePaths )
    {
        this.FilePath = filePath;
        this.Hash = hash;
        this.DependentFilePaths = dependentFilePaths;
    }

    public DependencyGraphByDependentSyntaxTree AddDependency( string dependentFilePath )
        => new( this.FilePath, this.Hash, this.DependentFilePaths.Add( dependentFilePath ) );

    public DependencyGraphByDependentSyntaxTree RemoveDependency( string dependentFilePath )
        => new( this.FilePath, this.Hash, this.DependentFilePaths.Remove( dependentFilePath ) );
}