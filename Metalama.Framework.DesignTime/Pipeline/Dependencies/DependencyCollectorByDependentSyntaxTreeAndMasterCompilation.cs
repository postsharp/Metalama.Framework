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
    private int _hashCode;

    public string DependentFilePath { get; }

    public AssemblyIdentity AssemblyIdentity { get; }

    public IReadOnlyDictionary<string, ulong> MasterFilePathsAndHashes => this._masterFilePathsAndHashes;

    public DependencyCollectorByDependentSyntaxTreeAndMasterCompilation( string dependentFilePath, AssemblyIdentity assemblyIdentity )
    {
        this.DependentFilePath = dependentFilePath;
        this.AssemblyIdentity = assemblyIdentity;
    }

    public void AddDependency( string masterFilePath, ulong masterHash )
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

        if ( this._hashCode != other._hashCode || this._masterFilePathsAndHashes.Count != other._masterFilePathsAndHashes.Count )
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

        return true;
    }
}