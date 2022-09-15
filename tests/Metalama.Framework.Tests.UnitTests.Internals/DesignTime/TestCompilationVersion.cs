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
        string assemblyIdentity,
        ulong compileTimeProjectHash = 0,
        Dictionary<string, ulong>? hashes = null,
        ICompilationVersion[]? referencedCompilations = null ) : this(
        new AssemblyIdentity( assemblyIdentity ),
        compileTimeProjectHash,
        hashes,
        referencedCompilations ) { }

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

        this.ReferencedCompilations = referencedCompilations?.ToImmutableDictionary( c => c.AssemblyIdentity, c => c )
                                      ?? ImmutableDictionary<AssemblyIdentity, ICompilationVersion>.Empty;
    }

    public TestCompilationVersion( Compilation compilation )
    {
        this.AssemblyIdentity = compilation.Assembly.Identity;
        this.CompileTimeProjectHash = 5;
        this.Compilation = compilation;

        this.ReferencedCompilations = compilation.References.OfType<CompilationReference>()
            .ToImmutableDictionary( r => r.Compilation.Assembly.Identity, c => (ICompilationVersion) new TestCompilationVersion( c.Compilation ) );

#pragma warning disable CA1307
        this._hashes = compilation.SyntaxTrees.ToDictionary( t => t.FilePath, t => (ulong) t.GetRoot().ToFullString().GetHashCode() );
#pragma warning restore CA1307
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

    public ImmutableDictionary<AssemblyIdentity, ICompilationVersion> ReferencedCompilations { get; }
}