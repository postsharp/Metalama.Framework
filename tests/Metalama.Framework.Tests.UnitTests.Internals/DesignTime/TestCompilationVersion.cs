// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

internal class TestCompilationVersion : ICompilationVersion
{
    private readonly Dictionary<string, ulong> _hashes;

    public TestCompilationVersion(
        AssemblyIdentity assemblyIdentity,
        ulong compileTimeProjectHash = 0,
        Dictionary<string, ulong>? hashes = null,
        ICompilationVersion[]? referencedCompilations = null )
    {
        this._hashes = hashes ?? new Dictionary<string, ulong>();
        this.AssemblyIdentity = assemblyIdentity;
        this.CompileTimeProjectHash = compileTimeProjectHash;
        this.Compilation = CSharpCompilation.Create( assemblyIdentity.Name, hashes?.Select( p => CSharpSyntaxTree.ParseText( "", path: p.Key ) ) );

        this.References = referencedCompilations?.ToImmutableDictionary( c => c.AssemblyIdentity, c => c )
                          ?? ImmutableDictionary<AssemblyIdentity, ICompilationVersion>.Empty;
    }

    public AssemblyIdentity AssemblyIdentity { get; }

    public ulong CompileTimeProjectHash { get; }

    public bool TryGetSyntaxTreeVersion( string path, out SyntaxTreeVersion syntaxTreeVersion )
    {
        if ( this._hashes.TryGetValue( path, out var hash ) )
        {
            syntaxTreeVersion = new SyntaxTreeVersion( null!, false, hash );

            return true;
        }
        else
        {
            syntaxTreeVersion = default;

            return false;
        }
    }

    public Compilation Compilation { get; }

    public IEnumerable<string> EnumerateSyntaxTreePaths() => this._hashes.Keys;

    public ImmutableDictionary<AssemblyIdentity, ICompilationVersion> References { get; }
}