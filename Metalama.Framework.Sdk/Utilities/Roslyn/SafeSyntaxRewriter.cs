// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

/// <summary>
/// A derivation of <see cref="CSharpSyntaxRewriter"/> that throws a <see cref="SyntaxProcessingException"/>
/// when an unhandled exception is detected while processing a node. 
/// </summary>
[PublicAPI]
public abstract class SafeSyntaxRewriter : CSharpSyntaxRewriter
{
    protected SafeSyntaxRewriter( bool visitIntoStructuredTrivia = false ) : base( visitIntoStructuredTrivia ) { }

    private RecursionGuard _recursionGuard;

    [return: NotNullIfNotNull( nameof(node) )]
    public sealed override SyntaxNode? Visit( SyntaxNode? node )
    {
        try
        {
            this._recursionGuard.IncrementDepth();

            var result = this._recursionGuard.ShouldSwitch ? this._recursionGuard.Switch( () => this.VisitCore( node ) ) : this.VisitCore( node );

            this._recursionGuard.DecrementDepth();

            return result;
        }
        catch ( Exception e ) when ( SyntaxProcessingException.ShouldWrapException( e, node ) )
        {
            throw new SyntaxProcessingException( e, node );
        }
    }

    protected virtual SyntaxNode? VisitCore( SyntaxNode? node )
    {
        return base.Visit( node );
    }
}