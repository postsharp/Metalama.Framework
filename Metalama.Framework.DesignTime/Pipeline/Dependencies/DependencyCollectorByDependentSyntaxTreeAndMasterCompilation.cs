// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

/// <summary>
/// Collects the dependencies of a given dependent syntax tree in a given compilation.
/// </summary>
internal class DependencyCollectorByDependentSyntaxTreeAndMasterCompilation
{
    private readonly Dictionary<string, ulong> _masterFilePathsAndHashes = new( StringComparer.Ordinal );
    private readonly HashSet<TypeDependencyKey> _masterPartialTypes = new();
    private int _hashCode;

    public string DependentFilePath { get; }

    public AssemblyIdentity AssemblyIdentity { get; }

    public IReadOnlyDictionary<string, ulong> MasterFilePathsAndHashes => this._masterFilePathsAndHashes;

    public IReadOnlyCollection<TypeDependencyKey> MasterPartialTypes => this._masterPartialTypes;

    public bool Contains( TypeDependencyKey type ) => this._masterPartialTypes.Contains( type );

    public DependencyCollectorByDependentSyntaxTreeAndMasterCompilation( string dependentFilePath, AssemblyIdentity assemblyIdentity )
    {
        this.DependentFilePath = dependentFilePath;
        this.AssemblyIdentity = assemblyIdentity;
    }

    public void AddSyntaxTreeDependency( string masterFilePath, ulong masterHash )
    {
#if DEBUG
        if ( this.IsReadOnly )
        {
            throw new InvalidOperationException();
        }
#endif

        if ( !this._masterFilePathsAndHashes.TryGetValue( masterFilePath, out var existingHash ) )
        {
            this._masterFilePathsAndHashes.Add( masterFilePath, masterHash );
            this._hashCode ^= HashCode.Combine( masterFilePath, masterHash );
        }
        else if ( existingHash != masterHash )
        {
            throw new AssertionFailedException();
        }
    }

    public void AddPartialTypeDependency( TypeDependencyKey masterPartialType )
    {
#if DEBUG
        if ( this.IsReadOnly )
        {
            throw new InvalidOperationException();
        }
#endif

        if ( this._masterPartialTypes.Add( masterPartialType ) )
        {
            this._hashCode ^= masterPartialType.GetHashCode();
        }
    }

#if DEBUG
    public bool IsReadOnly { get; private set; }

    public void Freeze()
    {
        this.IsReadOnly = true;
    }
#endif

    public bool IsStructurallyEqual( DependencyCollectorByDependentSyntaxTreeAndMasterCompilation other )
    {
        if ( ReferenceEquals( this, other ) )
        {
            return true;
        }

        if ( this._hashCode != other._hashCode
             || this._masterFilePathsAndHashes.Count != other._masterFilePathsAndHashes.Count
             || this._masterPartialTypes.Count != other._masterPartialTypes.Count )
        {
            return false;
        }

        foreach ( var dependency in this._masterFilePathsAndHashes )
        {
            if ( !other._masterFilePathsAndHashes.TryGetValue( dependency.Key, out var otherHash ) || otherHash != dependency.Value )
            {
                return false;
            }
        }

        foreach ( var dependency in this._masterPartialTypes )
        {
            if ( !other._masterPartialTypes.Contains( dependency ) )
            {
                return false;
            }
        }

        return true;
    }
}