// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

internal readonly struct DependencyGraphByMasterPartialType
{
    private static readonly ImmutableHashSet<string> _emptyDependencies = ImmutableHashSet.Create<string>().WithComparer( StringComparer.Ordinal );

    public DependencyGraphByMasterPartialType( TypeDependencyKey masterPartialType ) : this( masterPartialType, _emptyDependencies ) { }

    public DependencyGraphByMasterPartialType RemoveDependency( string dependentFilePath )
        => new( this.MasterPartialType, this.DependentFilePaths.Remove( dependentFilePath ) );

    public TypeDependencyKey MasterPartialType { get; }

    public ImmutableHashSet<string> DependentFilePaths { get; }

    private DependencyGraphByMasterPartialType( TypeDependencyKey masterPartialType, ImmutableHashSet<string> dependentFilePaths )
    {
        this.MasterPartialType = masterPartialType;
        this.DependentFilePaths = dependentFilePaths;
    }

    public DependencyGraphByMasterPartialType AddPartialTypeDependency( string dependentFilePath )
        => new( this.MasterPartialType, this.DependentFilePaths.Add( dependentFilePath ) );
}