﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using K4os.Hash.xxHash;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Caching;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Metalama.Framework.DesignTime;

/// <summary>
/// Represents a unique project in a solution. The implementation is optimized to be cheaply computed from a Compilation,
/// because a Compilation does not hold a reference to its project.
/// </summary>
public sealed class ProjectKey : IEquatable<ProjectKey>
{
    private static readonly ConditionalWeakTable<Compilation, ProjectKey> _cache = new();

    private static readonly ConditionalWeakTable<ParseOptions, StrongBox<ulong>> _preprocessorSymbolHashCodeCache = new();

    // We compare equality of two projects that have the same assembly name by hashing their preprocessor 
    // symbols. There are typically very few compilations of the same assembly name in a solution (one for each different platform)
    // so the change of collision is negligible.

    // ReSharper disable once MemberCanBePrivate.Global (Json)
    public ulong PreprocessorSymbolHashCode { get; }

    public string AssemblyName { get; }

    /// <summary>
    /// Gets a value indicating whether the <see cref="ProjectKey"/> contains a valid hash code. The value can be <c>false</c> in tests
    /// or at design time when the project has no syntax tree.
    /// </summary>
    [JsonIgnore]
    public bool HasHashCode => this.PreprocessorSymbolHashCode != 0;

    private ProjectKey( Compilation compilation )
    {
        this.AssemblyName = compilation.AssemblyName.AssertNotNull();

        var syntaxTrees = ((CSharpCompilation) compilation).SyntaxTrees;

        if ( syntaxTrees.IsDefaultOrEmpty )
        {
            this.PreprocessorSymbolHashCode = 0;
        }
        else
        {
            var parseOptions = syntaxTrees[0].Options;
            this.PreprocessorSymbolHashCode = _preprocessorSymbolHashCodeCache.GetOrAdd( parseOptions, GetPreprocessorSymbolHashCode ).Value;
        }
    }

    private static StrongBox<ulong> GetPreprocessorSymbolHashCode( ParseOptions parseOptions )
    {
        // ProjectKey is a cross-process identifier so we have to use a robust hasher.
        var hasher = new XXH64();

        var preprocessorSymbolNames = parseOptions.PreprocessorSymbolNames;

        if ( preprocessorSymbolNames is ImmutableArray<string> immutableArray )
        {
            // The Roslyn implementation of PreprocessorSymbolNames is an ImmutableArray, so we allocate less memory.

            if ( immutableArray.IsDefaultOrEmpty )
            {
                return new StrongBox<ulong>( 0 );
            }

            foreach ( var symbol in immutableArray )
            {
                hasher.Update( symbol );
            }
        }
        else
        {
            // This is for forward compatibility.

            var hasHashCode = false;

            foreach ( var symbol in preprocessorSymbolNames )
            {
                hasHashCode = true;
                hasher.Update( symbol );
            }

            if ( !hasHashCode )
            {
                return new StrongBox<ulong>( 0 );
            }
        }

        var hashCode = hasher.Digest();

        if ( hashCode == 0 )
        {
            hashCode = 1;
        }

        return new StrongBox<ulong>( hashCode );
    }

    [JsonConstructor]
    private ProjectKey( string assemblyName, ulong preprocessorSymbolHashCode )
    {
        this.AssemblyName = assemblyName;
        this.PreprocessorSymbolHashCode = preprocessorSymbolHashCode;
    }

    public static ProjectKey FromCompilation( Compilation compilation ) => _cache.GetOrAdd( compilation, c => new ProjectKey( c ) );

    internal static ProjectKey CreateTest( string id )
    {
        // We intentionally don't use a zero hash so that we can test serialization roundtrip.
        return new ProjectKey( id, 12345 );
    }

    public bool Equals( ProjectKey? other )
    {
        if ( ReferenceEquals( null, other ) )
        {
            return false;
        }

        if ( ReferenceEquals( this, other ) )
        {
            return true;
        }

        if ( this.AssemblyName != other.AssemblyName )
        {
            return false;
        }

        if ( this.PreprocessorSymbolHashCode != other.PreprocessorSymbolHashCode )
        {
            return false;
        }

        return true;
    }

    public override bool Equals( object? obj )
    {
        if ( ReferenceEquals( null, obj ) )
        {
            return false;
        }

        if ( ReferenceEquals( this, obj ) )
        {
            return true;
        }

        if ( obj.GetType() != this.GetType() )
        {
            return false;
        }

        return this.Equals( (ProjectKey) obj );
    }

    public override int GetHashCode()
    {
        return HashCode.Combine( this.AssemblyName, this.PreprocessorSymbolHashCode );
    }

    public static bool operator ==( ProjectKey? left, ProjectKey? right ) => Equals( left, right );

    public static bool operator !=( ProjectKey? left, ProjectKey? right ) => !Equals( left, right );

    public override string ToString() => $"{this.AssemblyName}, {this.PreprocessorSymbolHashCode:x}";
}