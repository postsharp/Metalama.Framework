// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using K4os.Hash.xxHash;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Metalama.Framework.DesignTime.Pipeline.Diff;

/// <summary>
/// Base for the auto-generated <see cref="RunTimeCodeHasher"/> and <see cref="CompileTimeCodeHasher"/>.
/// Generates a hash that is unique enough under the desired invariants.
/// </summary>
public abstract class BaseCodeHasher : CSharpSyntaxWalker
{
    private readonly XXH64 _hasher;

    protected BaseCodeHasher( XXH64 hasher )
    {
        this._hasher = hasher;
    }

    protected void VisitTrivialToken( SyntaxToken token )
    {
        this._hasher.Update( token.RawKind );
    }

    protected void VisitNonTrivialToken( SyntaxToken token )
    {
        this._hasher.Update( token.Text );
    }

    protected void Visit<T>( in SyntaxList<T> list )
        where T : SyntaxNode
    {
        foreach ( var item in list )
        {
            this.Visit( item );
        }
    }

    protected void Visit<T>( in SeparatedSyntaxList<T> list )
        where T : SyntaxNode
    {
        foreach ( var item in list )
        {
            this.Visit( item );
        }
    }

    protected void Visit( in SyntaxToken token )
    {
        this._hasher.Update( token.Text );
    }

    protected void Visit( in SyntaxTokenList list )
    {
        foreach ( var item in list )
        {
            this.Visit( item );
        }
    }
}