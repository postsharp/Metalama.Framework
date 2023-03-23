// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

/// <summary>
/// A derivation of <see cref="CSharpSyntaxWalker"/> that throws a <see cref="SyntaxProcessingException"/>
/// when an unhandled exception is detected while processing a node.
/// Also prevents <see cref="InsufficientExecutionStackException "/> for deeply nested trees.
/// </summary>
[PublicAPI]
public abstract class SafeSyntaxWalker : CSharpSyntaxWalker
{
    protected SafeSyntaxWalker( SyntaxWalkerDepth depth = SyntaxWalkerDepth.Node ) : base( depth ) { }

    private RecursionGuard _recursionGuard;

    public sealed override void Visit( SyntaxNode? node )
    {
        try
        {
            this._recursionGuard.IncrementDepth();

            if ( this._recursionGuard.ShouldSwitch )
            {
                this._recursionGuard.Switch( node, this.VisitCore );
            }
            else
            {
                this.VisitCore( node );
            }

            this._recursionGuard.DecrementDepth();
        }
        catch ( Exception e ) when ( SyntaxProcessingException.ShouldWrapException( e, node ) )
        {
            throw new SyntaxProcessingException( e, node );
        }
    }

    protected virtual void VisitCore( SyntaxNode? node )
    {
        base.Visit( node );
    }
}