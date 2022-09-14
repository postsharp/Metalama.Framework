// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

internal class NonMetalamaCompilationVersion : ICompilationVersion
{
    private readonly ImmutableDictionary<string, SyntaxTree> _syntaxTrees;
    private readonly Func<SyntaxTree, ulong> _computeHashFunc;

    public AssemblyIdentity AssemblyIdentity { get; }

    public ulong CompileTimeProjectHash => 0;

    public bool TryGetSyntaxTreeVersion( string path, out SyntaxTreeVersion syntaxTreeVersion )
    {
        if ( this._syntaxTrees.TryGetValue( path, out var syntaxTree ) )
        {
            syntaxTreeVersion = new SyntaxTreeVersion( syntaxTree, false, this._computeHashFunc( syntaxTree ) );

            return true;
        }
        else
        {
            syntaxTreeVersion = default;

            return false;
        }
    }

    public Compilation Compilation { get; set; }

    public NonMetalamaCompilationVersion( Compilation compilation, Func<SyntaxTree, ulong> computeHashFunc )
    {
        this.Compilation = compilation;
        this.AssemblyIdentity = compilation.Assembly.Identity;
        this._computeHashFunc = computeHashFunc;
        this._syntaxTrees = compilation.GetIndexedSyntaxTrees();
    }
}