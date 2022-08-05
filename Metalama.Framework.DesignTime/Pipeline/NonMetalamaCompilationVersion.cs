// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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

    public NonMetalamaCompilationVersion( Compilation compilation, Func<SyntaxTree, ulong> computeHashFunc )
    {
        this.AssemblyIdentity = compilation.Assembly.Identity;
        this._computeHashFunc = computeHashFunc;
        this._syntaxTrees = compilation.GetIndexedSyntaxTrees();
    }

    public bool TryGetSyntaxTreeDeclarationHash( string path, out ulong hash )
    {
        if ( this._syntaxTrees.TryGetValue( path, out var syntaxTree ) )
        {
            hash = this._computeHashFunc( syntaxTree );

            return true;
        }
        else
        {
            hash = 0;

            return false;
        }
    }

    public bool TryGetSyntaxTreePartialTypesHash( string path, out ulong hash ) => throw new NotImplementedException();
}