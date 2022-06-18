// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using K4os.Hash.xxHash;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;

namespace Metalama.Framework.DesignTime.Pipeline.Diff;

/// <summary>
/// Base for the auto-generated <see cref="RunTimeCodeHasher"/> and <see cref="CompileTimeCodeHasher"/>.
/// Generates a hash that is unique enough under the desired invariants.
/// </summary>
public abstract class BaseCodeHasher : CSharpSyntaxWalker
{
    private readonly XXH64 _hasher;
    public StringBuilder? Log { get; private set; }

    internal void EnableLogging() => this.Log = new StringBuilder();
    
    protected BaseCodeHasher( XXH64 hasher )
    {
        this._hasher = hasher;
    }

    protected void VisitTrivialToken( SyntaxToken token )
    {
        if ( token.RawKind != 0 )
        {
            this._hasher.Update( token.RawKind );
            this.Log?.AppendLine( $"Adding '{token.RawKind}' to the hash." );
        }
    }

    protected void VisitNonTrivialToken( SyntaxToken token )
    {
        this._hasher.Update( token.Text );
        this.Log?.AppendLine( $"Adding '{token.Text}' to the hash." );
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
        this.Log?.AppendLine( $"Adding '{token.Text}' to the hash." );
    }

    protected void Visit( in SyntaxTokenList list )
    {
        foreach ( var item in list )
        {
            this.Visit( item );
        }
    }
}