// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

internal class TestCompilationVersion : ICompilationVersion
{
    private readonly Dictionary<string, ulong>? _hashes;

    public TestCompilationVersion( AssemblyIdentity assemblyIdentity, ulong compileTimeProjectHash = 0, Dictionary<string, ulong>? hashes = null )
    {
        this._hashes = hashes;
        this.AssemblyIdentity = assemblyIdentity;
        this.CompileTimeProjectHash = compileTimeProjectHash;
    }

    public AssemblyIdentity AssemblyIdentity { get; }

    public ulong CompileTimeProjectHash { get; }

    public bool TryGetSyntaxTreeHash( string path, out ulong hash )
    {
        if ( this._hashes == null )
        {
            hash = 0;

            return false;
        }

        return this._hashes.TryGetValue( path, out hash );
    }
}