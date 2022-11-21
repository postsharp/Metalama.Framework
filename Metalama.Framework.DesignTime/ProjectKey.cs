// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using K4os.Hash.xxHash;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Caching;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Metalama.Framework.DesignTime;

/// <summary>
/// Represents a unique project in a solution. The implementation is optimized to be cheaply computed from a Compilation,
/// because a Compilation does not hold a reference to its project.
/// </summary>
public static class ProjectKeyFactory
{
    private static readonly WeakCache<Compilation, ProjectKey> _cache = new();
    private static readonly WeakCache<ParseOptions, StrongBox<ulong>> _preprocessorSymbolHashCodeCache = new();

    private static ProjectKey Create( Compilation compilation )
    {
        var assemblyName = compilation.AssemblyName.AssertNotNull();

        var syntaxTrees = ((CSharpCompilation) compilation).SyntaxTrees;

        ulong preprocessorSymbolHashCode;

        bool isMetalamaEnabled;

        if ( syntaxTrees.IsDefaultOrEmpty )
        {
            preprocessorSymbolHashCode = 0;
            isMetalamaEnabled = false;
        }
        else
        {
            var parseOptions = syntaxTrees[0].Options;
            preprocessorSymbolHashCode = _preprocessorSymbolHashCodeCache.GetOrAdd( parseOptions, GetPreprocessorSymbolHashCode ).Value;
            isMetalamaEnabled = parseOptions.PreprocessorSymbolNames.Contains( "METALAMA" );
        }

        return new ProjectKey( assemblyName, preprocessorSymbolHashCode, isMetalamaEnabled );
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

    public static ProjectKey FromCompilation( Compilation compilation ) => _cache.GetOrAdd( compilation, Create );

    internal static ProjectKey CreateTest( string id, bool isMetalamaEnabled = true )
    {
        // We intentionally don't use a zero hash so that we can test serialization roundtrip.
        return new ProjectKey( id, 12345, isMetalamaEnabled );
    }
}