﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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

    // We compare equality of two projects that have the same assembly name by hashing their preprocessor 
    // symbols. There are typically very few compilations of the same assembly name in a solution (one for each different platform)
    // so the change of collision is negligible.

    public string AssemblyName { get; }

    public ulong PreprocessorSymbolHashCode { get; }

    public bool HasMetalama { get; }

    private string? _asString;

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
            var preprocessorSymbols = syntaxTrees[0].Options.PreprocessorSymbolNames;

            // ProjectKey is a cross-process identifier so we have to use a robust hasher.
            var hasher = new XXH64();

            if ( preprocessorSymbols is ImmutableArray<string> immutableArray )
            {
                // The Roslyn implementation of PreprocessorSymbolNames is an ImmutableArray, so we allocate less memory.

                foreach ( var symbol in immutableArray )
                {
                    hasher.Update( symbol );

                    if ( symbol == "METALAMA" )
                    {
                        this.HasMetalama = true;
                    }
                }
            }
            else
            {
                // This is for forward compatibility.

                foreach ( var symbol in immutableArray )
                {
                    hasher.Update( symbol );

                    if ( symbol == "METALAMA" )
                    {
                        this.HasMetalama = true;
                    }
                }
            }

            this.PreprocessorSymbolHashCode = hasher.Digest();
        }
    }

    [JsonConstructor]
    private ProjectKey( string assemblyName, ulong preprocessorSymbolHashCode, bool hasMetalama )
    {
        this.AssemblyName = assemblyName;
        this.PreprocessorSymbolHashCode = preprocessorSymbolHashCode;
        this.HasMetalama = hasMetalama;
    }

    public static ProjectKey FromCompilation( Compilation compilation ) => _cache.GetOrAdd( compilation, c => new ProjectKey( c ) );

    internal static ProjectKey CreateTest( string id )
    {
        return new ProjectKey( id, 0, true );
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

    public override string ToString() => this._asString ??= $"{this.AssemblyName},{this.PreprocessorSymbolHashCode:x},{this.HasMetalama}";
}